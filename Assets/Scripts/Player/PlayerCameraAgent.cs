using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어의 시야 및 카메라 타겟팅을 전담하는 독립 모듈입니다.
/// Player 클래스와 분리되어 카메라 연출(피킹, 관전 등)만을 관리합니다.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerCameraAgent : NetworkBehaviour
{
    [Header("카메라 연출 설정")]
    [SerializeField] private float _peekDistance = 3.0f; // 마우스 방향 피킹 거리
    [SerializeField] private float _peekSmoothTime = 0.1f; // 카메라 이동 부드러움

    // 카메라가 추적할 가상의 타겟 (플레이어 자식)
    private Transform _cameraTarget;
    private Vector3 _targetLocalPosVelocity;

    // 의존성
    private Player _player;

    // 상태 변수 (Input 이벤트에 의해 제어됨)
    private bool _isWatchingCore = false;
    private bool _isPeeking = false;
    private bool _isCameraInitialized;

    private void Awake()
    {
        _player = GetComponent<Player>();

        // 가상 타겟 생성
        GameObject targetGo = new GameObject("CameraTarget_Holder");
        targetGo.transform.SetParent(transform);
        targetGo.transform.localPosition = Vector3.zero;
        _cameraTarget = targetGo.transform;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false; // 내 캐릭터가 아니면 업데이트 불필요
            return;
        }

        // 초기 타겟 설정
        TryConnectCamera();
        
        // 플레이어 사망 이벤트 구독
        Player.OnLocalPlayerDeadStateChanged += OnLocalPlayerDead;

        // 입력 액션 바인딩
        Managers.Input.BindAction("ViewCore", OnViewCoreInput, InputActionPhase.Started);
        Managers.Input.BindAction("ViewCore", OnViewCoreInput, InputActionPhase.Canceled);

        Managers.Input.BindAction("Peek", OnPeekInput, InputActionPhase.Started);
        Managers.Input.BindAction("Peek", OnPeekInput, InputActionPhase.Canceled);
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Player.OnLocalPlayerDeadStateChanged -= OnLocalPlayerDead;

            if (Managers.Inst != null)
            {
                Managers.Input.UnbindAction("ViewCore", OnViewCoreInput, InputActionPhase.Started);
                Managers.Input.UnbindAction("ViewCore", OnViewCoreInput, InputActionPhase.Canceled);
                Managers.Input.UnbindAction("Peek", OnPeekInput, InputActionPhase.Started);
                Managers.Input.UnbindAction("Peek", OnPeekInput, InputActionPhase.Canceled);
            }
        }
    }

    private void Update()
    {
        // 사망 상태라면 카메라 로직 중단 (관전 모드는 이벤트로 처리됨)
        // Player 클래스에 IsDead() 접근자가 없다면 프로퍼티 추가 필요 (아래 Player 코드 참조)
        if (_player.Hp <= 0) return;

        if (!_isCameraInitialized)
            TryConnectCamera();

        MoveCameraTarget();
    }

    /// <summary>
    /// 카메라 연결을 시도하고, 성공하면 초기화 플래그를 true로 설정합니다.
    /// </summary>
    private void TryConnectCamera()
    {
        // 매니저가 없거나, 활성 카메라가 아직 설정되지 않았다면 리턴
        if (Managers.Camera == null || Managers.Camera.GetActiveCamera() == null)
            return;

        // 연결 성공
        Managers.Camera.SetCurrentCameraTarget(_cameraTarget);
        _isCameraInitialized = true;

        Debug.Log("[PlayerCameraAgent] 카메라 타겟 연결 성공");
    }


    // ========================================================================
    // Input Callbacks
    // ========================================================================

    /// <summary>
    /// 코어 보기(Space) 입력 처리
    /// </summary>
    private void OnViewCoreInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            _isWatchingCore = true;
            if (Core.Instance != null)
                Managers.Camera.SetCurrentCameraTarget(Core.Instance.transform);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            _isWatchingCore = false;
            // 키를 떼면 다시 내 시점(_cameraTarget)으로 복귀
            Managers.Camera.SetCurrentCameraTarget(_cameraTarget);
        }
    }

    /// <summary>
    /// 마우스 피킹(우클릭) 입력 처리
    /// </summary>
    private void OnPeekInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
            _isPeeking = true;
        else if (context.phase == InputActionPhase.Canceled)
            _isPeeking = false;
    }

    // ========================================================================
    // Camera Logic
    // ========================================================================

    /// <summary>
    /// 마우스 위치 및 상태에 따른 카메라 타겟 이동 계산
    /// </summary>
    private void MoveCameraTarget()
    {
        // 코어를 보고 있다면 피킹 로직 무시
        if (_isWatchingCore) return;

        Vector3 targetLocalPos = Vector3.zero;

        if (_isPeeking)
        {
            // [Manager 활용] 좌표 변환
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPos = Managers.Camera.ScreenToWorldPoint(mouseScreenPos);
            Vector2 direction = (mouseWorldPos - (Vector2)transform.position);

            // 거리 제한
            targetLocalPos = Vector2.ClampMagnitude(direction, _peekDistance);
        }

        // SmoothDamp로 부드럽게 이동 (복귀 포함)
        _cameraTarget.localPosition = Vector3.SmoothDamp(
            _cameraTarget.localPosition,
            targetLocalPos,
            ref _targetLocalPosVelocity,
            _peekSmoothTime
        );
    }

    private void OnLocalPlayerDead(bool isDead)
    {
        if (isDead)
            SpectateTeammate();
    }

    private void SpectateTeammate()
    {
        if (NetworkManager.Singleton == null) return;

        var otherPlayer = NetworkManager.Singleton.ConnectedClientsList
            .Select(c => c.PlayerObject.GetComponent<Player>())
            .FirstOrDefault(p => p != _player && p != null && p.gameObject.activeInHierarchy && p.Hp > 0);

        if (otherPlayer != null)
        {
            Debug.Log($"[CameraAgent] {otherPlayer.name}을 관전합니다.");
            Managers.Camera.SetCurrentCameraTarget(otherPlayer.transform);
        }
        else
        {
            Debug.Log("[CameraAgent] 관전할 팀원이 없어 코어를 비춥니다.");
            if (Core.Instance != null)
                Managers.Camera.SetCurrentCameraTarget(Core.Instance.transform);
        }
    }
}

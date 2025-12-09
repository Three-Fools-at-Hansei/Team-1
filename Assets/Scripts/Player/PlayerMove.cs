using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : NetworkBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    [Header("참조")]
    [SerializeField] private Player _player;

    private Rigidbody2D _rigid;
    private Vector2 _moveInput;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Camera _mainCamera; // 로컬에서만 사용

    // [수정 1] 조준 방향을 네트워크 변수로 동기화 (Owner가 쓰기 권한 가짐)
    private readonly NetworkVariable<Vector2> _netAimDirection = new NetworkVariable<Vector2>(
        Vector2.right,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // 마우스 조준 방향을 네트워크 변수로 동기화 (Owner가 쓰기 권한 가짐)
    private readonly NetworkVariable<Vector2> _netMouseAimDirection = new NetworkVariable<Vector2>(
        Vector2.right,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // 로컬에서 계산된 또는 네트워크에서 받은 최종 방향
    private Vector2 _currentAimDirection = Vector2.right;

    /// <summary>
    /// 동기화된 마우스 조준 방향을 반환합니다.
    /// </summary>
    public Vector2 CurrentMouseAimDirection => _netMouseAimDirection.Value;

    /// <summary>
    /// 마우스 조준 방향을 업데이트합니다 (Owner만 호출 가능).
    /// </summary>
    public void UpdateMouseAimDirection(Vector2 direction)
    {
        if (IsOwner)
        {
            _netMouseAimDirection.Value = direction;
        }
    }

    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mainCamera = Camera.main;

        if (_player == null)
            _player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Managers.Input.BindAction("Player", HandleMove, InputActionPhase.Performed);
            Managers.Input.BindAction("Player", HandleMove, InputActionPhase.Canceled);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            if (Managers.Inst != null)
            {
                Managers.Input.UnbindAction("Player", HandleMove, InputActionPhase.Performed);
                Managers.Input.UnbindAction("Player", HandleMove, InputActionPhase.Canceled);
            }
        }
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        _moveInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        // [수정 2] 조준 방향 업데이트 로직 분리
        UpdateAimDirection();
        UpdateVisuals();

        if (_player != null)
        {
            // 동기화된 방향을 Player/Weapon에 전달
            _player.UpdateAimDirection(_currentAimDirection);

            // Attack은 내부적으로 Input을 체크하므로 호출 유지
            // (Player.cs의 Attack 로직이 IsOwner 체크를 하는지 확인 필요)
            if (IsOwner)
                _player.Attack();
        }
    }

    void FixedUpdate()
    {
        // 물리 이동은 Owner가 직접 수행 (ClientNetworkTransform이 결과 동기화)
        if (IsOwner)
        {
            _rigid.linearVelocity = _moveInput * moveSpeed;
        }
        // IsOwner가 아닐 때는 ClientNetworkTransform이 위치를 보간해주므로
        // 별도의 velocity 설정이 필요 없습니다.
    }

    private void UpdateAimDirection()
    {
        if (IsOwner)
        {
            // Owner는 입력값 기반으로 방향 결정
            if (_moveInput.magnitude > 0.1f)
            {
                _currentAimDirection = _moveInput.normalized;
            }
            // (옵션) 멈췄을 때 마지막 방향 유지하려면 else 블록 제거
            // else if (_currentAimDirection.magnitude < 0.1f)
            // {
            //     _currentAimDirection = Vector2.right;
            // }

            // [중요] 변경된 값을 네트워크 변수에 반영
            if (_netAimDirection.Value != _currentAimDirection)
            {
                _netAimDirection.Value = _currentAimDirection;
            }
        }
        else
        {
            // 다른 클라이언트는 네트워크 변수 값을 받아옴
            _currentAimDirection = _netAimDirection.Value;
        }
    }

    private void UpdateVisuals()
    {
        // [수정 3] 애니메이션 및 플립 처리는 모든 클라이언트에서 실행
        // 단, IsMoving 파라미터는 ClientNetworkAnimator가 있다면 자동으로 동기화되지만,
        // 없을 경우를 대비해 위치 변화량이나 Aim을 기준으로 처리할 수도 있습니다.

        // 여기서는 Owner가 Animator 파라미터를 설정하면 
        // ClientNetworkAnimator가 이를 다른 클라이언트에 전파한다고 가정합니다.

        if (IsOwner && _animator != null)
        {
            bool isMoving = _moveInput.magnitude > 0.1f;
            _animator.SetBool("IsMoving", isMoving);
        }

        // 스프라이트 뒤집기는 동기화된 _currentAimDirection을 기준
        if (_spriteRenderer != null)
        {
            // 왼쪽을 보고 있다면 flipX = true
            _spriteRenderer.flipX = _currentAimDirection.x < 0;
        }
    }
}
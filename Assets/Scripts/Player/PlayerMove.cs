using Unity.Netcode; // [추가] NGO 네임스페이스
using UnityEngine;
using UnityEngine.InputSystem;

// [변경] MonoBehaviour -> NetworkBehaviour
public class PlayerMove : NetworkBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D _rigid;
    private Vector2 _moveInput;

    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    // [변경] Start -> OnNetworkSpawn (네트워크 객체가 생성될 때 호출됨)
    public override void OnNetworkSpawn()
    {
        // IsOwner: 이 캐릭터가 '나'의 것인지 확인
        if (IsOwner)
        {
            Managers.Input.BindAction("Player", HandleMove, InputActionPhase.Performed);
            Managers.Input.BindAction("Player", HandleMove, InputActionPhase.Canceled);

            // 카메라는 내 캐릭터를 따라가야 합니다.
            // Managers.Camera 기능이 구현되어 있다면 여기서 타겟 설정을 합니다.
            // 예: Camera.main.GetComponent<CameraFollow>()?.SetTarget(transform);
        }
    }

    // [변경] OnDestroy -> OnNetworkDespawn (네트워크 객체가 사라질 때 호출됨)
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
        // 내 캐릭터가 아니면 입력을 무시 (이중 안전장치)
        if (!IsOwner) return;

        _moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // 내 캐릭터만 직접 이동 (다른 클라이언트에서는 NetworkTransform이 위치를 동기화함)
        if (IsOwner)
        {
            _rigid.linearVelocity = _moveInput * moveSpeed;
        }
    }
}
//using UnityEngine;
//using UnityEngine.InputSystem; // 새 입력 시스템

//public class PlayerMove : MonoBehaviour
//{
//    public float moveSpeed = 5f;
//    private Rigidbody2D _rigid;
//    private Vector2 _moveInput;

//    void Awake()
//    {
//        _rigid = GetComponent<Rigidbody2D>();
//    }


//    // Input System의 입력 이벤트 (Input Action에서 호출)
//    public void OnMove(InputAction.CallbackContext context)
//    {
//        _moveInput = context.ReadValue<Vector2>();
//    }

//    void FixedUpdate()
//    {
//        _rigid.linearVelocity = _moveInput * moveSpeed;
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D _rigid;
    private Vector2 _moveInput;

    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 게임이 시작되면 "Gameplay" 액션맵으로 전환합니다.

        // "Move" 액션에 HandleMove 콜백 함수를 등록(바인딩)합니다.
        // 키가 눌렸을 때(performed)와 떨어졌을 때(canceled) 모두 신호를 받도록 등록합니다.
        Managers.Input.BindAction("Player", HandleMove, InputActionPhase.Performed);
        Managers.Input.BindAction("Player", HandleMove, InputActionPhase.Canceled);
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 등록했던 콜백을 반드시 해제해야 메모리 누수를 막을 수 있습니다.
        // 게임 종료 시 Managers가 먼저 파괴될 수 있으므로 null 체크를 해주는 것이 안전합니다.
        if (Managers.Inst != null)
        {
            Managers.Input.UnbindAction("Player", HandleMove, InputActionPhase.Performed);
            Managers.Input.UnbindAction("Player", HandleMove, InputActionPhase.Canceled);
        }
    }

    // InputManager가 "Move" 액션의 입력 신호를 보내줄 함수입니다.
    private void HandleMove(InputAction.CallbackContext context)
    {
        // context에서 Vector2 값을 읽어와 _moveInput 변수에 저장합니다.
        // 키를 떼면(canceled) Vector2.zero 값이 들어옵니다.
        _moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        _rigid.linearVelocity = _moveInput * moveSpeed;
    }
}
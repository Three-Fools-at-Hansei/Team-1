using UnityEngine;
using UnityEngine.InputSystem; // 새 입력 시스템

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D _rigid;
    private Vector2 _moveInput;

    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }


    // Input System의 입력 이벤트 (Input Action에서 호출)
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        _rigid.linearVelocity = _moveInput * moveSpeed;
    }
}

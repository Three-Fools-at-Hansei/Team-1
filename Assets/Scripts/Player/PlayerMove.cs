using UnityEngine;
using UnityEngine.InputSystem; // �� �Է� �ý���

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D _rigid;
    private Vector2 _moveInput;

    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }


    // Input System�� �Է� �̺�Ʈ (Input Action���� ȣ��)
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        _rigid.linearVelocity = _moveInput * moveSpeed;
    }
}

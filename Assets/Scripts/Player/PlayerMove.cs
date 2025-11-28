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
    private Camera _mainCamera;
    private Vector2 _aimDirection = Vector2.right;

    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mainCamera = Camera.main;

        if (_player == null)
        {
            _player = GetComponent<Player>();
        }
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
        UpdateAimDirection();

        if (_player != null)
        {
            _player.UpdateAimDirection(_aimDirection);
            _player.Attack();
        }
    }

    void FixedUpdate()
    {
        // [Main 브랜치 적용] 내 캐릭터(Owner)만 물리 힘을 가함
        // NetworkTransform이 없는 다른 클라이언트의 위치는 동기화 받음
        if (IsOwner)
        {
            _rigid.linearVelocity = _moveInput * moveSpeed;
        }

        // [HEAD 브랜치 적용] 애니메이션 제어
        // 주의: _moveInput은 Owner에게만 값이 존재하므로, 현재 로직으로는
        // Owner 화면에서만 애니메이션이 작동합니다. (아래 Step-by-Step 분석 참고)
        if (_animator != null)
        {
            // 이동 중인지 확인
            bool isMoving = _moveInput.magnitude > 0.1f;
            _animator.SetBool("IsMoving", isMoving);

            // 이동 방향에 따라 스프라이트 뒤집기
            if (isMoving && _spriteRenderer != null)
            {
                _spriteRenderer.flipX = _moveInput.x < 0;
            }
        }
    }

    /// <summary>
    /// [HEAD 브랜치 복구] 조준 방향 업데이트
    /// </summary>
    private void UpdateAimDirection()
    {
        if (_moveInput.magnitude > 0.1f)
        {
            _aimDirection = _moveInput.normalized;
        }
        else if (_aimDirection.magnitude < 0.1f)
        {
            _aimDirection = Vector2.right;
        }
    }
}
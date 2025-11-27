using UnityEngine;

/// <summary>
/// 적 캐릭터
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : Entity
{
    [Header("타겟 설정")]
    [SerializeField] private Transform _coreTarget;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] private float _stopDistance = 0.1f;

    private Rigidbody2D _rigidbody;
    private Player _player;
    private Transform _currentTarget;
    private float _lastAttackTime;
    private Animator _animator;
    private bool _isDead;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        CacheTargets();
    }

    private void Update()
    {
        CacheTargets();
        UpdateTarget();
        MoveTowardsTarget();
        TryAttack();
    }

    private void CacheTargets()
    {
        if (_coreTarget == null && Core.Instance != null)
        {
            _coreTarget = Core.Instance.transform;
            if (_currentTarget == null)
            {
                _currentTarget = _coreTarget;
            }
        }

        if (_player == null)
        {
            _player = FindObjectOfType<Player>();
        }
    }

    private void UpdateTarget()
    {
        if (_player != null)
        {
            float playerDistance = Vector2.Distance(transform.position, _player.transform.position);
            if (playerDistance <= _detectionRange)
            {
                _currentTarget = _player.transform;
                return;
            }
        }

        _currentTarget = _coreTarget;
    }

    private void MoveTowardsTarget()
    {
        if (_currentTarget == null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector2)_currentTarget.position - (Vector2)transform.position);

        if (direction.magnitude <= _stopDistance)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            UpdateAnimator(false);
            return;
        }

        direction.Normalize();
        _rigidbody.linearVelocity = direction * _moveSpeed;
        UpdateAnimator(true);
    }

    private void UpdateAnimator(bool isMoving)
    {
        if (_animator == null) return;
        _animator.SetBool("IsMoving", isMoving);
    }

    private void TryAttack()
    {
        if (_currentTarget == null)
            return;

        float distance = Vector2.Distance(transform.position, _currentTarget.position);

        if (distance <= _attackRange && Time.time >= _lastAttackTime + _attackCooldown)
        {
            Attack();
            _lastAttackTime = Time.time;
        }
    }

    public override void Attack()
    {
        if (_currentTarget == null)
            return;

        Entity targetEntity = _currentTarget.GetComponent<Entity>();
        targetEntity?.TakeDamage(_attackPower);
    }

    public override void TakeDamage(int damage)
    {
        if (IsDead())
            return;

        _hp = Mathf.Max(0, _hp - damage);
        _animator?.SetTrigger("Hit");
        UpdateHealthBar();

        if (IsDead())
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        Debug.Log("[Enemy] 적이 사망했습니다.");
        _rigidbody.linearVelocity = Vector2.zero;
        UpdateAnimator(false);
        _animator?.SetTrigger("Die");

        if (TryGetComponent(out Poolable poolable) && Managers.Pool != null)
        {
            Managers.Pool.Despawn(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}



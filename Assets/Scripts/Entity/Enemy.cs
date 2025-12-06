using UnityEngine;
using Unity.Netcode;

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

    /// <summary>
    /// 오브젝트 풀에서 재사용될 때 상태를 초기화합니다.
    /// </summary>
    private void OnEnable()
    {
        _isDead = false;
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }

        // 필요한 경우 스탯 초기화 로직 추가 (예: _hp = _maxHp 등)
        // 현재 구조상 Spawn 직후 별도 Init 호출이 권장되므로 여기서는 플래그와 물리 상태만 리셋합니다.
    }

    private void Start()
    {
        CacheTargets();
    }

    private void Update()
    {
        // 사망 시 행동 중지
        if (_isDead) return;

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
        if (_currentTarget == null || _isDead)
            return;

        Entity targetEntity = _currentTarget.GetComponent<Entity>();
        targetEntity?.TakeDamage(_attackPower);
    }

    public override void TakeDamage(int damage)
    {
        // [중요] 데미지 판정과 사망 처리는 서버에서만 수행합니다.
        if (!IsServer || _isDead)
            return;

        _hp = Mathf.Max(0, _hp - damage);

        // 피격 애니메이션 트리거 (NetworkAnimator가 있다면 자동 동기화됨)
        _animator?.SetTrigger("Hit");

        // 체력바 갱신
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

        // [핵심 수정] 서버에서 Despawn을 호출하면, 
        // 등록된 NetworkObjectPool 핸들러를 통해 로컬 PoolManager.Despawn이 실행됩니다.
        if (IsServer && IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}



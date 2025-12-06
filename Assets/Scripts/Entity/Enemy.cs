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
    }

    private void Start()
    {
        CacheTargets();
    }

    private void Update()
    {
        // 사망 시 행동 중지
        if (_isDead) return;

        // 서버에서만 AI 로직 수행 (위치는 NetworkTransform으로 동기화)
        if (!IsServer) return;

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
        _rigidbody.linearVelocity = direction * MoveSpeed; // NetMoveSpeed 사용 (프로퍼티 연결됨)
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
        targetEntity?.TakeDamage(AttackPower);
    }

    public override void TakeDamage(int damage)
    {
        // 서버에서만 데미지 처리
        if (!IsServer || _isDead)
            return;

        // [수정] NetworkVariable 프로퍼티 사용 -> 값 변경 시 자동 동기화 및 UI 갱신
        Hp = Mathf.Max(0, Hp - damage);

        _animator?.SetTrigger("Hit");

        if (IsDead())
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log("[Enemy] 적이 사망했습니다.");

        _rigidbody.linearVelocity = Vector2.zero;
        UpdateAnimator(false);
        _animator?.SetTrigger("Die");

        // 서버에서 Despawn을 호출하면, 
        // 등록된 NetworkObjectPool 핸들러를 통해 로컬 PoolManager.Despawn이 실행됩니다.
        if (IsServer && IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}


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
    private Transform _currentTarget;
    private float _lastAttackTime;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isDead;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        transform.rotation = Quaternion.identity;
    }

    private void Start()
    {
        // Core 타겟은 처음에 한 번만 찾아서 캐싱 (Core는 하나뿐이므로)
        if (_coreTarget == null && Core.Instance != null)
        {
            _coreTarget = Core.Instance.transform;
        }
    }

    private void Update()
    {
        // 사망 시 행동 중지
        if (_isDead) return;

        // 서버에서만 AI 로직 수행
        if (!IsServer) return;

        // 타겟 갱신 (매 프레임 혹은 코루틴으로 최적화 가능)
        UpdateTarget();

        // 이동 및 공격
        MoveTowardsTarget();
        TryAttack();
    }

    /// <summary>
    /// 가장 가까운 생존 플레이어 또는 코어를 타겟으로 설정합니다.
    /// </summary>
    private void UpdateTarget()
    {
        // 1. 가장 가까운 플레이어 찾기
        Player closestPlayer = FindClosestPlayer();

        // 2. 플레이어가 감지 범위 내에 있다면 타겟으로 설정
        if (closestPlayer != null)
        {
            _currentTarget = closestPlayer.transform;
        }
        else
        {
            // 3. 플레이어가 없거나 멀면 코어를 타겟으로 설정
            _currentTarget = _coreTarget;
        }
    }

    /// <summary>
    /// 접속 중인 클라이언트 중, 살아서 범위 내에 있는 가장 가까운 플레이어를 반환합니다.
    /// </summary>
    private Player FindClosestPlayer()
    {
        if (NetworkManager.Singleton == null) return null;

        Player closest = null;
        float minDistance = _detectionRange; // 감지 범위보다 멀면 무시

        // 모든 연결된 클라이언트 순회
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            var player = client.PlayerObject.GetComponent<Player>();

            // 플레이어가 없거나, 죽었거나(비활성), HP가 0 이하라면 무시
            if (player == null || !player.gameObject.activeInHierarchy || player.Hp <= 0)
                continue;

            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player;
            }
        }

        return closest;
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

        _rigidbody.linearVelocity = direction * MoveSpeed;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = direction.x < 0;
        }

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

        // NetworkVariable 프로퍼티 사용 -> 값 변경 시 자동 동기화 및 UI 갱신
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


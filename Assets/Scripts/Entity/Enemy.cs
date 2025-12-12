using System.Collections;
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

    // [추가] 사망 애니메이션 대기 시간
    [Header("사망 설정")]
    [SerializeField] private float _deathDelay = 1.0f;

    private Rigidbody2D _rigidbody;
    private Collider2D _collider; // [추가] 충돌체 캐싱
    private Transform _currentTarget;
    private float _lastAttackTime;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private bool _isDead;

    private readonly NetworkVariable<bool> _netIsFacingLeft = new NetworkVariable<bool>(false);

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>(); // [추가] 컴포넌트 가져오기
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            ApplyGlobalDebuffs();
        }

        // [추가] 네트워크 변수 변경 시 호출될 콜백 연결
        _netIsFacingLeft.OnValueChanged += OnFacingChanged;

        // 초기 상태 적용 (접속 시점의 방향 동기화)
        UpdateSpriteFlip(_netIsFacingLeft.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _netIsFacingLeft.OnValueChanged -= OnFacingChanged;
    }

    /// <summary>
    /// 데이터 테이블 정보를 바탕으로 몬스터 스탯을 초기화합니다.
    /// 서버에서 스폰 직후 호출해야 합니다. (EnemySpawner에서 호출)
    /// </summary>
    public void Init(MonsterGameData data)
    {
        if (!IsServer) return; // 데이터 초기화는 서버 권한

        // Entity의 NetworkVariable 프로퍼티에 값 할당 -> 자동 동기화
        MaxHp = data.hp;
        Hp = data.hp;
        AttackPower = data.attack;
        MoveSpeed = data.moveSpeed;
        AttackSpeed = data.attackSpeed;

        // 사거리 설정 (Entity protected 필드)
        _attackRange = data.attackRange;

        // 초기화된 스탯에 전역 디버프(약화 효과) 다시 적용
        ApplyGlobalDebuffs();
    }

    private void UpdateSpriteFlip(bool isLeft)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = isLeft;
        }
    }

    /// <summary>
    /// 네트워크 변수 값이 변경되면 호출됩니다. (모든 클라이언트)
    /// </summary>
    private void OnFacingChanged(bool previous, bool current)
    {
        UpdateSpriteFlip(current);
    }

    /// <summary>
    /// 오브젝트 풀에서 재사용될 때 상태를 초기화합니다.
    /// </summary>
    private void OnEnable()
    {
        _isDead = false;

        // [추가] 재사용 시 충돌체 다시 활성화
        if (_collider != null)
        {
            _collider.enabled = true;
        }

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

    private void ApplyGlobalDebuffs()
    {
        if (CombatGameManager.Instance == null)
            return;

        // 1. 최대 체력 보정
        float hpMult = CombatGameManager.Instance.EnemyHpMultiplier.Value;
        if (hpMult < 1.0f)
        {
            int newMaxHp = Mathf.FloorToInt(MaxHp * hpMult);
            // Entity의 프로퍼티를 통해 설정 (NetworkVariable 자동 동기화)
            MaxHp = newMaxHp;
            Hp = newMaxHp; // 체력도 함께 조정
        }

        // 2. 공격력 보정
        float atkMult = CombatGameManager.Instance.EnemyAtkMultiplier.Value;
        if (atkMult < 1.0f)
        {
            int newAtk = Mathf.FloorToInt(AttackPower * atkMult);
            AttackPower = Mathf.Max(1, newAtk);
        }

        // 3. 이동속도 보정
        float speedMult = CombatGameManager.Instance.EnemySpeedMultiplier.Value;
        if (speedMult < 1.0f)
        {
            MoveSpeed *= speedMult;
        }
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

        if (IsServer)
        {
            bool isLeft = direction.x < 0;
            // 값이 다를 때만 변경 (네트워크 트래픽 최적화)
            if (_netIsFacingLeft.Value != isLeft)
            {
                _netIsFacingLeft.Value = isLeft;
            }
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

        // 공격 사운드 재생
        PlayAttackSoundClientRpc();
    }

    [ClientRpc]
    private void PlayAttackSoundClientRpc()
    {
        string soundKey = Random.Range(0, 2) == 0 ? "Melee0" : "Melee1";
        Managers.Sound.PlaySFX(soundKey);
    }

    public override void TakeDamage(int damage)
    {
        // 서버에서만 데미지 처리
        if (!IsServer || _isDead)
            return;

        // NetworkVariable 프로퍼티 사용 -> 값 변경 시 자동 동기화 및 UI 갱신
        Hp = Mathf.Max(0, Hp - damage);

        // 사망이 아닐 때만 피격 모션 (사망 시에는 Die 트리거가 우선)
        if (!IsDead())
        {
            _animator?.SetTrigger("Hit");
        }

        // 피격 사운드 재생
        PlayHitSoundClientRpc();

        if (IsDead())
        {
            Die();
        }
    }

    [ClientRpc]
    private void PlayHitSoundClientRpc()
    {
        Managers.Sound.PlaySFX(Random.Range(0, 2) == 0 ? "Hit0" : "Hit1");
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log("[Enemy] 적이 사망했습니다.");

        _rigidbody.linearVelocity = Vector2.zero;
        UpdateAnimator(false);

        // [수정] 충돌체 비활성화 (추가 피격 및 길막 방지)
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        // [수정] 사망 애니메이션 코루틴 시작
        if (IsServer)
        {
            StartCoroutine(CoDeathSequence());
        }
    }

    // [추가] 사망 연출 코루틴 (서버)
    private IEnumerator CoDeathSequence()
    {
        // NetworkAnimator를 통해 클라이언트들에게 트리거 동기화
        // (ClientNetworkAnimator가 설정되어 있다면 자동 동기화됨)
        _animator?.SetTrigger("Die");

        // 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(_deathDelay);

        // 서버에서 Despawn을 호출하면, 
        // 등록된 NetworkObjectPool 핸들러를 통해 로컬 PoolManager.Despawn이 실행됩니다.
        if (IsServer && IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        // 감지 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        // 정지 거리 (파란색)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _stopDistance);
    }
}
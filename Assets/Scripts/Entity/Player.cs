using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // [추가] New Input System 네임스페이스

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity
{
    [Header("무기 설정")]
    [SerializeField] private Gun _gun;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    private PlayerMove _playerMove;
    private Camera _mainCamera; // [추가] 마우스 좌표 변환용 카메라 캐싱

    protected override void Awake()
    {
        base.Awake();
        _playerMove = GetComponent<PlayerMove>();
        _mainCamera = Camera.main;

        if (_gun == null) _gun = GetComponent<Gun>();
        if (_gun == null) _gun = gameObject.AddComponent<Gun>();
    }

    private void Start()
    {
        ConfigureWeapon();
    }

    private void ConfigureWeapon()
    {
        if (_gun == null) return;
        if (_bulletPrefab != null) _gun.SetBulletPrefab(_bulletPrefab);
        _gun.SetFirePoint(_firePoint != null ? _firePoint : transform);
    }

    /// <summary>
    /// 네트워크 스폰 시 입력 이벤트 바인딩
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Managers.Input을 통해 "Fire" 액션 바인딩
            // 주의: GameInputActions의 "Lobby" 맵에 "Fire" 액션이 추가되어 있어야 함
            Managers.Input.BindAction("Fire", HandleFire, InputActionPhase.Performed);
        }
    }

    /// <summary>
    /// 네트워크 디스폰 시 입력 이벤트 해제
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            if (Managers.Inst != null)
            {
                Managers.Input.UnbindAction("Fire", HandleFire, InputActionPhase.Performed);
            }
        }
    }

    /// <summary>
    /// 발사 입력 처리 핸들러
    /// </summary>
    private void HandleFire(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        // 마우스 위치 가져오기 (New Input System 방식)
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        if (_mainCamera == null) _mainCamera = Camera.main;
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // 서버에 발사 요청
        FireServerRpc(mouseWorldPos);
    }

    /// <summary>
    /// 클라이언트 -> 서버: 발사 요청 (ServerRpc)
    /// </summary>
    /// <param name="targetPos">목표 지점(마우스 위치)</param>
    [ServerRpc]
    private void FireServerRpc(Vector2 targetPos)
    {
        // 서버에서 조준 방향 갱신
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        UpdateAimDirection(dir);

        // 실제 발사 로직 수행 (Gun.cs는 PoolManager를 통해 총알 생성)
        _gun?.Attack(_attackPower);
    }

    /// <summary>
    /// 조준 방향 업데이트 (Entity/Weapon 기능)
    /// </summary>
    public void UpdateAimDirection(Vector2 direction) => _gun?.UpdateAimDirection(direction);

    public override void Attack()
    {
        // Entity 추상 메서드 구현체
        // 실제 로직은 FireServerRpc -> _gun.Attack()으로 처리됨
    }

    public override void TakeDamage(int damage)
    {
        // [중요] 데미지 판정 및 HP 감소는 오직 서버에서만 수행
        if (!IsServer) return;

        if (IsDead()) return;

        _hp = Mathf.Max(0, _hp - damage);
        UpdateHealthBar(); // 서버 측 체력바 갱신

        // 변경된 스탯(HP)을 모든 클라이언트에 동기화
        SyncStatsClientRpc(GetComponent<NetworkObject>().NetworkObjectId, _hp, _maxHp, _attackPower, _attackSpeed, _moveSpeed);

        if (IsDead())
        {
            Die();
        }
    }

    /// <summary>
    /// 서버에서 변경된 스탯을 클라이언트들에게 전파
    /// </summary>
    [ClientRpc]
    private void SyncStatsClientRpc(ulong networkObjectId, int hp, int maxHp, int atk, float atkSpeed, float moveSpeed)
    {
        // 자신(Player)의 Entity.SyncStats 호출
        SyncStats(hp, maxHp, atk, atkSpeed, moveSpeed);
    }

    private void Die()
    {
        // 서버에서만 호출됨
        Debug.Log($"[Player] 플레이어가 사망했습니다. OwnerID: {OwnerClientId}");
        gameObject.SetActive(false);

        // 매니저에게 사망 알림
        if (IsServer)
        {
            CombatGameManager.Instance?.OnPlayerDied(OwnerClientId);
        }
    }
}
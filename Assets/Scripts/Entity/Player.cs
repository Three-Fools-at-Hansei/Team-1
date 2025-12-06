using System.Globalization;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity
{
    [Header("무기 설정")]
    [SerializeField] private Gun _gun;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    private PlayerMove _playerMove;

    protected override void Awake()
    {
        base.Awake();
        _playerMove = GetComponent<PlayerMove>();

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

    private void Update()
    {
        // 로컬 플레이어(본인)만 입력을 처리하여 서버로 요청
        if (IsOwner)
        {
            // 마우스 왼쪽 클릭 감지 (추후 InputManager의 Action으로 교체 권장)
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // 마우스 위치를 월드 좌표로 변환
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);

                // 서버에 발사 요청
                FireServerRpc(mousePos);
            }
        }
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

        // 실제 발사 로직 수행 (Gun.cs가 서버 스폰 방식으로 수정되었다고 가정)
        _gun?.Attack(_attackPower);
    }

    /// <summary>
    /// 조준 방향 업데이트 (Entity/Weapon 기능)
    /// </summary>
    public void UpdateAimDirection(Vector2 direction) => _gun?.UpdateAimDirection(direction);

    public override void Attack()
    {
        // Entity 추상 메서드 구현체
        // 실제 로직은 FireServerRpc -> _gun.Attack()으로 처리되므로, 
        // 여기서는 로컬 클라이언트의 시각적/청각적 효과(발사음 등)를 처리하거나 비워둡니다.
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
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 플레이어 캐릭터 구현
/// </summary>
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

        if (_gun == null)
        {
            _gun = GetComponent<Gun>();
        }

        if (_gun == null)
        {
            _gun = gameObject.AddComponent<Gun>();
        }
    }

    private void Start()
    {
        ConfigureWeapon();
    }

    /// <summary>
    /// 무기 프리팹 / 발사 위치 설정
    /// </summary>
    private void ConfigureWeapon()
    {
        if (_gun == null)
        {
            Debug.LogWarning("[Player] Gun 컴포넌트가 없습니다.");
            return;
        }

        if (_bulletPrefab != null)
        {
            _gun.SetBulletPrefab(_bulletPrefab);
        }

        _gun.SetFirePoint(_firePoint != null ? _firePoint : transform);
    }

    /// <summary>
    /// 조준 방향 업데이트 (PlayerMove에서 호출)
    /// </summary>
    public void UpdateAimDirection(Vector2 direction)
    {
        _gun?.UpdateAimDirection(direction);
    }

    public override void Attack()
    {
        _gun?.Attack(_attackPower);
    }

    public override void TakeDamage(int damage)
    {
        // 서버 권한 체크가 필요하지만, Entity 구조상 일단 실행 후 검증하거나
        // NetworkBehaviour인 PlayerMove를 통해 처리하는 것이 정석입니다.
        // 여기서는 피격 로직이 서버에서 호출된다고 가정합니다 (Bullet 충돌 로직 등).

        if (IsDead()) return;

        _hp = Mathf.Max(0, _hp - damage);
        UpdateHealthBar();

        if (IsDead())
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[Player] 플레이어가 사망했습니다.");
        gameObject.SetActive(false);

        // 서버라면 매니저에게 사망 알림
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            // NetworkObject 컴포넌트를 통해 ClientId 확인
            var netObj = GetComponent<NetworkObject>();
            if (netObj != null)
            {
                CombatGameManager.Instance?.OnPlayerDied(netObj.OwnerClientId);
            }
        }
    }
}
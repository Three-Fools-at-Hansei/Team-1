using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 총 클래스
/// </summary>
public class Gun : Weapon
{
    [Header("총 설정")]
    [SerializeField] private GameObject _bulletPrefab; // 반드시 NetworkObject가 달린 프리팹이어야 함
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _bulletSpeed = 10.0f;

    /// <summary>
    /// 서버에서 호출되어 실제 총알을 생성합니다.
    /// </summary>
    public override void Attack(int attackPower)
    {
        // [중요] 서버 권한 체크 (서버만 스폰 가능)
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        if (_bulletPrefab == null) return;

        // 1. 총알 생성 (서버 로컬)
        GameObject bulletGo = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);

        // 2. 네트워크 스폰 (모든 클라이언트에 복제)
        var netObj = bulletGo.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }

        // 3. 총알 초기화 (속도 및 데미지 설정)
        Bullet bullet = bulletGo.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(attackPower, _aimDirection, _bulletSpeed);
        }
    }
}
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 서버 권한으로 총알을 발사하는 무기 클래스입니다.
/// </summary>
public class Gun : Weapon
{
    [Header("총 설정")]
    [SerializeField] private GameObject _bulletPrefab; // 반드시 NetworkObject가 달린 프리팹이어야 함
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _bulletSpeed = 10.0f;
    [SerializeField] private float _fireRate = 0.5f; // 연사 속도 (초 단위)

    private float _lastFireTime;

    private void Awake()
    {
        // FirePoint가 없으면 총의 위치를 사용
        if (_firePoint == null)
        {
            _firePoint = transform;
        }
    }

    /// <summary>
    /// 서버에서 호출되어 총알을 생성하고 네트워크에 스폰합니다.
    /// </summary>
    public void Attack(Vector2 spawnPosition, int attackPower)
    {
        // 서버 권한 체크 (서버만 스폰 가능)
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        // 쿨타임 체크 (서버 시간 기준)
        if (Time.time - _lastFireTime < _fireRate)
        {
            return;
        }

        if (_bulletPrefab == null)
        {
            Debug.LogWarning("[Gun] Bullet Prefab이 할당되지 않았습니다.");
            return;
        }

        _lastFireTime = Time.time;

        PlayFireSoundClientRpc();

        // 1. 회전 계산
        float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

        // 2. 전달받은 spawnPosition(클라이언트가 요청한 위치)에서 생성
        GameObject bulletGo = Managers.Pool.Spawn(_bulletPrefab, spawnPosition, rotation);

        var netObj = bulletGo.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            // 이미 풀에서 가져온 객체이므로 활성화된 상태입니다.
            // NetworkObject.Spawn()을 호출하여 네트워크 ID를 할당하고 클라이언트들에게 전파합니다.
            netObj.Spawn();
        }
        else
        {
            Debug.LogError("[Gun] Bullet Prefab에 NetworkObject 컴포넌트가 없습니다!");
            Managers.Pool.Despawn(bulletGo); // 동기화 불가능하므로 즉시 반환
            return;
        }

        // 3. 총알 초기화 (속도 및 데미지 설정)
        Bullet bullet = bulletGo.GetComponent<Bullet>();
        if (bullet != null)
        {
            // 회전은 이미 적용되었으므로 공격력과 속도만 설정
            bullet.Initialize(attackPower, _aimDirection, _bulletSpeed);
        }
    }

    // 기존 추상 메서드 구현 유지를 위한 오버로딩 (필요하다면 유지, 안 쓰면 삭제 가능)
    public override void Attack(int attackPower)
    {
        Attack(_firePoint.position, attackPower);
    }

    /// <summary>
    /// (옵션) 총알 프리팹 동적 변경 시 사용
    /// </summary>
    public void SetBulletPrefab(GameObject prefab)
    {
        _bulletPrefab = prefab;
    }

    /// <summary>
    /// (옵션) 발사 위치 동적 변경 시 사용
    /// </summary>
    public void SetFirePoint(Transform firePoint)
    {
        _firePoint = firePoint;
    }

    [ClientRpc]
    private void PlayFireSoundClientRpc()
    {
        Managers.Sound.PlaySFX("Range");
    }
}
using UnityEngine;

/// <summary>
/// 총 클래스
/// </summary>
public class Gun : Weapon
{
    [Header("총 설정")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _bulletSpeed = 10.0f;
    [SerializeField] private float _fireRate = 1.0f; // 1초에 1발

    private float _lastFireTime = 0f;

    private void Awake()
    {
        // FirePoint가 없으면 총의 위치를 사용
        if (_firePoint == null)
        {
            _firePoint = transform;
        }
    }

    /// <summary>
    /// 공격 메서드 오버라이드
    /// 총알을 공격 방향으로 생성하여 지속적으로 이동하게 함
    /// </summary>
    /// <param name="attackPower">공격력</param>
    public override void Attack(int attackPower)
    {
        // 공격 속도 제한 (가장 먼저 체크 - 디버그 로그보다 먼저!)
        if (Time.time - _lastFireTime < _fireRate)
        {
            return; // 아직 발사 시간이 안 됨
        }

        if (_bulletPrefab == null)
        {
            return;
        }

        if (Managers.Pool == null)
        {
            return;
        }

        // FirePoint 확인
        if (_firePoint == null)
        {
            Debug.LogWarning("[Gun] FirePoint가 설정되지 않았습니다!");
            return;
        }

        // 총알 생성 전에 시간 업데이트 (중복 호출 방지)
        _lastFireTime = Time.time;

        Debug.Log($"[Gun] 총알 생성: prefab={_bulletPrefab.name}, position={_firePoint.position}, time={Time.time:F3}");

        // 총알 생성 (parent를 null로 전달하여 씬 루트에 생성)
        GameObject bulletObj = Managers.Pool.Spawn(
            _bulletPrefab,
            _firePoint.position,
            Quaternion.identity,
            null  // parent를 명시적으로 null로 설정
        );

        if (bulletObj == null)
        {
            Debug.LogError("[Gun] 총알 생성 실패!");
            return;
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            // 총알 초기화
            bullet.Initialize(attackPower, _aimDirection, _bulletSpeed);
        }
        else
        {
            Debug.LogWarning("[Gun] 총알 오브젝트에 Bullet 컴포넌트가 없습니다.");
        }
    }

    /// <summary>
    /// 총알 프리팹 설정
    /// </summary>
    /// <param name="prefab">총알 프리팹</param>
    public void SetBulletPrefab(GameObject prefab)
    {
        _bulletPrefab = prefab;
    }

    /// <summary>
    /// 발사 위치 설정
    /// </summary>
    /// <param name="firePoint">발사 위치 Transform</param>
    public void SetFirePoint(Transform firePoint)
    {
        _firePoint = firePoint;
    }
}


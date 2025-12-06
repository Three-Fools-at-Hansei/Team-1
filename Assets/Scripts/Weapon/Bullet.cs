using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 총알 클래스
/// </summary>
public class Bullet : NetworkBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private float _speed = 10.0f;
    [SerializeField] private float _lifeTime = 3.0f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        // 서버에서만 수명 관리 (일정 시간 후 파괴)
        if (IsServer)
        {
            Invoke(nameof(DestroyBullet), _lifeTime);
        }
    }

    /// <summary>
    /// 서버에서 생성 직후 초기 속도 설정
    /// </summary>
    /// <param name="damage">공격 데미지</param>
    /// <param name="direction">공격 방향</param>
    /// <param name="speed">이동 속도</param>
    public void Initialize(int damage, Vector2 direction, float speed)
    {
        if (!IsServer) return; // 서버만 설정 가능

        _attackDamage = damage;
        _speed = speed;

        // 방향 회전 (오른쪽 기준)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 속도 적용
        if (_rigidbody != null)
        {
            Debug.Log($"[Bullet] Rigidbody 속도 설정: {_rigidbody.linearVelocity}");
            _rigidbody.linearVelocity = direction.normalized * _speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // [중요] 충돌 판정은 오직 서버에서만 수행
        if (!IsServer) return;

        // 발사한 플레이어 본인 충돌 무시 (태그 또는 Layer 활용 권장)
        if (collision.CompareTag("Player")) return;

        // 데미지 처리
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null)
        {
            entity.TakeDamage(_attackDamage);
        }

        // 충돌 시 즉시 파괴 (서버)
        DestroyBullet();
    }

    private void DestroyBullet()
    {
        if (IsSpawned)
        {
            // NetworkObject Despawn (모든 클라이언트에서 사라짐)
            NetworkObject.Despawn();
        }
    }
}
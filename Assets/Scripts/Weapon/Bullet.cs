using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 서버 권한으로 동작하는 총알 클래스입니다.
/// 충돌 판정과 수명 관리를 서버에서 수행하며, NetworkObject를 통해 동기화됩니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkObject))]
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
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f; // 중력 영향 받지 않음
        }
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
    /// 서버에서 생성 직후 초기 속도 및 데미지를 설정합니다.
    /// (Gun.cs에서 호출)
    /// </summary>
    /// <param name="damage">공격력</param>
    /// <param name="direction">발사 방향</param>
    /// <param name="speed">이동 속도</param>
    public void Initialize(int damage, Vector2 direction, float speed)
    {
        // 서버에서만 물리 속도를 설정하면, NetworkTransform을 통해 클라이언트로 위치가 동기화됩니다.
        if (!IsServer) return;

        _attackDamage = damage;
        _speed = speed;

        // 방향 회전 (오른쪽(Vector2.right)이 0도 기준일 때의 회전 처리)
        // 만약 스프라이트가 위쪽을 보고 있다면 -90도 보정이 필요할 수 있습니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 속도 적용
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = direction.normalized * _speed;
        }
    }

    /// <summary>
    /// 물리 충돌 처리 (Trigger)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // [중요] 충돌 판정은 오직 서버에서만 수행하여 데이터 불일치를 방지합니다.
        if (!IsServer) return;

        // 발사한 플레이어 본인과의 충돌은 무시합니다.
        // (플레이어 프리팹의 태그가 "Player"여야 합니다)
        if (collision.CompareTag("Player")) return;

        // 데미지 처리
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null)
        {
            entity.TakeDamage(_attackDamage);
        }

        // 벽이나 적 등과 충돌 시 총알 파괴 (서버)
        // Trigger 충돌이므로 관통이 필요 없다면 즉시 파괴합니다.
        DestroyBullet();
    }

    /// <summary>
    /// 총알을 네트워크상에서 제거합니다.
    /// </summary>
    private void DestroyBullet()
    {
        // 이미 파괴되었거나 스폰 상태가 아니라면 무시
        if (IsSpawned)
        {
            // NetworkObject Despawn 호출 시 모든 클라이언트에서 해당 오브젝트가 사라집니다.
            NetworkObject.Despawn();
        }
    }
}
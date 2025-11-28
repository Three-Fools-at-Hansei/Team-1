using UnityEngine;

/// <summary>
/// 총알 클래스
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private Vector2 _attackDirection = Vector2.right;
    [SerializeField] private float _speed = 10.0f;
    [SerializeField] private float _lifeTime = 5.0f;

    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private float _spawnTime;
    private float _ignorePlayerTime = 0.1f; // 생성 후 0.1초 동안 플레이어와 충돌 무시

    private void Awake()
    {
        Debug.Log($"[Bullet] Awake 호출: {gameObject.name}");
        
        // 불필요한 컴포넌트 제거 (Camera 등)
        RemoveUnnecessaryComponents();
        
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0;
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            // 총알이 다른 오브젝트보다 앞에 렌더링되도록 설정 (플레이어보다는 뒤에)
            _spriteRenderer.sortingOrder = 1;
            Debug.Log($"[Bullet] SpriteRenderer 설정: sprite={_spriteRenderer.sprite?.name}, sortingOrder={_spriteRenderer.sortingOrder}");
        }
        else
        {
            Debug.LogWarning($"[Bullet] SpriteRenderer가 없습니다!");
        }

        _collider = GetComponent<Collider2D>();
        Debug.Log($"[Bullet] Collider: {_collider?.GetType().Name}");
    }

    /// <summary>
    /// 총알에 불필요한 컴포넌트 제거 (Camera 등)
    /// </summary>
    private void RemoveUnnecessaryComponents()
    {
        // 중요: UniversalAdditionalCameraData를 먼저 제거해야 함 (Camera에 의존하므로)
        var cameraData = GetComponent("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
        if (cameraData != null)
        {
            Debug.LogWarning($"[Bullet] UniversalAdditionalCameraData 컴포넌트가 발견되어 제거합니다: {gameObject.name}");
            DestroyImmediate(cameraData);
        }

        // 그 다음 Camera 컴포넌트 제거
        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            Debug.LogWarning($"[Bullet] Camera 컴포넌트가 발견되어 제거합니다: {gameObject.name}");
            DestroyImmediate(camera);
        }

        // Physics 2D Raycaster 제거
        var raycaster = GetComponent("UnityEngine.EventSystems.Physics2DRaycaster");
        if (raycaster != null)
        {
            Debug.LogWarning($"[Bullet] Physics2DRaycaster 컴포넌트가 발견되어 제거합니다: {gameObject.name}");
            DestroyImmediate(raycaster);
        }

        // Canvas 관련 컴포넌트 제거
        var canvas = GetComponent("UnityEngine.Canvas");
        if (canvas != null)
        {
            Debug.LogWarning($"[Bullet] Canvas 컴포넌트가 발견되어 제거합니다: {gameObject.name}");
            DestroyImmediate(canvas);
        }

        var canvasRenderer = GetComponent("UnityEngine.CanvasRenderer");
        if (canvasRenderer != null)
        {
            Debug.LogWarning($"[Bullet] CanvasRenderer 컴포넌트가 발견되어 제거합니다: {gameObject.name}");
            DestroyImmediate(canvasRenderer);
        }

        var graphicRaycaster = GetComponent("UnityEngine.UI.GraphicRaycaster");
        if (graphicRaycaster != null)
        {
            Debug.LogWarning($"[Bullet] GraphicRaycaster 컴포넌트가 발견되어 제거합니다: {gameObject.name}");
            DestroyImmediate(graphicRaycaster);
        }
    }

    private void OnEnable()
    {
        _spawnTime = Time.time;
        Debug.Log($"[Bullet] OnEnable 호출: {gameObject.name}, position={transform.position}, parent={transform.parent?.name ?? "null"}");
        
        // 플레이어와의 충돌 무시 설정
        IgnorePlayerCollision();
    }

    /// <summary>
    /// 플레이어와의 충돌을 일시적으로 무시
    /// </summary>
    private void IgnorePlayerCollision()
    {
        if (_collider == null) return;

        // 씬의 모든 플레이어 찾기
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(_collider, playerCollider, true);
            }
        }

        // 일정 시간 후 충돌 다시 활성화
        Invoke(nameof(EnablePlayerCollision), _ignorePlayerTime);
    }

    /// <summary>
    /// 플레이어와의 충돌 다시 활성화
    /// </summary>
    private void EnablePlayerCollision()
    {
        if (_collider == null) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(_collider, playerCollider, false);
            }
        }
    }
  

    private void Update()
    {
        // 생명 주기 체크
        if (Time.time - _spawnTime > _lifeTime)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 총알 초기화
    /// </summary>
    /// <param name="damage">공격 데미지</param>
    /// <param name="direction">공격 방향</param>
    /// <param name="speed">이동 속도</param>
    public void Initialize(int damage, Vector2 direction, float speed)
    {
        Debug.Log($"[Bullet] Initialize 호출: damage={damage}, direction={direction}, speed={speed}");
        
        _attackDamage = damage;
        _attackDirection = direction.normalized;
        _speed = speed;
        _spawnTime = Time.time;

        // 방향에 따라 회전
        // Bullet 3 스프라이트는 기본적으로 위쪽(↑)을 향하고 있으므로, 각도에서 90도를 빼야 함
        // Atan2는 오른쪽(→)이 0도, 위쪽(↑)이 90도이므로, 위쪽이 기본 방향이면 -90도 보정 필요
        if (_attackDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(_attackDirection.y, _attackDirection.x) * Mathf.Rad2Deg;
            angle -= 90f; // 위쪽이 기본 방향이므로 90도 보정
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Debug.Log($"[Bullet] 회전 설정: direction={_attackDirection}, angle={angle:F1}도 (보정 후)");
        }
        else
        {
            // 방향이 없으면 기본 방향(위쪽) 유지
            transform.rotation = Quaternion.AngleAxis(-90f, Vector3.forward);
        }

        // 속도 설정
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = _attackDirection * _speed;
            Debug.Log($"[Bullet] Rigidbody 속도 설정: {_rigidbody.linearVelocity}");
        }

        // 스케일 초기화 (너무 크지 않도록)
        transform.localScale = Vector3.one;
        Debug.Log($"[Bullet] 최종 설정: position={transform.position}, scale={transform.localScale}, rotation={transform.rotation.eulerAngles}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollision(collision.collider);
    }

    /// <summary>
    /// 충돌 처리
    /// 적과 충돌 시 데미지를 입히고 사라짐 (풀 반환)
    /// </summary>
    /// <param name="collision">충돌한 Collider2D</param>
    private void OnCollision(Collider2D collision)
    {
        // 플레이어와는 충돌 무시 (총알이 플레이어에게서 발사되므로)
        if (collision.CompareTag("Player"))
        {
            return;
        }

        // Entity(Enemy/Core 등) 충돌 시
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null)
        {
            entity.TakeDamage(_attackDamage);
            ReturnToPool();
            return;
        }

        // 벽이나 장애물과 충돌 시에도 풀 반환
        if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    private void ReturnToPool()
    {
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }

        EnablePlayerCollision();

        if (Managers.Pool != null)
        {
            Managers.Pool.Despawn(gameObject);
        }
    }

    // 프로퍼티
    public int AttackDamage => _attackDamage;
    public Vector2 AttackDirection => _attackDirection;
}

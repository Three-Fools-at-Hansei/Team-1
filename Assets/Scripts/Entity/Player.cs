using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity
{
    // [Merge] 로컬 플레이어의 사망 상태 변경 알림 이벤트
    // UI(HEAD)와 PlayerCameraAgent(Main) 모두에서 구독합니다.
    public static event Action<bool> OnLocalPlayerDeadStateChanged;

    [Header("무기 설정")]
    [SerializeField] private Gun _gun;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    
    // [HEAD] WeaponUI 참조 추가
    [SerializeField] private WeaponUI _weaponUI;

    private PlayerMove _playerMove;
    private Camera _mainCamera;
    private Vector2 _lastMouseAimDirection = Vector2.right;

    protected override void Awake()
    {
        base.Awake();
        _playerMove = GetComponent<PlayerMove>();
        _mainCamera = Camera.main; // [Optim] 카메라 참조 캐싱

        // [HEAD] 안전한 컴포넌트 할당 방식 사용
        if (_gun == null) _gun = GetComponent<Gun>();
        if (_gun == null) _gun = gameObject.AddComponent<Gun>();

        if (_weaponUI == null) _weaponUI = GetComponent<WeaponUI>();
    }

    private void Start()
    {
        ConfigureWeapon();
    }

    private void Update()
    {
        // [HEAD] Owner인 경우 마우스 조준 방향을 지속적으로 업데이트 (WeaponUI 및 스프라이트 회전)
        if (IsOwner && _mainCamera != null && Mouse.current != null)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
            
            // 동적 총구 위치(WeaponUI 등)를 고려한 조준 방향 계산
            Vector2 firePos = GetCurrentFirePointPosition();
            Vector2 direction = (mouseWorldPos - firePos).normalized;
            
            // 불필요한 연산 방지를 위해 방향이 유의미하게 변경되었을 때만 업데이트
            if (Vector2.Distance(direction, _lastMouseAimDirection) > 0.01f)
            {
                _lastMouseAimDirection = direction;
                if (_playerMove != null)
                {
                    _playerMove.UpdateMouseAimDirection(direction);
                }
            }
        }
    }

    private void ConfigureWeapon()
    {
        if (_gun == null) return;
        if (_bulletPrefab != null) _gun.SetBulletPrefab(_bulletPrefab);
        // WeaponUI가 없다면 기본 Transform을 사용
        _gun.SetFirePoint(_firePoint != null ? _firePoint : transform);
    }

    /// <summary>
    /// 네트워크 스폰 시 입력 이벤트 바인딩 및 초기화
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn(); 

        // [HEAD] 무기 UI 초기화
        if (_weaponUI != null)
        {
            _weaponUI.Initialize(transform);
        }

        if (IsOwner)
        {
            Managers.Input.BindAction("Fire", HandleFire, InputActionPhase.Performed);

            // [Merge] 스폰 시 생존 상태 알림 (false = 살아있음)
            OnLocalPlayerDeadStateChanged?.Invoke(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner && Managers.Inst != null)
        {
            Managers.Input.UnbindAction("Fire", HandleFire, InputActionPhase.Performed);
        }
    }

    /// <summary>
    /// [HEAD] 현재 총구 위치를 반환합니다. (WeaponUI가 존재하면 해당 위치 우선)
    /// </summary>
    private Vector2 GetCurrentFirePointPosition()
    {
        if (_weaponUI != null)
        {
            return _weaponUI.GetMuzzlePosition();
        }
        
        if (_firePoint != null)
        {
            return _firePoint.position;
        }
        
        return transform.position;
    }

    /// <summary>
    /// 발사 입력 처리 핸들러
    /// </summary>
    private void HandleFire(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        if (_mainCamera == null) _mainCamera = Camera.main;
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // [HEAD] WeaponUI의 보정된 총구 위치 사용
        Vector2 firePos = GetCurrentFirePointPosition();

        // 발사 요청
        FireServerRpc(mouseWorldPos, firePos);
    }

    /// <summary>
    /// 클라이언트 -> 서버: 발사 요청
    /// </summary>
    [ServerRpc]
    private void FireServerRpc(Vector2 targetPos, Vector2 clientFirePos)
    {
        // 1. 조준 방향 업데이트 (서버에서도 동기화)
        Vector2 firePos = GetCurrentFirePointPosition();
        Vector2 dir = (targetPos - firePos).normalized;
        UpdateAimDirection(dir);

        // 2. 보안 검사 (Anti-Cheat): 거리 오차 검증
        Vector2 serverFirePos = GetCurrentFirePointPosition();
        float distance = Vector2.Distance(clientFirePos, serverFirePos);
        
        // 오차가 크면 서버 위치, 작으면 클라이언트 위치(반응성) 사용
        Vector2 spawnPos = (distance > 2.0f) ? serverFirePos : clientFirePos;

        // 3. 발사
        _gun?.Attack(spawnPos, AttackPower);
    }

    public void UpdateAimDirection(Vector2 direction) => _gun?.UpdateAimDirection(direction);

    public override void Attack()
    {
        // 실제 공격 로직은 FireServerRpc -> _gun.Attack()으로 위임됨
    }

    public override void TakeDamage(int damage)
    {
        if (!IsServer) return;
        if (IsDead()) return;

        Hp = Mathf.Max(0, Hp - damage);

        if (IsDead())
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsServer)
        {
            Debug.Log($"[Player] 플레이어 사망 처리 (Server). OwnerID: {OwnerClientId}");
            CombatGameManager.Instance?.OnPlayerDied(OwnerClientId);
            DieClientRpc();
        }
    }

    /// <summary>
    /// 사망 처리 클라이언트 RPC
    /// </summary>
    [ClientRpc]
    private void DieClientRpc()
    {
        Debug.Log($"[Player] 플레이어 비활성화 (Client). OwnerID: {OwnerClientId}");

        // [Main Logic Adoption] 
        // 오브젝트를 끄기 전에 이벤트를 먼저 호출합니다.
        // 이유: 구독자(CameraAgent 등)가 Player 오브젝트의 마지막 위치나 상태를 참조해야 할 수 있기 때문입니다.
        if (IsOwner)
        {
            OnLocalPlayerDeadStateChanged?.Invoke(true);
        }

        gameObject.SetActive(false);
    }
}
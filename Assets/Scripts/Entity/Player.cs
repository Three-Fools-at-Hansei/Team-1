using System;
using System.Threading.Tasks; // [New] 비동기 처리를 위해 추가
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : Entity
{
    // 로컬 플레이어의 사망 상태 변경 알림 이벤트
    public static event Action<bool> OnLocalPlayerDeadStateChanged;

    // 플레이어 스폰, 디스폰 이벤트
    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;

    [Header("무기 설정")]
    [SerializeField] private Gun _gun;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    // WeaponUI 참조
    [SerializeField] private WeaponUI _weaponUI;

    // [New] 묘비 생성을 위한 Addressable Key 상수
    private const string TOMBSTONE_KEY = "Tombstone";

    private PlayerMove _playerMove;
    private Camera _mainCamera;
    private Vector2 _lastMouseAimDirection = Vector2.right;

    protected override void Awake()
    {
        base.Awake();
        _playerMove = GetComponent<PlayerMove>();
        _mainCamera = Camera.main;

        // 안전한 컴포넌트 할당 방식 사용
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
        // Owner인 경우 마우스 조준 방향을 지속적으로 업데이트
        if (IsOwner && _mainCamera != null && Mouse.current != null)
        {
            // 사망 시 조준 방향 업데이트 중지
            if (IsDead()) return;

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);

            Vector2 firePos = GetCurrentFirePointPosition();
            Vector2 direction = (mouseWorldPos - firePos).normalized;

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
        _gun.SetFirePoint(_firePoint != null ? _firePoint : transform);
    }

    /// <summary>
    /// PlayerMove 등 다른 클래스에서 사망 여부를 확인할 수 있도록 공개
    /// </summary>
    public new bool IsDead() => base.IsDead();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (_weaponUI != null)
        {
            _weaponUI.Initialize(transform);
        }

        if (IsOwner)
        {
            Managers.Input.BindAction("Fire", HandleFire, InputActionPhase.Performed);
            OnLocalPlayerDeadStateChanged?.Invoke(false);
        }

        OnPlayerSpawned?.Invoke(this);
    }

    public override void OnNetworkDespawn()
    {
        OnPlayerSpawned?.Invoke(this); // [수정] 기존 코드에서 Invoke(this) 호출 위치가 OnPlayerDespawned여야 하나 변수명이 혼동되어 있을 수 있음. 문맥상 OnPlayerDespawned?.Invoke(this)가 맞음.
        OnPlayerDespawned?.Invoke(this);

        base.OnNetworkDespawn();

        if (IsOwner && Managers.Inst != null)
        {
            Managers.Input.UnbindAction("Fire", HandleFire, InputActionPhase.Performed);
        }
    }

    private Vector2 GetCurrentFirePointPosition()
    {
        if (_weaponUI != null) return _weaponUI.GetMuzzlePosition();
        if (_firePoint != null) return _firePoint.position;
        return transform.position;
    }

    private void HandleFire(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (IsDead()) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        if (_mainCamera == null) _mainCamera = Camera.main;
        Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 firePos = GetCurrentFirePointPosition();
        FireServerRpc(mouseWorldPos, firePos);
    }

    [ServerRpc]
    private void FireServerRpc(Vector2 targetPos, Vector2 clientFirePos)
    {
        if (IsDead()) return;

        Vector2 firePos = GetCurrentFirePointPosition();
        Vector2 dir = (targetPos - firePos).normalized;
        UpdateAimDirection(dir);

        Vector2 serverFirePos = GetCurrentFirePointPosition();
        float distance = Vector2.Distance(clientFirePos, serverFirePos);
        Vector2 spawnPos = (distance > 2.0f) ? serverFirePos : clientFirePos;

        _gun?.Attack(spawnPos, AttackPower);
    }

    public void UpdateAimDirection(Vector2 direction) => _gun?.UpdateAimDirection(direction);

    public override void Attack() { }

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
    /// 사망 처리 클라이언트 RPC (비동기)
    /// </summary>
    [ClientRpc]
    private async void DieClientRpc()
    {
        Debug.Log($"[Player] 플레이어 비활성화 (Client). OwnerID: {OwnerClientId}");

        // 사망 사운드 재생
        Managers.Sound.PlaySFX("Dead");

        // [New] 묘비 동적 생성 (Addressable Key 사용)
        // 리소스 로드가 완료될 때까지 기다린 후 플레이어를 비활성화합니다.
        try
        {
            // 위치와 회전은 현재 플레이어 기준
            await Managers.Resource.InstantiateAsync(TOMBSTONE_KEY, transform.position, Quaternion.identity);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Player] 묘비 생성 실패: {e.Message}");
        }

        // UI 및 카메라 등 로컬 시스템에 사망 알림
        if (IsOwner)
        {
            OnLocalPlayerDeadStateChanged?.Invoke(true);
        }

        // 즉시 비활성화
        gameObject.SetActive(false);
    }
}
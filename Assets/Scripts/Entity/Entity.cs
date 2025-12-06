using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어와 적, 코어의 기반 클래스
/// NetworkVariable을 도입하여 모든 스탯을 자동으로 동기화합니다.
/// </summary>
public abstract class Entity : NetworkBehaviour
{
    [Header("기본 스탯 (초기값)")]
    [SerializeField] protected int _defaultHp = 100;
    [SerializeField] protected int _defaultAttackPower = 10;
    [SerializeField] protected float _defaultAttackSpeed = 1.0f;
    [SerializeField] protected float _defaultMoveSpeed = 5.0f;
    [SerializeField] protected float _attackRange = 2.0f;
    [SerializeField] protected float _detectionRange = 10.0f;

    [Header("UI")]
    [SerializeField] private HealthBar _healthBar;

    // [핵심] 네트워크 변수 선언 (자동 동기화)
    public readonly NetworkVariable<int> NetHp = new NetworkVariable<int>(100);
    public readonly NetworkVariable<int> NetMaxHp = new NetworkVariable<int>(100);
    public readonly NetworkVariable<int> NetAttackPower = new NetworkVariable<int>(10);
    public readonly NetworkVariable<float> NetAttackSpeed = new NetworkVariable<float>(1.0f);
    public readonly NetworkVariable<float> NetMoveSpeed = new NetworkVariable<float>(5.0f);

    // 프로퍼티가 NetworkVariable을 참조하도록 연결
    public int Hp
    {
        get => NetHp.Value;
        protected set { if (IsServer) NetHp.Value = value; }
    }
    public int MaxHp
    {
        get => NetMaxHp.Value;
        protected set { if (IsServer) NetMaxHp.Value = value; }
    }
    public int AttackPower
    {
        get => NetAttackPower.Value;
        protected set { if (IsServer) NetAttackPower.Value = value; }
    }
    public float AttackSpeed
    {
        get => NetAttackSpeed.Value;
        protected set { if (IsServer) NetAttackSpeed.Value = value; }
    }
    public float MoveSpeed
    {
        get => NetMoveSpeed.Value;
        protected set { if (IsServer) NetMoveSpeed.Value = value; }
    }

    protected virtual void Awake()
    {
        // Awake 시점에는 네트워크 연결 전이므로 초기화하지 않음
    }

    public override void OnNetworkSpawn()
    {
        // 서버인 경우 초기값 설정
        if (IsServer)
        {
            NetHp.Value = _defaultHp;
            NetMaxHp.Value = _defaultHp;
            NetAttackPower.Value = _defaultAttackPower;
            NetAttackSpeed.Value = _defaultAttackSpeed;
            NetMoveSpeed.Value = _defaultMoveSpeed;
        }

        // 값이 변경될 때마다 UI 갱신 이벤트 연결 (서버/클라이언트 모두 동작)
        NetHp.OnValueChanged += OnHpChanged;
        NetMaxHp.OnValueChanged += OnMaxHpChanged;

        // 초기 UI 설정
        InitializeHealthBar();
        UpdateHealthBar();
    }

    public override void OnNetworkDespawn()
    {
        NetHp.OnValueChanged -= OnHpChanged;
        NetMaxHp.OnValueChanged -= OnMaxHpChanged;
    }

    // NetworkVariable 콜백 함수
    private void OnHpChanged(int prev, int curr) => UpdateHealthBar();
    private void OnMaxHpChanged(int prev, int curr) => InitializeHealthBar();

    public abstract void Attack();
    public abstract void TakeDamage(int damage);

    /// <summary>
    /// 체력 회복
    /// </summary>
    public virtual void Heal(int amount)
    {
        if (!IsServer || IsDead()) return;
        Hp = Mathf.Min(Hp + amount, MaxHp); // 자동 동기화
    }

    /// <summary>
    /// 최대 체력 증가 (증가한 만큼 현재 체력도 회복)
    /// </summary>
    public void IncreaseMaxHp(int amount)
    {
        if (!IsServer) return;
        MaxHp += amount;
        Hp += amount;
    }

    public void IncreaseAttackPower(int amount) { if (IsServer) AttackPower += amount; }
    public void IncreaseAttackSpeed(float amount) { if (IsServer) AttackSpeed += amount; }
    public void IncreaseMoveSpeed(float amount) { if (IsServer) MoveSpeed += amount; }

    protected bool IsDead() => Hp <= 0;

    protected void InitializeHealthBar()
    {
        if (_healthBar != null) _healthBar.Initialize(transform, MaxHp);
    }

    protected void UpdateHealthBar()
    {
        _healthBar?.SetValue(Hp);
    }
}
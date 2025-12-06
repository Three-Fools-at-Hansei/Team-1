using UnityEngine;

/// <summary>
/// 플레이어와 적의 기반 클래스
/// </summary>
public abstract class Entity : MonoBehaviour
{
    [Header("기본 스탯")]
    [SerializeField] protected int _hp = 100;
    [SerializeField] protected int _maxHp = 100; // 최대 체력 추가
    [SerializeField] protected int _attackPower = 10;
    [SerializeField] protected float _attackSpeed = 1.0f;
    [SerializeField] protected float _moveSpeed = 5.0f;
    [SerializeField] protected float _attackRange = 2.0f;
    [SerializeField] protected float _detectionRange = 10.0f;

    [Header("UI")]
    [SerializeField] private HealthBar _healthBar;

    public int Hp => _hp;
    public int MaxHp => _maxHp;
    public int AttackPower => _attackPower;
    public float AttackSpeed => _attackSpeed;
    public float MoveSpeed => _moveSpeed;

    protected virtual void Awake()
    {
        _maxHp = _hp; // 초기 체력을 최대 체력으로 설정
        InitializeHealthBar();
    }

    public abstract void Attack();
    public abstract void TakeDamage(int damage);

    /// <summary>
    /// 체력 회복
    /// </summary>
    public virtual void Heal(int amount)
    {
        if (IsDead()) return;
        _hp = Mathf.Min(_hp + amount, _maxHp);
        UpdateHealthBar();
    }

    /// <summary>
    /// 최대 체력 증가 (증가한 만큼 현재 체력도 회복)
    /// </summary>
    public void IncreaseMaxHp(int amount)
    {
        _maxHp += amount;
        _hp += amount;
        UpdateHealthBar();
    }

    public void IncreaseAttackPower(int amount) => _attackPower += amount;
    public void IncreaseAttackSpeed(float amount) => _attackSpeed += amount; // 공격 속도는 낮을수록 빠른지, 높을수록 빠른지 정의 필요 (여기선 높을수록 빠름 가정)
    public void IncreaseMoveSpeed(float amount) => _moveSpeed += amount;

    /// <summary>
    /// (클라이언트 동기화용) 스탯 강제 설정
    /// </summary>
    public void SyncStats(int currentHp, int maxHp, int atk, float atkSpeed, float moveSpeed)
    {
        _hp = currentHp;
        _maxHp = maxHp;
        _attackPower = atk;
        _attackSpeed = atkSpeed;
        _moveSpeed = moveSpeed;
        UpdateHealthBar();
    }

    protected bool IsDead() => _hp <= 0;

    protected void InitializeHealthBar()
    {
        if (_healthBar != null) _healthBar.Initialize(transform, _maxHp);
    }

    protected void UpdateHealthBar()
    {
        _healthBar?.SetValue(_hp);
    }
}

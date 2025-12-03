using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 플레이어와 적의 기반 클래스
/// </summary>
public abstract class Entity : NetworkBehaviour
{
    [Header("기본 스탯")]
    [SerializeField] protected int _hp = 100;
    [SerializeField] protected int _attackPower = 10;
    [SerializeField] protected float _attackSpeed = 1.0f;
    [SerializeField] protected float _moveSpeed = 5.0f;
    [SerializeField] protected float _attackRange = 2.0f;
    [SerializeField] protected float _detectionRange = 10.0f;
    [Header("UI")]
    [SerializeField] private HealthBar _healthBar;

    // 프로퍼티
    public int Hp 
    { 
        get => _hp; 
        protected set => _hp = value; 
    }

    public int AttackPower 
    { 
        get => _attackPower; 
        protected set => _attackPower = value; 
    }

    public float AttackSpeed 
    { 
        get => _attackSpeed; 
        protected set => _attackSpeed = value; 
    }

    public float MoveSpeed 
    { 
        get => _moveSpeed; 
        protected set => _moveSpeed = value; 
    }

    public float AttackRange 
    { 
        get => _attackRange; 
        protected set => _attackRange = value; 
    }

    public float DetectionRange 
    { 
        get => _detectionRange; 
        protected set => _detectionRange = value; 
    }

    protected virtual void Awake()
    {
        InitializeHealthBar();
    }

    /// <summary>
    /// 공격 메서드 (추상)
    /// </summary>
    public abstract void Attack();

    /// <summary>
    /// 피격 메서드 (추상)
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    public abstract void TakeDamage(int damage);

    /// <summary>
    /// 체력이 0 이하인지 확인
    /// </summary>
    /// <returns>체력이 0 이하면 true</returns>
    protected bool IsDead()
    {
        return _hp <= 0;
    }

    protected void InitializeHealthBar()
    {
        if (_healthBar != null)
        {
            _healthBar.Initialize(transform, _hp);
        }
    }

    protected void UpdateHealthBar()
    {
        _healthBar?.SetValue(_hp);
    }
}


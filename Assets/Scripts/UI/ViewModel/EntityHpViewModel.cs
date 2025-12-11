using System;
using UI;
using UnityEngine;

public class EntityHpViewModel : IViewModel
{
    public event Action OnStateChanged;

    private readonly Entity _entity;
    public string Name { get; private set; }

    public float HpRatio => _entity != null && _entity.MaxHp > 0
        ? (float)_entity.Hp / _entity.MaxHp
        : 0f;

    public string HpText => _entity != null
        ? $"{_entity.Hp} / {_entity.MaxHp}"
        : "0 / 0";

    public EntityHpViewModel(Entity entity, string nameOverride = null)
    {
        _entity = entity;
        Name = nameOverride ?? _entity.name;

        // 체력 변경 감지
        if (_entity != null)
        {
            _entity.NetHp.OnValueChanged += OnHpChanged;
            _entity.NetMaxHp.OnValueChanged += OnHpChanged;
        }
    }

    private void OnHpChanged(int prev, int cur)
    {
        OnStateChanged?.Invoke();
    }

    // ViewModel이 해제될 때(View에서 연결 끊길 때 등) 필요하다면 호출
    public void Dispose()
    {
        if (_entity != null)
        {
            _entity.NetHp.OnValueChanged -= OnHpChanged;
            _entity.NetMaxHp.OnValueChanged -= OnHpChanged;
        }
    }
}
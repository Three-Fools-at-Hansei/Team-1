using System;
using UnityEngine;

[Serializable]
public class ReactiveProperty<T>
{
    public event Action<T> OnValueChanged;

    [SerializeField]
    private T _value;

    public T Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    public ReactiveProperty() { }

    public ReactiveProperty(T initialValue)
    {
        _value = initialValue;
    }

    public void SetValueWithoutNotify(T value)
    {
        _value = value;
    }

    public override string ToString() => _value?.ToString() ?? "null";
}
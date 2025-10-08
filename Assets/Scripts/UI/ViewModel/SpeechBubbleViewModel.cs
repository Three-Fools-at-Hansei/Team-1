using System;
using UI;
using UnityEngine;

public class SpeechBubbleViewModel : IViewModel
{
    public event Action OnStateChanged;

    public Transform Target { get; private set; }
    public string Message { get; private set; } = string.Empty;

    public void Init(Transform target, string initialMessage)
    {
        Target = target;
        Message = initialMessage ?? string.Empty;
        OnStateChanged?.Invoke();
    }

    public void SetMessage(string message)
    {
        Message = message ?? string.Empty;
        OnStateChanged?.Invoke();
    }
}

using System;
using UI;

public class PopupTestViewModel : IViewModel
{
    public event Action OnStateChanged;
    public event Action OnEscapeKeyDown;

    public string Title { get; private set; } = "�׽�Ʈ �˾�";
    public int ClickCount = 0;

    public void OnEscape()
    {
        OnEscapeKeyDown?.Invoke();
    }

    public void OnConfirm()
    {
        ClickCount++;
        Title = $"Ȯ�� ��ư�� {ClickCount}�� Ŭ���Ǿ����ϴ�.";

        if (ClickCount >= 10)
        {
            OnEscape();
            return;
        }

        OnStateChanged?.Invoke();
    }
}
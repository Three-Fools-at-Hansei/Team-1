using System;
using UI;

public class PopupTestViewModel : ViewModelBase
{
    public override event Action OnStateChanged;
    public event Action OnEscapeKeyDown;

    public string Title { get; private set; } = "테스트 팝업";
    public int ClickCount = 0;

    public void OnEscape()
    {
        OnEscapeKeyDown?.Invoke();
    }

    public void OnConfirm()
    {
        ClickCount++;
        Title = $"확인 버튼이 {ClickCount}번 클릭되었습니다.";

        if (ClickCount >= 10)
        {
            OnEscape();
            return;
        }

        OnStateChanged?.Invoke();
    }
}
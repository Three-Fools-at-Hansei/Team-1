using System;

/// <summary>
/// 게임 시작 확인 팝업의 ViewModel입니다.
/// 메시지 텍스트를 관리합니다.
/// </summary>
public class GameStartConfirmPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    private string _messageText = "게임을 시작할까요?";
    public string MessageText => _messageText;

    public void SetMessage(string message)
    {
        _messageText = message;
        OnStateChanged?.Invoke();
    }
}


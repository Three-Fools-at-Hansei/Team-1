using System;
using UI;

/// <summary>
/// 게임 시작 확인 팝업의 ViewModel입니다.
/// 메시지 텍스트와 확인 시 실행할 콜백을 관리합니다.
/// </summary>
public class GameStartConfirmPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    private string _messageText = "게임을 시작할까요?";
    public string MessageText => _messageText;

    // 확인 버튼 클릭 시 실행할 로직 (예: 방 생성)
    public Action OnConfirmAction { get; set; }

    public void SetMessage(string message)
    {
        _messageText = message;
        OnStateChanged?.Invoke();
    }

    public void Confirm()
    {
        OnConfirmAction?.Invoke();
    }
}
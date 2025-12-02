using System;

/// <summary>
/// 게임 참가 팝업의 ViewModel입니다.
/// 게임 코드 입력 상태를 관리합니다.
/// </summary>
public class GameJoinPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    private string _messageText = "게임 코드 입력";
    private string _gameCode = string.Empty;

    public string MessageText => _messageText;
    public string GameCode => _gameCode;

    /// <summary>
    /// 게임 코드를 설정합니다.
    /// </summary>
    public void SetGameCode(string code)
    {
        _gameCode = code ?? string.Empty;
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// 게임 코드가 유효한지 검증합니다.
    /// </summary>
    public bool IsValidGameCode()
    {
        return !string.IsNullOrWhiteSpace(_gameCode);
    }
}


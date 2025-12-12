using System;
using UnityEngine;

/// <summary>
/// 게임 참가 팝업의 ViewModel입니다.
/// 게임 코드 입력 상태 및 입장 로직을 관리합니다.
/// </summary>
public class GameJoinPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;
    // 팝업 닫기 요청 이벤트
    public event Action OnCloseRequested;

    private string _messageText = "게임 코드 입력";
    private string _gameCode = string.Empty;
    private bool _isBusy = false;

    public string MessageText => _messageText;
    public string GameCode => _gameCode;
    public bool IsInteractable => !_isBusy;

    /// <summary>
    /// 게임 코드를 설정합니다.
    /// </summary>
    public void SetGameCode(string code)
    {
        _gameCode = code ?? string.Empty;
    }

    /// <summary>
    /// 입장 시도 (비동기)
    /// </summary>
    public async void TryJoinGame()
    {
        if (string.IsNullOrWhiteSpace(_gameCode))
        {
            SetMessage("코드를 입력해주세요.");
            return;
        }

        if (_isBusy) return;

        // UI 잠금 및 상태 표시
        _isBusy = true;
        SetMessage("접속 중...");
        OnStateChanged?.Invoke();

        // 1. 네트워크 매니저를 통해 접속 시도
        var result = await Managers.Network.JoinByCodeAsync(_gameCode);

        _isBusy = false;

        if (result.success)
        {
            // 성공: 팝업 닫기 (이후 NetworkManager가 Scene 전환)
            Debug.Log("[GameJoinPopup] 접속 성공. 팝업을 닫습니다.");
            OnCloseRequested?.Invoke();
        }
        else
        {
            // 실패: 에러 메시지를 UI에 표시 (빨간색 등 뷰에서 처리 가능하도록 메시지 변경)
            // 예: "게임이 진행 중이거나 입장할 수 없습니다."
            SetMessage(result.message);
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void Close()
    {
        OnCloseRequested?.Invoke();
    }

    private void SetMessage(string msg)
    {
        _messageText = msg;
        OnStateChanged?.Invoke();
    }
}
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

        // [수정] 1. 로딩 팝업 표시
        var loadingVM = new LoadingViewModel();
        loadingVM.Report(0f);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);

        // 2. 접속 시도
        var result = await Managers.Network.JoinByCodeAsync(_gameCode);

        _isBusy = false;

        if (result.success)
        {
            Debug.Log("[GameJoinPopup] 접속 성공. 팝업 닫기.");
            // 성공 시 입력 팝업만 닫음 (로딩 팝업은 씬 전환/동기화 완료까지 유지)
            OnCloseRequested?.Invoke();
        }
        else
        {
            // 실패 시 로딩 팝업 닫기 및 에러 표시
            loadingVM.Report(1.0f);
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
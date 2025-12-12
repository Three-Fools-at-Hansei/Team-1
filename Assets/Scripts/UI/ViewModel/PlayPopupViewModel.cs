using System;
using UI;
using UnityEngine;

/// <summary>
/// Play 팝업의 ViewModel입니다.
/// 게임 참가(Join) 및 게임 생성 팝업(Create) 호출 로직을 담당합니다.
/// </summary>
public class PlayPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    public event Action<GameStartConfirmPopupViewModel> OnCreateGamePopupRequested;
    public event Action<GameJoinPopupViewModel> OnJoinGamePopupRequested;

    /// <summary>
    /// 게임 참가 (Quick Join)
    /// </summary>
    public void JoinGame()
    {
        Debug.Log("[PlayPopupViewModel] 게임 참가(QuickJoin) 요청");
        OnJoinGamePopupRequested?.Invoke(new GameJoinPopupViewModel());
    }

    /// <summary>
    /// 게임 생성 팝업 요청
    /// </summary>
    public void ShowCreateGamePopup()
    {
        Debug.Log("[PlayPopupViewModel] 게임 생성 확인 팝업 요청 이벤트 발생");
        OnCreateGamePopupRequested?.Invoke(new GameStartConfirmPopupViewModel());
    }
}
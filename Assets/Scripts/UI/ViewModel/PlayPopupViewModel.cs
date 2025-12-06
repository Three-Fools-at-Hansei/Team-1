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

    // View가 구독할 이벤트 정의 (매개변수로 다음 팝업의 ViewModel을 전달)
    public event Action<GameStartConfirmPopupViewModel> OnCreateGamePopupRequested;

    /// <summary>
    /// 게임 참가 (Quick Join)
    /// </summary>
    public async void JoinGame()
    {
        Debug.Log("[PlayPopupViewModel] 게임 참가(QuickJoin) 요청");

        // 빠른 참가 시도
        bool success = await Managers.Network.QuickJoinAsync();

        if (success)
        {
            Debug.Log("[PlayPopupViewModel] 참가 성공! 호스트의 씬으로 이동 대기 중...");
        }
        else
        {
            Debug.LogWarning("[PlayPopupViewModel] 참가 실패.");
        }
    }

    /// <summary>
    /// 게임 생성 팝업 요청
    /// </summary>
    public void ShowCreateGamePopup()
    {
        Debug.Log("[PlayPopupViewModel] 게임 생성 확인 팝업 요청 이벤트 발생");

        // 새로운 ViewModel을 생성하여 이벤트와 함께 전달
        // ViewModel은 View(UI_GameStartConfirmPopup)를 몰라도 됩니다.
        var confirmVm = new GameStartConfirmPopupViewModel();

        // View에게 "팝업을 띄워달라"고 요청
        OnCreateGamePopupRequested?.Invoke(confirmVm);
    }
}
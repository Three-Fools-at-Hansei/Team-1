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
            // 성공 시, NetworkManager에 의해 자동으로 씬 동기화가 이루어지므로
            // 별도의 씬 로드 호출은 필요 없습니다.
            // 필요하다면 여기서 팝업을 닫거나 "입장 중..." UI를 표시할 수 있습니다.
        }
        else
        {
            Debug.LogWarning("[PlayPopupViewModel] 참가 실패. (방이 없거나 오류)");
            // 실패 알림 처리 (추후 구현)
        }
    }

    /// <summary>
    /// 게임 생성 확인 팝업 표시
    /// </summary>
    public async void ShowCreateGamePopup()
    {
        Debug.Log("[PlayPopupViewModel] 게임 생성 확인 팝업 표시");
        await Managers.UI.ShowAsync<UI_GameStartConfirmPopup>(new GameStartConfirmPopupViewModel());
    }
}
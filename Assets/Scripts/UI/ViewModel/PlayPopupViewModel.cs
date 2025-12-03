using System;
using UI;
using UnityEngine;

/// <summary>
/// Play 팝업의 ViewModel입니다.
/// 게임 생성 및 참가 로직을 담당합니다.
/// </summary>
public class PlayPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    /// <summary>
    /// 게임 생성 버튼 클릭 시 호출
    /// </summary>
    public async void CreateGame()
    {
        // 확인 팝업 ViewModel 생성
        var confirmVM = new GameStartConfirmPopupViewModel();
        confirmVM.SetMessage("방을 생성하고 전투를 시작하시겠습니까?");

        // 확인 버튼 클릭 시 실제 방 생성 로직 연결
        confirmVM.OnConfirmAction = async () =>
        {
            Debug.Log("[PlayPopup] 방 생성 시도...");
            bool success = await Managers.Network.StartHostAsync();
            if (!success)
            {
                Debug.LogError("[PlayPopup] 방 생성 실패!");
                // 필요하다면 실패 알림 팝업 등을 띄울 수 있음
            }
            // 성공 시 NetworkManagerEx에서 자동으로 SceneLoad("CombatScene")을 수행함
        };

        // 팝업 표시
        await Managers.UI.ShowAsync<UI_GameStartConfirmPopup>(confirmVM);
    }

    /// <summary>
    /// 게임 참가 버튼 클릭 시 호출
    /// </summary>
    public async void JoinGame()
    {
        Debug.Log("[PlayPopup] 빠른 참가 시도...");
        bool success = await Managers.Network.QuickJoinAsync();

        if (success)
        {
            Debug.Log("[PlayPopup] 참가 성공! 호스트 대기 중...");
        }
        else
        {
            Debug.LogError("[PlayPopup] 참가 실패 (방이 없거나 오류 발생)");
        }
    }
}

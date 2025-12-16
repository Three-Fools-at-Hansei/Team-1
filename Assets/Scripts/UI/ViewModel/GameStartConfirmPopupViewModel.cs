using System;
using UnityEngine;

/// <summary>
/// 게임 시작 확인 팝업의 ViewModel입니다.
/// 호스트로 게임을 시작하는 로직을 포함합니다.
/// </summary>
public class GameStartConfirmPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    // 팝업을 닫아야 할 때 발생하는 이벤트
    public event Action OnCloseRequested;

    private string _messageText = "게임을 생성하시겠습니까?";
    public string MessageText => _messageText;

    public void SetMessage(string message)
    {
        _messageText = message;
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// 게임 시작 확정 (Host 시작)
    /// </summary>
    public async void ConfirmGameStart()
    {
        Debug.Log("[GameStartConfirmPopupViewModel] 호스트 시작 요청");

        var loadingVM = new LoadingViewModel();
        loadingVM.Report(0f);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);

        // 2. 호스트 시작 (성공 시 내부적으로 CombatScene 로드 시작)
        bool success = await Managers.Network.StartHostAsync();

        if (success)
        {
            Debug.Log("[GameStartConfirm] 호스트 시작 성공. 팝업 닫기.");

            // [중요] 성공 시 이 팝업(확인창)만 닫습니다.
            // 로딩 팝업은 Scene 전환이 완료될 때까지 화면에 남아있어야 합니다.
            OnCloseRequested?.Invoke();
        }
        else
        {
            Debug.LogError("[GameStartConfirm] 호스트 시작 실패.");

            // 실패 시 로딩 팝업을 즉시 닫아줍니다.
            loadingVM.Report(1.0f);
        }
    }

    /// <summary>
    /// 취소 버튼 클릭 시 처리
    /// </summary>
    public void Cancel()
    {
        OnCloseRequested?.Invoke();
    }
}
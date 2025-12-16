using System;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public enum eCombatResult
{
    Victory,
    Defeat
}

public class CombatResultViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    public eCombatResult Result { get; private set; }
    public string TitleText => Result == eCombatResult.Victory ? "승리" : "패배";
    public string ButtonText => "로비로 돌아가기";

    public void SetResult(eCombatResult result)
    {
        Result = result;
        OnStateChanged?.Invoke();
    }

    public async void GoToLobby()
    {
        // 네트워크 연결 해제 (Shutdown)
        Managers.Network.Clear();

        // [수정] 로딩 팝업과 함께 씬 전환
        var loadingVM = new LoadingViewModel();
        var loadingUI = await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);

        try
        {
            // 메인(로비) 씬으로 이동
            await Managers.Scene.LoadSceneAsync(eSceneType.MainScene, loadingVM);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LoginViewModel] 씬 전환 실패: {ex}");
            Managers.UI.Close(loadingUI); // 실패 시 닫기
        }
    }
}
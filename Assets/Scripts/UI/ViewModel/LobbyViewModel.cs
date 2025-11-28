using System;
using System.Threading.Tasks;
using UI;

/// <summary>
/// 로비 화면의 ViewModel입니다.
/// 버튼 클릭 시 각각의 팝업을 표시하는 로직을 담당합니다.
/// </summary>
public class LobbyViewModel : IViewModel
{
    public event Action OnStateChanged;

    public string VersionText { get; private set; } = "Ver. 0.0.0.1d";

    public async void OnClickPlay()
    {
        // Play 팝업 표시
        await Managers.UI.ShowAsync<UI_PlayPopup>(new PlayPopupViewModel());
    }

    public async void OnClickSetting()
    {
        // Setting 팝업 표시
        await Managers.UI.ShowAsync<UI_SettingPopup>(new SettingPopupViewModel());
    }

    public async void OnClickExit()
    {
        // Exit 확인 팝업 표시
        await Managers.UI.ShowAsync<UI_ExitPopup>(new ExitPopupViewModel());
    }
}




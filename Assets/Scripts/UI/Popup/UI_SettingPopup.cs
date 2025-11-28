using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Setting 버튼 클릭 시 표시되는 팝업입니다.
/// 현재는 기본 구조만 구현되어 있으며, 내용은 추후 구현 예정입니다.
/// </summary>
public class UI_SettingPopup : UI_Popup
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private TMP_Text _contentText;

    protected override void Awake()
    {
        base.Awake();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);
    }

    private void OnClickClose()
    {
        Managers.UI.Close(this);
    }

    protected override void OnStateChanged()
    {
        // TODO: ViewModel에서 데이터를 받아 UI를 업데이트하는 로직 구현 예정
        // if (ViewModel is SettingPopupViewModel vm && _contentText != null)
        // {
        //     _contentText.text = vm.ContentText;
        // }
    }
}




using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exit 버튼 클릭 시 표시되는 확인 팝업입니다.
/// "정말 종료하시겠습니까?" 메시지와 예/아니오 버튼을 제공합니다.
/// </summary>
public class UI_ExitPopup : UI_Popup
{
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private TMP_Text _messageText;

    protected override void Awake()
    {
        base.Awake();

        if (_yesButton != null)
            _yesButton.onClick.AddListener(OnClickYes);

        if (_noButton != null)
            _noButton.onClick.AddListener(OnClickNo);
    }

    private void OnClickYes()
    {
        // TODO: 게임 종료 로직 구현 예정
        // Application.Quit(); 또는 게임 종료 처리
        Debug.Log("[UI_ExitPopup] 게임 종료 요청 (구현 예정)");
        
        Managers.UI.Close(this);
    }

    private void OnClickNo()
    {
        Managers.UI.Close(this);
    }

    protected override void OnStateChanged()
    {
        if (ViewModel is ExitPopupViewModel vm && _messageText != null)
        {
            _messageText.text = vm.MessageText;
        }
    }
}




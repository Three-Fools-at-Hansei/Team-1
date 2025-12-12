using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 추가

/// <summary>
/// Exit 버튼 클릭 시 표시되는 확인 팝업입니다.
/// "정말 종료하시겠습니까?" 메시지와 예/아니오 버튼을 제공합니다.
/// </summary>
public class UI_ExitPopup : UI_Popup
{
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private TMP_Text _messageText;

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

    protected override void Awake()
    {
        base.Awake();

        // [연출] 초기화
        _fadeIn = new FadeInUIAnimation(0.3f, Ease.OutBack);
        _fadeOut = new FadeOutUIAnimation(0.2f, Ease.InQuad);
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        if (_yesButton != null)
            _yesButton.onClick.AddListener(OnClickYes);

        if (_noButton != null)
            _noButton.onClick.AddListener(OnClickNo);
    }

    // [연출] 등장 애니메이션
    private void OnEnable()
    {
        _fadeIn?.ExecuteAsync(_canvasGroup);
    }

    // [연출] 퇴장 애니메이션 적용
    private async void OnClickYes()
    {
        Managers.Sound.PlaySFX("Select");
        // TODO: 게임 종료 로직 구현 예정
        // Application.Quit(); 또는 게임 종료 처리
        Debug.Log("[UI_ExitPopup] 게임 종료 요청 (구현 예정)");

        if (_fadeOut != null)
        {
            await _fadeOut.ExecuteAsync(_canvasGroup);
        }

        Managers.UI.Close(this);
    }

    // [연출] 퇴장 애니메이션 적용
    private async void OnClickNo()
    {
        Managers.Sound.PlaySFX("Select");

        if (_fadeOut != null)
        {
            await _fadeOut.ExecuteAsync(_canvasGroup);
        }

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
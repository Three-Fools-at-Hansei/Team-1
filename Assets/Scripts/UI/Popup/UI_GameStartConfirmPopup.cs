using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 추가

/// <summary>
/// 게임 생성 버튼 클릭 시 표시되는 확인 팝업입니다.
/// "게임을 시작할까요?" 메시지와 확인/취소 버튼을 제공합니다.
/// </summary>
public class UI_GameStartConfirmPopup : UI_Popup
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private GameStartConfirmPopupViewModel _viewModel;

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

    public override void SetViewModel(IViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        _viewModel = viewModel as GameStartConfirmPopupViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested += OnCloseRequested;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // [연출] 초기화
        _fadeIn = new FadeInUIAnimation(0.3f, Ease.OutBack);
        _fadeOut = new FadeOutUIAnimation(0.2f, Ease.InQuad);
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        if (_confirmButton != null) _confirmButton.onClick.AddListener(OnClickConfirm);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(OnClickCancel);
    }

    // [연출] 등장 애니메이션
    private void OnEnable()
    {
        _fadeIn?.ExecuteAsync(_canvasGroup);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null || _messageText == null)
            return;

        _messageText.text = _viewModel.MessageText;
    }

    private void OnClickConfirm()
    {
        Managers.Sound.PlaySFX("Select");
        _viewModel?.ConfirmGameStart();
    }

    private void OnClickCancel()
    {
        Managers.Sound.PlaySFX("Select");
        _viewModel?.Cancel();
    }

    /// <summary>
    /// ViewModel이 닫기를 요청했을 때 실행
    /// </summary>
    // [연출] 퇴장 애니메이션 적용
    private async void OnCloseRequested()
    {
        if (_fadeOut != null)
        {
            await _fadeOut.ExecuteAsync(_canvasGroup);
        }

        Managers.UI.Close(this);
    }
}
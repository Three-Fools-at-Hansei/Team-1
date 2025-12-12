using UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 추가

/// <summary>
/// Play 버튼 클릭 시 표시되는 팝업입니다.
/// 게임 생성 및 게임 참가 버튼을 제공합니다.
/// </summary>
public class UI_PlayPopup : UI_Popup
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _createGameButton;  // 게임 생성 버튼
    [SerializeField] private Button _joinGameButton;    // 게임 참가 버튼

    private PlayPopupViewModel _viewModel;

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

    public override void SetViewModel(IViewModel viewModel)
    {
        // 기존 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnCreateGamePopupRequested -= OnCreateGamePopupRequested;
            _viewModel.OnJoinGamePopupRequested -= OnJoinGamePopupRequested;
        }

        _viewModel = viewModel as PlayPopupViewModel;
        base.SetViewModel(viewModel);

        // 새 구독 연결
        if (_viewModel != null)
        {
            _viewModel.OnCreateGamePopupRequested += OnCreateGamePopupRequested;
            _viewModel.OnJoinGamePopupRequested += OnJoinGamePopupRequested;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // [연출] 초기화
        _fadeIn = new FadeInUIAnimation(0.3f, Ease.OutQuad);
        _fadeOut = new FadeOutUIAnimation(0.2f, Ease.InQuad);
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);
    }

    private void OnEnable()
    {
        if (_createGameButton != null) _createGameButton.onClick.AddListener(OnClickCreateGame);
        if (_joinGameButton != null) _joinGameButton.onClick.AddListener(OnClickJoinGame);

        // [연출] 등장 애니메이션
        _fadeIn?.ExecuteAsync(_canvasGroup);
    }

    private void OnDisable()
    {
        if (_createGameButton != null) _createGameButton.onClick.RemoveListener(OnClickCreateGame);
        if (_joinGameButton != null) _joinGameButton.onClick.RemoveListener(OnClickJoinGame);
    }

    // [연출] 퇴장 애니메이션 적용
    private async void OnClickClose()
    {
        Managers.Sound.PlaySFX("Select");

        if (_fadeOut != null)
        {
            await _fadeOut.ExecuteAsync(_canvasGroup);
        }

        Managers.UI.Close(this);
    }

    private void OnClickCreateGame()
    {
        Managers.Sound.PlaySFX("Select");
        _viewModel?.ShowCreateGamePopup();
    }

    private void OnClickJoinGame()
    {
        Managers.Sound.PlaySFX("Select");
        _viewModel?.JoinGame();
    }

    private async void OnCreateGamePopupRequested(GameStartConfirmPopupViewModel vm)
    {
        await Managers.UI.ShowAsync<UI_GameStartConfirmPopup>(vm);
    }

    private async void OnJoinGamePopupRequested(GameJoinPopupViewModel vm)
    {
        await Managers.UI.ShowAsync<UI_GameJoinPopup>(vm);
    }

    protected override void OnStateChanged() { }
}
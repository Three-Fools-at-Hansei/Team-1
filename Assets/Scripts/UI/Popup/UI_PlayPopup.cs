using UI;
using UnityEngine;
using UnityEngine.UI;

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

    public override void SetViewModel(IViewModel viewModel)
    {
        // 기존 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnCreateGamePopupRequested -= OnCreateGamePopupRequested;
        }

        _viewModel = viewModel as PlayPopupViewModel;
        base.SetViewModel(viewModel);

        // 새 구독 연결
        if (_viewModel != null)
        {
            _viewModel.OnCreateGamePopupRequested += OnCreateGamePopupRequested;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);
    }

    private void OnEnable()
    {
        if (_createGameButton != null) _createGameButton.onClick.AddListener(OnClickCreateGame);
        if (_joinGameButton != null) _joinGameButton.onClick.AddListener(OnClickJoinGame);
    }

    private void OnDisable()
    {
        if (_createGameButton != null) _createGameButton.onClick.RemoveListener(OnClickCreateGame);
        if (_joinGameButton != null) _joinGameButton.onClick.RemoveListener(OnClickJoinGame);
    }

    private void OnClickClose()
    {
        Managers.Sound.PlaySFX("Select");
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

    /// <summary>
    /// ViewModel의 요청에 의해 실제 팝업 UI를 띄우는 로직 (View의 역할)
    /// </summary>
    private async void OnCreateGamePopupRequested(GameStartConfirmPopupViewModel vm)
    {
        await Managers.UI.ShowAsync<UI_GameStartConfirmPopup>(vm);
    }

    /// <summary>
    /// 게임 생성/참가 버튼 리스너를 제거합니다.
    /// </summary>
    private void UnregisterGameButtonsListeners()
    {
        if (_createGameButton != null)
            _createGameButton.onClick.RemoveListener(OnClickCreateGame);

        if (_joinGameButton != null)
            _joinGameButton.onClick.RemoveListener(OnClickJoinGame);
    }

    protected override void OnStateChanged()
    {
        // ViewModel에서 데이터를 받아 UI를 업데이트하는 로직이 필요할 경우 여기에 구현
        // 예: 버튼 활성화/비활성화, 버튼 텍스트 변경 등
    }
}


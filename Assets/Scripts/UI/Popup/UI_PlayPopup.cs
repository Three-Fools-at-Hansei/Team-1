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
        _viewModel = viewModel as PlayPopupViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void Awake()
    {
        base.Awake();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);
    }

    private void OnEnable()
    {
        RegisterGameButtonsListeners();
    }

    private void OnDisable()
    {
        UnregisterGameButtonsListeners();
    }

    private void RegisterGameButtonsListeners()
    {
        if (_createGameButton != null)
            _createGameButton.onClick.AddListener(OnClickCreateGame);

        if (_joinGameButton != null)
            _joinGameButton.onClick.AddListener(OnClickJoinGame);
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

    private void OnClickClose()
    {
        Managers.UI.Close(this);
    }

    /// <summary>
    /// 게임 생성 버튼 클릭 -> ViewModel 위임
    /// </summary>
    private void OnClickCreateGame()
    {
        _viewModel?.ShowCreateGamePopup();
    }

    /// <summary>
    /// 게임 참가 버튼 클릭 -> ViewModel 위임
    /// </summary>
    private void OnClickJoinGame()
    {
        _viewModel?.JoinGame();
    }

    protected override void OnStateChanged()
    {
        // 상태 변경 시 UI 갱신 로직 (필요 시 구현)
    }
}




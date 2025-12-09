using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_CombatHUD : UI_View
{
    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _lobbyButton;

    private CombatHUDViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();
        if (_startGameButton != null)
            _startGameButton.onClick.AddListener(() => _viewModel?.OnClickStartGame());

        if (_lobbyButton != null)
            _lobbyButton.onClick.AddListener(() => _viewModel?.OnClickGoLobby());
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as CombatHUDViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        if (_waveText != null) _waveText.text = _viewModel.WaveText;
        if (_statusText != null) _statusText.text = _viewModel.StatusText;

        if (_startGameButton != null)
            _startGameButton.gameObject.SetActive(_viewModel.IsStartButtonVisible);

        if (_lobbyButton != null)
            _lobbyButton.gameObject.SetActive(_viewModel.IsLobbyButtonVisible);
    }
}
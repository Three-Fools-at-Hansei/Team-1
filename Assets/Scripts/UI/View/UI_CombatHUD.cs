using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_CombatHUD : UI_View
{
    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _roomCodeText;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _returnButton;

    private CombatHUDViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_startGameButton != null)
        {
            _startGameButton.onClick.AddListener(() =>
            {
                Managers.Sound.PlaySFX("Select");
                _viewModel?.OnClickStartGame();
            });
        }

        // [New] 로비 복귀 버튼 이벤트 연결
        if (_returnButton != null)
        {
            _returnButton.onClick.AddListener(() =>
            {
                Managers.Sound.PlaySFX("Select");
                _viewModel?.OnClickReturnToLobby();
            });
        }
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
        if (_roomCodeText != null) _roomCodeText.text = _viewModel.RoomCodeText;

        if (_startGameButton != null)
            _startGameButton.gameObject.SetActive(_viewModel.IsStartButtonVisible);
        if (_returnButton != null)
            _returnButton.gameObject.SetActive(_viewModel.IsReturnButtonVisible);
    }
}
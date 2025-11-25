using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainLobby : UI_View
{
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private TMP_Text _statusText;

    private MainLobbyViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_createButton != null)
            _createButton.onClick.AddListener(() => _viewModel?.CreateRoom());

        if (_joinButton != null)
            _joinButton.onClick.AddListener(() => _viewModel?.JoinRoom());
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as MainLobbyViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        if (_statusText != null)
            _statusText.text = _viewModel.StatusText;

        if (_createButton != null)
            _createButton.interactable = _viewModel.IsInteractable;

        if (_joinButton != null)
            _joinButton.interactable = _viewModel.IsInteractable;
    }
}
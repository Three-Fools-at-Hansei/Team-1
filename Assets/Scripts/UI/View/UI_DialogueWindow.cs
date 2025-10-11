using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_DialogueWindow : UI_Popup
{
    [SerializeField] private TMP_Text _npcNameText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _closeButton;

    private DialogueWindowViewModel _viewModel;
    public override string ActionMapKey => "UI_DialogueWindow";

    public override void SetViewModel(IViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnStateChanged -= OnStateChanged;
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        base.SetViewModel(viewModel);
        _viewModel = viewModel as DialogueWindowViewModel;

        if (_viewModel != null)
        {
            _viewModel.OnStateChanged += OnStateChanged;
            _viewModel.OnCloseRequested += OnCloseRequested;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (_nextButton != null) _nextButton.onClick.AddListener(OnClickNext);
        if (_closeButton != null) _closeButton.onClick.AddListener(OnClickClose);
    }

    private void OnClickNext() => _viewModel?.Next();
    private void OnClickClose() => _viewModel?.Close();

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;
        if (_npcNameText != null) _npcNameText.text = _viewModel.NpcName;
        if (_bodyText != null) _bodyText.text = _viewModel.CurrentLine;
    }

    private void OnCloseRequested() => Managers.UI.Close(this);
}

using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 추가

public class UI_DialogueWindow : UI_Popup
{
    [SerializeField] private TMP_Text _npcNameText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _closeButton;

    private DialogueWindowViewModel _viewModel;
    public override string ActionMapKey => "UI_DialogueWindow";

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

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

        // [연출] 초기화
        _fadeIn = new FadeInUIAnimation(0.3f, Ease.OutQuad);
        _fadeOut = new FadeOutUIAnimation(0.2f, Ease.InQuad);
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        if (_nextButton != null) _nextButton.onClick.AddListener(OnClickNext);
        if (_closeButton != null) _closeButton.onClick.AddListener(OnClickClose);
    }

    // [연출] 등장 애니메이션
    private void OnEnable()
    {
        _fadeIn?.ExecuteAsync(_canvasGroup);
    }

    private void OnClickNext() => _viewModel?.Next();
    private void OnClickClose() => _viewModel?.Close();

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;
        if (_npcNameText != null) _npcNameText.text = _viewModel.NpcName;
        if (_bodyText != null) _bodyText.text = _viewModel.CurrentLine;
    }

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
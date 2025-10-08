using TMPro;
using UI;
using UnityEngine;

public class UI_SpeechBubble : UI_View
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 1.2f, 0f);

    private SpeechBubbleViewModel _viewModel;
    private RectTransform _rect;

    public override void SetViewModel(IViewModel viewModel)
    {
        base.SetViewModel(viewModel);
        _viewModel = viewModel as SpeechBubbleViewModel;
    }

    protected override void Awake()
    {
        base.Awake();
        _rect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (_viewModel == null || _viewModel.Target == null) return;

        Vector3 worldPos = _viewModel.Target.position + _worldOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        _rect.position = screenPos;
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;
        if (_messageText != null)
            _messageText.text = _viewModel.Message ?? string.Empty;
    }
}

using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_PopupTest : UI_Popup
{
    public override string ActionMapKey => "UI_PopupTest";

    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _titleText;

    private PopupTestViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnEscapeKeyDown -= OnClose;

        base.SetViewModel(viewModel);

        _viewModel = viewModel as PopupTestViewModel;

        _viewModel.OnEscapeKeyDown += OnClose;
    }

    protected override void Awake()
    {
        base.Awake();

        Managers.Input.BindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
        _confirmButton.onClick.AddListener(OnConfirmAction);

    }

    private void OnEscapeAction(InputAction.CallbackContext context) => _viewModel?.OnEscape();
    private void OnConfirmAction() => _viewModel?.OnConfirm();


    protected override void OnStateChanged()
    {
        if (_viewModel == null) 
            return;

        _titleText.text = _viewModel.Title;
    }

    void OnClose()
    {
        // ���⿡ UI fade out ���� ������ ���� �� �ְ���.

        Debug.Log("UI_PopupTest �� �ݽ��ϴ�.");
        Managers.UI.Close(this);
    }
}
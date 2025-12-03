using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameStartConfirmPopup : UI_Popup
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private GameStartConfirmPopupViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_confirmButton != null)
            _confirmButton.onClick.AddListener(OnClickConfirm);

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(OnClickCancel);
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as GameStartConfirmPopupViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null || _messageText == null)
            return;

        _messageText.text = _viewModel.MessageText;
    }

    private void OnClickConfirm()
    {
        // ViewModel에 위임된 확인 로직(방 생성 등) 실행
        _viewModel?.Confirm();
        Managers.UI.Close(this);
    }

    private void OnClickCancel()
    {
        Managers.UI.Close(this);
    }
}
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_PopupHello : UI_Popup
{
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TMP_Text _titleText;

protected override void Awake()
{
    base.Awake();

    if (_confirmButton != null)
        _confirmButton.onClick.AddListener(OnClickConfirm);
}

    public override void SetViewModel(IViewModel viewModel)
    {
        base.SetViewModel(viewModel);
    }

    private void OnClickConfirm()
    {
        Managers.UI.Close(this);
    }

    protected override void OnStateChanged()
    {
        if (ViewModel is HelloPopupViewModel vm && _titleText != null)
        {
            _titleText.text = vm.Title;
        }
    }
}

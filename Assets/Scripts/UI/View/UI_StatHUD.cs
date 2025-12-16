using TMPro;
using UI;
using UnityEngine;

public class UI_StatHUD : UI_View
{
    [Header("Stats Texts")]
    [SerializeField] private TMP_Text _atkText;
    [SerializeField] private TMP_Text _atkSpeedText;
    [SerializeField] private TMP_Text _moveSpeedText;

    private StatHUDViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as StatHUDViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        if (_atkText != null) _atkText.text = _viewModel.AtkText;
        if (_atkSpeedText != null) _atkSpeedText.text = _viewModel.AtkSpeedText;
        if (_moveSpeedText != null) _moveSpeedText.text = _viewModel.MoveSpeedText;
    }
}
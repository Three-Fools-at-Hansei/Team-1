using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_EntityHp : UI_View
{
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _nameText;

    private EntityHpViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as EntityHpViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        if (_hpSlider != null)
            _hpSlider.value = _viewModel.HpRatio;

        if (_hpText != null)
            _hpText.text = _viewModel.HpText;

        if (_nameText != null)
            _nameText.text = _viewModel.Name;
    }
}
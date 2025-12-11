using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보상 팝업 내의 개별 카드 UI
/// </summary>
public class UI_RewardItem : UI_View
{
    [SerializeField] private TMP_Text _typeText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private Button _selectButton;

    private RewardItemViewModel _viewModel;
    private UI_RewardPopup _parentPopup;

    public void Init(RewardItemViewModel viewModel, UI_RewardPopup parent)
    {
        _viewModel = viewModel;
        _parentPopup = parent;

        // ViewModelBase가 아닌 단순 POCO ViewModel이므로 직접 세팅
        if (_typeText) _typeText.text = _viewModel.TypeText;
        if (_descText) _descText.text = _viewModel.Desc;
    }

    // UI_View 필수 구현 (사용 안 함)
    public override void SetViewModel(IViewModel viewModel) { }
    protected override void OnStateChanged() { }

    protected override void Awake()
    {
        base.Awake();
        if (_selectButton)
            _selectButton.onClick.AddListener(OnClickSelect);
    }

    private void OnClickSelect()
    {
        Managers.Sound.PlaySFX("Select");

        _viewModel?.Select();

        // 부모 팝업 닫기
        if (_parentPopup != null)
            Managers.UI.Close(_parentPopup);
    }
}
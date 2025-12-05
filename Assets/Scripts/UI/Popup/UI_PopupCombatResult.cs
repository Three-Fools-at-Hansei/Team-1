using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_PopupCombatResult : UI_Popup
{
    public override string ActionMapKey => "None";

    [Header("UI References")]
    [SerializeField] private Image _backgroundPanel;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TMP_Text _buttonText;

    private CombatResultViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as CombatResultViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void Awake()
    {
        base.Awake();

        // 반투명 효과를 위한 CanvasGroup 설정
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0.9f; // 약간 반투명
        }

        // 버튼 이벤트 바인딩
        if (_actionButton != null)
        {
            _actionButton.onClick.AddListener(OnActionButtonClicked);
        }
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null)
            return;

        // 제목 텍스트 업데이트
        if (_titleText != null)
        {
            _titleText.text = _viewModel.TitleText;
        }

        // 버튼 텍스트 업데이트
        if (_buttonText != null)
        {
            _buttonText.text = _viewModel.ButtonText;
        }

        // 배경색 설정 (승리: 파란색, 패배: 분홍색)
        if (_backgroundPanel != null)
        {
            Color bgColor = _viewModel.Result == eCombatResult.Victory
                ? new Color(0.2f, 0.4f, 0.8f, 0.9f) // 파란색
                : new Color(0.9f, 0.7f, 0.8f, 0.9f); // 분홍색
            _backgroundPanel.color = bgColor;
        }
    }

    private void OnActionButtonClicked()
    {
        // 버튼 클릭 시 팝업 닫기 (실제 게임에서는 여기서 다음 라운드나 재시작 로직 호출)
        Debug.Log($"[CombatResult] {_viewModel.ButtonText} 버튼 클릭됨");
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        if (_actionButton != null)
        {
            _actionButton.onClick.RemoveListener(OnActionButtonClicked);
        }
        base.OnDestroy();
    }
}


using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 생성 버튼 클릭 시 표시되는 확인 팝업입니다.
/// "게임을 시작할까요?" 메시지와 확인/취소 버튼을 제공합니다.
/// </summary>
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
        // 호스트 시작 요청
        _viewModel?.ConfirmGameStart();

        // 팝업 닫기
        Managers.UI.Close(this);
    }

    private void OnClickCancel()
    {
        Managers.UI.Close(this);
    }
}


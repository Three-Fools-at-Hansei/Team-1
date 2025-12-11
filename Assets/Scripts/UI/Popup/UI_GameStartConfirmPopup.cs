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

    public override void SetViewModel(IViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        _viewModel = viewModel as GameStartConfirmPopupViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested += OnCloseRequested;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (_confirmButton != null) _confirmButton.onClick.AddListener(OnClickConfirm);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(OnClickCancel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null || _messageText == null)
            return;

        _messageText.text = _viewModel.MessageText;
    }

    private void OnClickConfirm()
    {
        Managers.Sound.PlaySFX("Select");
        _viewModel?.ConfirmGameStart();
    }

    private void OnClickCancel()
    {
        Managers.Sound.PlaySFX("Select");
        _viewModel?.Cancel();
    }

    /// <summary>
    /// ViewModel이 닫기를 요청했을 때 실행
    /// </summary>
    private void OnCloseRequested()
    {
        Managers.UI.Close(this);
    }
}


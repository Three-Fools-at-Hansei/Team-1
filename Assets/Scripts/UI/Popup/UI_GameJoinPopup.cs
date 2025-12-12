using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 참가 버튼 클릭 시 표시되는 팝업입니다.
/// 게임 코드 입력 필드와 입장하기 버튼을 제공합니다.
/// </summary>
public class UI_GameJoinPopup : UI_Popup
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private TMP_InputField _codeInputField;
    [SerializeField] private Button _enterButton;
    [SerializeField] private Button _closeButton;

    private GameJoinPopupViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_enterButton != null)
            _enterButton.onClick.AddListener(OnClickEnter);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnCloseRequested);
    }

    private void OnEnable()
    {
        // 팝업이 열릴 때마다 입력 필드 초기화
        if (_codeInputField != null)
        {
            _codeInputField.text = string.Empty;
            _codeInputField.interactable = true;
        }

        if (_enterButton != null)
            _enterButton.interactable = true;
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        // 1. 기존 ViewModel 이벤트 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        // 2. ViewModel 캐스팅 및 base 호출
        _viewModel = viewModel as GameJoinPopupViewModel;
        base.SetViewModel(viewModel);

        // 3. 새 ViewModel 이벤트 구독
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested += OnCloseRequested;
        }
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        // 메시지 갱신
        if (_messageText != null)
            _messageText.text = _viewModel.MessageText;

        // 로딩 중일 때 UI 잠금 처리
        bool isInteractable = _viewModel.IsInteractable;

        if (_enterButton != null)
            _enterButton.interactable = isInteractable;

        if (_codeInputField != null)
            _codeInputField.interactable = isInteractable;
    }

    /// <summary>
    /// 입장하기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnClickEnter()
    {
        if (_viewModel == null) return;

        Managers.Sound.PlaySFX("Select");

        // [Fix] 입력값 공백 제거 및 대문자 변환 처리
        // 복사/붙여넣기 시 발생하는 공백 오류를 방지합니다.
        string rawCode = _codeInputField != null ? _codeInputField.text : string.Empty;
        string gameCode = rawCode.Trim().ToUpper();

        if (string.IsNullOrEmpty(gameCode))
        {
            // ViewModel이 처리하도록 빈 값이라도 전달하거나, 여기서 차단
            return;
        }

        // ViewModel에 정제된 게임 코드 전달 및 접속 시도
        _viewModel.SetGameCode(gameCode);
        _viewModel.TryJoinGame();
    }

    /// <summary>
    /// ViewModel에서 닫기 요청이 오거나, 닫기 버튼을 눌렀을 때 호출
    /// </summary>
    private void OnCloseRequested()
    {
        Managers.Sound.PlaySFX("Select");
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        base.OnDestroy();
    }
}
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

    private GameJoinPopupViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_enterButton != null)
            _enterButton.onClick.AddListener(OnClickEnter);
    }

    private void OnEnable()
    {
        // Object Pool에서 재사용될 때 RectTransform 속성이 손실될 수 있으므로
        // 모든 주요 UI 요소들의 RectTransform을 프리팹 값으로 복원합니다.
        RestoreAllRectTransforms();
        
        // InputField 초기화 (Object Pool 재사용 시 깨끗한 상태 보장)
        if (_codeInputField != null)
        {
            _codeInputField.text = string.Empty;
        }
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as GameJoinPopupViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null || _messageText == null)
            return;

        _messageText.text = _viewModel.MessageText;
    }

    /// <summary>
    /// 입장하기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnClickEnter()
    {
        if (_viewModel == null)
            return;

        // InputField에서 게임 코드 읽기
        string gameCode = _codeInputField != null ? _codeInputField.text : string.Empty;

        // 빈 코드 입력 시 경고
        if (string.IsNullOrWhiteSpace(gameCode))
        {
            Debug.LogWarning("[UI_GameJoinPopup] 게임 코드를 입력해주세요.");
            // TODO: 경고 메시지 표시 (선택사항)
            return;
        }

        // ViewModel에 게임 코드 설정
        _viewModel.SetGameCode(gameCode);

        // TODO: 멀티 연동 로직 구현 예정
        // 예: await Managers.Network.JoinGameAsync(gameCode);
        // 또는: Managers.Network.JoinGame(gameCode);
        Debug.Log($"[UI_GameJoinPopup] 게임 참가 요청: {gameCode} (구현 예정)");

        // 멀티 연동 성공 시 팝업 닫기
        // Managers.UI.Close(this);
    }

    /// <summary>
    /// 모든 주요 UI 요소들의 RectTransform을 프리팹 값으로 복원합니다.
    /// Object Pool에서 재사용될 때 위치가 변경될 수 있으므로 필요합니다.
    /// </summary>
    private void RestoreAllRectTransforms()
    {
        // PopupPanel 복원
        Transform popupPanel = transform.Find("PopupPanel");
        if (popupPanel != null)
        {
            RectTransform popupRect = popupPanel.GetComponent<RectTransform>();
            if (popupRect != null)
            {
                popupRect.anchorMin = new Vector2(0.5f, 0.5f);
                popupRect.anchorMax = new Vector2(0.5f, 0.5f);
                popupRect.anchoredPosition = Vector2.zero;
                popupRect.sizeDelta = new Vector2(480, 320);
                popupRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        // MessageText 복원
        if (_messageText != null)
        {
            RectTransform messageRect = _messageText.GetComponent<RectTransform>();
            if (messageRect != null)
            {
                messageRect.anchorMin = new Vector2(0.5f, 0.7f);
                messageRect.anchorMax = new Vector2(0.5f, 0.7f);
                messageRect.anchoredPosition = Vector2.zero;
                messageRect.sizeDelta = new Vector2(320, 80);
                messageRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }
        else
        {
            Transform messageTransform = transform.Find("MessageText");
            if (messageTransform != null)
            {
                RectTransform messageRect = messageTransform.GetComponent<RectTransform>();
                if (messageRect != null)
                {
                    messageRect.anchorMin = new Vector2(0.5f, 0.7f);
                    messageRect.anchorMax = new Vector2(0.5f, 0.7f);
                    messageRect.anchoredPosition = Vector2.zero;
                    messageRect.sizeDelta = new Vector2(320, 80);
                    messageRect.pivot = new Vector2(0.5f, 0.5f);
                }
            }
        }

        // CodeInputField 복원
        if (_codeInputField != null)
        {
            RectTransform inputRect = _codeInputField.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.anchorMin = new Vector2(0.5f, 0.5f);
                inputRect.anchorMax = new Vector2(0.5f, 0.5f);
                inputRect.anchoredPosition = Vector2.zero;
                inputRect.sizeDelta = new Vector2(360, 60);
                inputRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }
        else
        {
            Transform inputTransform = transform.Find("CodeInputField");
            if (inputTransform != null)
            {
                RectTransform inputRect = inputTransform.GetComponent<RectTransform>();
                if (inputRect != null)
                {
                    inputRect.anchorMin = new Vector2(0.5f, 0.5f);
                    inputRect.anchorMax = new Vector2(0.5f, 0.5f);
                    inputRect.anchoredPosition = Vector2.zero;
                    inputRect.sizeDelta = new Vector2(360, 60);
                    inputRect.pivot = new Vector2(0.5f, 0.5f);
                }
            }
        }

        // EnterButton 복원
        if (_enterButton != null)
        {
            RectTransform enterRect = _enterButton.GetComponent<RectTransform>();
            if (enterRect != null)
            {
                enterRect.anchorMin = new Vector2(0.5f, 0.3f);
                enterRect.anchorMax = new Vector2(0.5f, 0.3f);
                enterRect.anchoredPosition = Vector2.zero;
                enterRect.sizeDelta = new Vector2(200, 60);
                enterRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }
        else
        {
            Transform enterTransform = transform.Find("EnterButton");
            if (enterTransform != null)
            {
                RectTransform enterRect = enterTransform.GetComponent<RectTransform>();
                if (enterRect != null)
                {
                    enterRect.anchorMin = new Vector2(0.5f, 0.3f);
                    enterRect.anchorMax = new Vector2(0.5f, 0.3f);
                    enterRect.anchoredPosition = Vector2.zero;
                    enterRect.sizeDelta = new Vector2(200, 60);
                    enterRect.pivot = new Vector2(0.5f, 0.5f);
                }
            }
        }
    }
}


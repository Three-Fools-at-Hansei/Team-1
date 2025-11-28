using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Play 버튼 클릭 시 표시되는 팝업입니다.
/// 게임 생성 및 게임 참가 버튼을 제공합니다.
/// </summary>
public class UI_PlayPopup : UI_Popup
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _createGameButton;  // 게임 생성 버튼
    [SerializeField] private Button _joinGameButton;    // 게임 참가 버튼

    protected override void Awake()
    {
        base.Awake();

        // 닫기 버튼은 Awake에서 등록 (원래 방식)
        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);
    }

    private void OnEnable()
    {
        // Object Pool에서 재사용될 때 Serialize된 필드가 손실될 수 있으므로
        // 버튼 참조를 다시 찾아서 설정합니다. (게임 생성/참가 버튼만)
        RefreshButtonReferences();
        
        // 버튼 GameObject들을 명시적으로 활성화합니다.
        // Object Pool에서 재사용될 때 자식 GameObject들이 비활성화될 수 있습니다.
        EnsureButtonsActive();
        
        // 게임 생성/참가 버튼 리스너를 등록합니다.
        RegisterGameButtonsListeners();
    }
    
    /// <summary>
    /// 버튼 참조를 다시 찾아서 설정합니다.
    /// Object Pool에서 재사용될 때 Serialize된 필드가 손실될 수 있으므로 필요합니다.
    /// 게임 생성/참가 버튼만 처리합니다.
    /// </summary>
    private void RefreshButtonReferences()
    {
        if (_createGameButton == null)
        {
            Transform createButtonTransform = transform.Find("ButtonContainer/CreateGameButton");
            if (createButtonTransform != null)
            {
                _createGameButton = createButtonTransform.GetComponent<Button>();
            }
        }
        
        if (_joinGameButton == null)
        {
            Transform joinButtonTransform = transform.Find("ButtonContainer/JoinGameButton");
            if (joinButtonTransform != null)
            {
                _joinGameButton = joinButtonTransform.GetComponent<Button>();
            }
        }
        
        // 디버그 로그 (문제 진단용)
        if (_createGameButton == null || _joinGameButton == null)
        {
            Debug.LogWarning($"[UI_PlayPopup] 버튼 참조가 null입니다. CreateGameButton: {_createGameButton != null}, JoinGameButton: {_joinGameButton != null}");
        }
    }
    
    /// <summary>
    /// 게임 생성/참가 버튼 GameObject를 명시적으로 활성화합니다.
    /// Object Pool에서 재사용될 때 자식 GameObject들이 비활성화될 수 있습니다.
    /// </summary>
    private void EnsureButtonsActive()
    {
        // CreateGameButton 활성화
        if (_createGameButton != null && !_createGameButton.gameObject.activeSelf)
        {
            _createGameButton.gameObject.SetActive(true);
        }
        else if (_createGameButton == null)
        {
            Transform createButtonTransform = transform.Find("ButtonContainer/CreateGameButton");
            if (createButtonTransform != null)
            {
                createButtonTransform.gameObject.SetActive(true);
            }
        }
        
        // JoinGameButton 활성화
        if (_joinGameButton != null && !_joinGameButton.gameObject.activeSelf)
        {
            _joinGameButton.gameObject.SetActive(true);
        }
        else if (_joinGameButton == null)
        {
            Transform joinButtonTransform = transform.Find("ButtonContainer/JoinGameButton");
            if (joinButtonTransform != null)
            {
                joinButtonTransform.gameObject.SetActive(true);
            }
        }
        
        // ButtonContainer도 활성화
        Transform buttonContainer = transform.Find("ButtonContainer");
        if (buttonContainer != null && !buttonContainer.gameObject.activeSelf)
        {
            buttonContainer.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        // Object Pool로 반환될 때 게임 생성/참가 버튼 리스너를 제거합니다.
        UnregisterGameButtonsListeners();
    }

    /// <summary>
    /// 게임 생성/참가 버튼 리스너를 등록합니다.
    /// </summary>
    private void RegisterGameButtonsListeners()
    {
        if (_createGameButton != null)
            _createGameButton.onClick.AddListener(OnClickCreateGame);

        if (_joinGameButton != null)
            _joinGameButton.onClick.AddListener(OnClickJoinGame);
    }

    /// <summary>
    /// 게임 생성/참가 버튼 리스너를 제거합니다.
    /// </summary>
    private void UnregisterGameButtonsListeners()
    {
        if (_createGameButton != null)
            _createGameButton.onClick.RemoveListener(OnClickCreateGame);

        if (_joinGameButton != null)
            _joinGameButton.onClick.RemoveListener(OnClickJoinGame);
    }

    private void OnClickClose()
    {
        Managers.UI.Close(this);
    }

    /// <summary>
    /// 게임 생성 버튼 클릭 시 호출됩니다.
    /// 현재는 미구현 상태입니다.
    /// </summary>
    private async void OnClickCreateGame()
    {
        // 확인 팝업을 표시하여 사용자에게 게임 시작 여부를 묻습니다.
        await Managers.UI.ShowAsync<UI_GameStartConfirmPopup>(new GameStartConfirmPopupViewModel());
    }

    /// <summary>
    /// 게임 참가 버튼 클릭 시 호출됩니다.
    /// 현재는 미구현 상태입니다.
    /// </summary>
    private void OnClickJoinGame()
    {
        // TODO: 게임 참가 로직 구현 예정
        // 예: Managers.Network.JoinGame() 또는 게임 목록 표시
        Debug.Log("[UI_PlayPopup] 게임 참가 요청 (구현 예정)");
    }

    protected override void OnStateChanged()
    {
        // ViewModel에서 데이터를 받아 UI를 업데이트하는 로직이 필요할 경우 여기에 구현
        // 예: 버튼 활성화/비활성화, 버튼 텍스트 변경 등
    }
}




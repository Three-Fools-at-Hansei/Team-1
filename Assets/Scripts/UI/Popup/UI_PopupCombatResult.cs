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
    [SerializeField] private GameObject _survivedObject;
    [SerializeField] private GameObject _deadObject;

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

        // 오브젝트 참조가 없으면 자동으로 찾기
        if (_survivedObject == null)
        {
            _survivedObject = FindChildByName("Survived");
            if (_survivedObject != null)
            {
                Debug.Log("[UI_PopupCombatResult] Survived 오브젝트를 자동으로 찾았습니다.");
            }
            else
            {
                Debug.LogWarning("[UI_PopupCombatResult] Survived 오브젝트를 찾을 수 없습니다.");
            }
        }
        
        if (_deadObject == null)
        {
            _deadObject = FindChildByName("Dead");
            if (_deadObject != null)
            {
                Debug.Log("[UI_PopupCombatResult] Dead 오브젝트를 자동으로 찾았습니다.");
            }
            else
            {
                Debug.LogWarning("[UI_PopupCombatResult] Dead 오브젝트를 찾을 수 없습니다.");
            }
        }


        // 초기 상태: 오브젝트 모두 비활성화
        if (_survivedObject != null)
        {
            _survivedObject.SetActive(false);
        }
        if (_deadObject != null)
        {
            _deadObject.SetActive(false);
        }

        // 버튼 이벤트 바인딩
        if (_actionButton != null)
        {
            _actionButton.onClick.AddListener(OnActionButtonClicked);
        }
    }

    /// <summary>
    /// 자식 오브젝트 중에서 이름으로 찾기 (재귀적으로 검색)
    /// </summary>
    private GameObject FindChildByName(string name)
    {
        return FindChildByNameRecursive(transform, name);
    }

    private GameObject FindChildByNameRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
            
            GameObject found = FindChildByNameRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }


    protected override void OnStateChanged()
    {
        if (_viewModel == null)
        {
            Debug.LogWarning("[UI_PopupCombatResult] ViewModel이 null입니다.");
            return;
        }

        Debug.Log($"[UI_PopupCombatResult] OnStateChanged 호출됨. Result: {_viewModel.Result}");

        // 제목 텍스트 숨기기 (오브젝트로 대체)
        if (_titleText != null)
        {
            RectTransform titleRect = _titleText.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                // TitleText의 부모와 월드 위치 가져오기
                Transform titleParent = titleRect.parent;
                Vector3 titleWorldPosition = titleRect.position;
                
                // Survived 오브젝트를 Title 위치로 이동
                if (_survivedObject != null)
                {
                    RectTransform survivedRect = _survivedObject.GetComponent<RectTransform>();
                    if (survivedRect != null)
                    {
                        // TitleText의 anchoredPosition 복사
                        survivedRect.anchoredPosition = titleRect.anchoredPosition;
                    }
                }
                
                // Dead 오브젝트를 Title 위치로 이동
                if (_deadObject != null)
                {
                    RectTransform deadRect = _deadObject.GetComponent<RectTransform>();
                    if (deadRect != null)
                    {
                        // TitleText의 anchoredPosition 복사
                        deadRect.anchoredPosition = titleRect.anchoredPosition;
                    }
                }
            }
            
            // TitleText 숨기기
            _titleText.gameObject.SetActive(false);
        }

        // 승리/패배 오브젝트 표시
        bool isVictory = _viewModel.Result == eCombatResult.Victory;
        Debug.Log($"[UI_PopupCombatResult] isVictory: {isVictory}, Survived: {_survivedObject != null}, Dead: {_deadObject != null}");
        
        if (_survivedObject != null)
        {
            _survivedObject.SetActive(isVictory);
            Debug.Log($"[UI_PopupCombatResult] Survived 오브젝트 활성화: {isVictory}");
        }
        else
        {
            Debug.LogWarning("[UI_PopupCombatResult] Survived 오브젝트가 null입니다.");
        }
        
        if (_deadObject != null)
        {
            _deadObject.SetActive(!isVictory);
            Debug.Log($"[UI_PopupCombatResult] Dead 오브젝트 활성화: {!isVictory}");
        }
        else
        {
            Debug.LogWarning("[UI_PopupCombatResult] Dead 오브젝트가 null입니다.");
        }

        // 버튼 텍스트 업데이트
        if (_buttonText != null)
        {
            _buttonText.text = _viewModel.ButtonText;
        }

        // 배경색 설정 (승리: 파란색, 패배: 분홍색)
        if (_backgroundPanel != null)
        {
            Color bgColor = isVictory
                ? new Color(0.2f, 0.4f, 0.8f, 0.9f) // 파란색
                : new Color(0.9f, 0.7f, 0.8f, 0.9f); // 분홍색
            _backgroundPanel.color = bgColor;
        }
    }

    private void OnActionButtonClicked()
    {
        Debug.Log($"[CombatResult] {_viewModel.ButtonText} 버튼 클릭됨 -> 로비로 이동");
        _viewModel?.GoToLobby();
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


using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 추가

public class UI_PopupCombatResult : UI_Popup
{
    public override string ActionMapKey => "None";

    [Header("UI References")]
    [SerializeField] private Image _backgroundPanel;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private GameObject _survivedObject;
    [SerializeField] private GameObject _deadObject;

    // [추가] 승리/패배 이미지 오브젝트
    [Header("Result Images")]
    [SerializeField] private GameObject _victoryImageObj;
    [SerializeField] private GameObject _defeatImageObj;

    private CombatResultViewModel _viewModel;

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as CombatResultViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void Awake()
    {
        base.Awake();

        // [연출] 초기화 (등장은 조금 강조되게, 퇴장은 빠르게)
        _fadeIn = new FadeInUIAnimation(0.5f, Ease.OutBack);
        _fadeOut = new FadeOutUIAnimation(0.2f, Ease.InQuad);

        // 반투명 효과를 위한 CanvasGroup 설정 및 초기 Alpha 0
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
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

        // [추가] 이미지 오브젝트 초기화
        if (_victoryImageObj != null) _victoryImageObj.SetActive(false);
        if (_defeatImageObj != null) _defeatImageObj.SetActive(false);

        // 버튼 이벤트 바인딩
        if (_actionButton != null)
        {
            _actionButton.onClick.AddListener(OnActionButtonClicked);
        }
    }

    // [연출] 팝업 활성화 시 FadeIn 실행
    private void OnEnable()
    {
        _fadeIn?.ExecuteAsync(_canvasGroup);
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

        // 승리/패배 여부 확인
        bool isVictory = _viewModel.Result == eCombatResult.Victory;
        Debug.Log($"[UI_PopupCombatResult] isVictory: {isVictory}, Survived: {_survivedObject != null}, Dead: {_deadObject != null}");

        // 캐릭터 연출 오브젝트 갱신
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

        // [추가] 승리/패배 이미지 갱신
        if (_victoryImageObj != null) _victoryImageObj.SetActive(isVictory);
        if (_defeatImageObj != null) _defeatImageObj.SetActive(!isVictory);

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

    // [연출] 버튼 클릭 시 FadeOut 실행 후 ViewModel 로직 수행
    private async void OnActionButtonClicked()
    {
        Debug.Log($"[CombatResult] {_viewModel.ButtonText} 버튼 클릭됨 -> 로비로 이동");
        Managers.Sound.PlaySFX("Select");

        if (_fadeOut != null)
        {
            await _fadeOut.ExecuteAsync(_canvasGroup);
        }

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
using System.Collections.Generic;
using UI;
using UnityEngine;
using DG.Tweening; // DOTween 추가

public class UI_RewardPopup : UI_Popup
{
    [SerializeField] private Transform _contentRoot; // 아이템이 생성될 부모
    [SerializeField] private GameObject _itemPrefab; // UI_RewardItem 프리팹

    private RewardPopupViewModel _viewModel;

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as RewardPopupViewModel;
        base.SetViewModel(viewModel);

        GenerateItems();
    }

    protected override void Awake()
    {
        base.Awake();

        // [연출] 초기화
        _fadeIn = new FadeInUIAnimation(0.3f, Ease.OutQuad);
        _fadeOut = new FadeOutUIAnimation(0.2f, Ease.InQuad);

        // 초기 Alpha 0
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
    }

    // [연출] 등장 애니메이션
    private void OnEnable()
    {
        _fadeIn?.ExecuteAsync(_canvasGroup);
    }

    private void GenerateItems()
    {
        // 기존 아이템 제거
        foreach (Transform child in _contentRoot)
            Destroy(child.gameObject);

        if (_viewModel == null || _itemPrefab == null) return;

        foreach (var itemVM in _viewModel.RewardItems)
        {
            GameObject go = Instantiate(_itemPrefab, _contentRoot);
            UI_RewardItem view = go.GetComponent<UI_RewardItem>();
            if (view != null)
            {
                view.Init(itemVM, this);
            }
        }
    }

    protected override void OnStateChanged()
    {
        // 특별한 상태 변경 처리 없음 (생성 시 1회성)
    }
}
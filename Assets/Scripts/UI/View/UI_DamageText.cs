using TMPro;
using UI;
using UnityEngine;
using DG.Tweening;

public class UI_DamageText : UI_View
{
    [SerializeField] private TMP_Text _damageText;

    [Header("Animation Settings")]
    [SerializeField] private float _floatDistance = 100f; // 스크린 좌표계이므로 픽셀 단위
    [SerializeField] private float _duration = 1.0f;
    [SerializeField] private Ease _easeType = Ease.OutQuad;

    private DamageTextViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as DamageTextViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        // [수정] 여기서는 텍스트와 색상만 설정하고, 애니메이션은 시작하지 않음
        if (_viewModel == null || _damageText == null) return;

        _damageText.text = _viewModel.DamageText;
        _damageText.color = _viewModel.TextColor;

        // 투명도 등 초기화만 수행
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        else _damageText.alpha = 1f;

        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// [New] 외부에서 위치를 잡은 뒤 호출하는 애니메이션 시작 함수
    /// </summary>
    public void Play(Vector3 startScreenPos)
    {
        // 1. 위치 확정
        transform.position = startScreenPos;

        // 2. 애니메이션 시작 (Relative로 현재 위치 기준 위로 이동)
        // DOKill을 호출하여 혹시 모를 이전 트윈 중단
        transform.DOKill();
        transform.DOMoveY(_floatDistance, _duration)
            .SetRelative(true) // [핵심] 현재 위치 기준으로 이동
            .SetEase(_easeType);

        // 3. 페이드 아웃
        if (_canvasGroup != null)
        {
            _canvasGroup.DOKill();
            _canvasGroup.alpha = 1f;
            _canvasGroup.DOFade(0f, _duration * 0.5f)
                .SetDelay(_duration * 0.5f)
                .OnComplete(OnAnimationComplete);
        }
        else
        {
            _damageText.DOKill();
            _damageText.alpha = 1f;
            _damageText.DOFade(0f, _duration * 0.5f)
                .SetDelay(_duration * 0.5f)
                .OnComplete(OnAnimationComplete);
        }
    }

    private void OnAnimationComplete()
    {
        Managers.UI.Close(this);
    }

    private void OnDisable()
    {
        transform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();
        if (_damageText != null) _damageText.DOKill();
    }
}
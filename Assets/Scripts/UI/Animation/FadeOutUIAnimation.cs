using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class FadeOutUIAnimation : IUIAnimation
{
    private readonly float _duration;
    private readonly Ease _ease;

    /// <summary>
    /// FadeOut 연출 객체를 생성합니다.
    /// </summary>
    /// <param name="duration">연출 시간(초)</param>
    /// <param name="ease">DOTween Ease</param>
    public FadeOutUIAnimation(float duration = 0.2f, Ease ease = Ease.OutQuad)
    {
        _duration = duration;
        _ease = ease;
    }

    public async Task ExecuteAsync(CanvasGroup cg)
    {
        if (cg == null) 
            return;

        cg.alpha = 1f;
        cg.interactable = false;

        await cg.DOFade(0f, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .AsyncWaitForCompletion();
    }
}
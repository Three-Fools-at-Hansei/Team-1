using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class FadeInUIAnimation : IUIAnimation
{
    private readonly float _duration;
    private readonly Ease _ease;

    /// <summary>
    /// FadeIn 연출 객체를 생성합니다.
    /// </summary>
    /// <param name="duration">연출 시간(초)</param>
    /// <param name="ease">DOTween Ease</param>
    public FadeInUIAnimation(float duration = 0.3f, Ease ease = Ease.OutQuad)
    {
        _duration = duration;
        _ease = ease;
    }

    public async Task ExecuteAsync(CanvasGroup cg)
    {
        if (cg == null) 
            return;

        cg.alpha = 0f;
        cg.interactable = false;

        await cg.DOFade(1f, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .AsyncWaitForCompletion();

        cg.interactable = true;
    }
}
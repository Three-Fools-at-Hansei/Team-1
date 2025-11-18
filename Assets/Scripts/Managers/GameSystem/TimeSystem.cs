using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TimeSystem : IDisposable
{
    /// <summary>
    /// 일일 초기화까지 남은 시간
    /// </summary>
    public ReactiveProperty<TimeSpan> RemainingTime { get; private set; }

    private CancellationTokenSource _timerCts;

    public void Init()
    {
        RemainingTime = new ReactiveProperty<TimeSpan>(TimeSpan.Zero);
        _timerCts = new CancellationTokenSource();
        RunTimerAsync(_timerCts.Token);
    }

    private async void RunTimerAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                RemainingTime.Value = DateTime.Today.AddDays(1) - DateTime.Now;
                
                // 1초 대기
                await Task.Delay(1000, ct); 
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("[TimeSystem] 타이머가 정상적으로 중지되었습니다.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[TimeSystem] 타이머 실행 중 오류 발생: {ex.Message}");
        }
    }

    public void Dispose()
    {
        // RunTimerAsync 중단
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }
}
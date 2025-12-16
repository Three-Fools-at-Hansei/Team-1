using System;
using UI;

public class LoadingViewModel : ViewModelBase, IProgress<float>
{
    public override event Action OnStateChanged;

    // View에게 닫기 연출을 시작하라고 요청하는 이벤트
    public event Action OnCloseRequested;

    public float Progress { get; private set; }
    public string LoadingText { get; private set; } = "Loading...";

    /// <summary>
    /// SceneManagerEx 등 외부에서 진행률을 보고할 때 호출
    /// </summary>
    public void Report(float value)
    {
        Progress = value;
        LoadingText = $"Loading... {(int)(value * 100)}%";
        OnStateChanged?.Invoke();

        // [핵심] 진행률이 100%에 도달하면 View에게 종료 요청
        if (Progress >= 1.0f)
        {
            OnCloseRequested?.Invoke();
        }
    }
}
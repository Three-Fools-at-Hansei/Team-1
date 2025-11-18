using UnityEngine;

public class GameSystemManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.GameSystem;
    public MissionSystem MissionSystem { get; private set; }
    public TimeSystem TimeSystem { get; private set; }

    public void Init()
    {
        MissionSystem = new MissionSystem();
        MissionSystem.Init();

        TimeSystem = new TimeSystem();
        TimeSystem.Init();

        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    /// <summary>
    /// 씬 데이터 로드가 완료되었을 때 SceneManagerEx에 의해 호출됩니다.
    /// </summary>
    public void OnDataLoaded()
    {
        MissionSystem?.OnDataLoaded();
        // ...
    }

    public void Update() { }

    public void Clear()
    {
        MissionSystem?.Dispose();
        TimeSystem?.Dispose();

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }


}

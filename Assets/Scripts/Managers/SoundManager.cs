using UnityEngine;

public class SoundManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Sound;
    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init �մϴ�.");
    }

    public void Update()
    {
    }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear �մϴ�.");
    }
}

using UnityEngine;

public class SoundManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Sound;
    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update()
    {
    }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }
}

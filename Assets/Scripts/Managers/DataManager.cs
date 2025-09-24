using UnityEngine;

public class DataManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Data;
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

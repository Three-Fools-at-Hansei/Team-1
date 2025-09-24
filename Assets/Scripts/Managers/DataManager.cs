using UnityEngine;

public class DataManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Data;
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

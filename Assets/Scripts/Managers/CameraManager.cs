using UnityEngine;

public class CameraManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Camera;
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

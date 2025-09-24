using UnityEngine;

public class InputManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Input;
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

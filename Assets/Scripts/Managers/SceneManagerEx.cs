using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Scene;
    public IScene CurrentScene { get; private set; }

    /// <summary>
    /// �� �� �ε� �� ���� �ڽ��� ����� �� ����մϴ�.
    /// </summary>
    /// <param name="scene">���� �ε�� �� �ڽ�</param>
    public void SetCurrentScene(IScene scene) => CurrentScene = scene;

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init �մϴ�.");
    }

    public void Update() { }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear �մϴ�.");

        // ���� �� Clear
        CurrentScene?.Clear();
        CurrentScene = null;
    }

    //////////////

    /// <summary>
    /// ���� �ε��ϰ� Managers.Inst.Clear() �մϴ�.
    /// </summary>
    /// <param name="sceneType">�� Ÿ��</param>
    public void LoadScene(eSceneType sceneType) => LoadScene(sceneType.ToString());

    /// <summary>
    /// ���� �ε��ϰ� Managers.Inst.Clear() �մϴ�.
    /// </summary>
    /// <param name="sceneName">�� �̸�</param>
    public void LoadScene(string sceneName)
    {
        Managers.Inst.Clear();
        SceneManager.LoadScene(sceneName);

        if (CurrentScene == null)
            Debug.LogError($"�� �ε� ����: {sceneName}�� null�Դϴ�.");
        else
            CurrentScene.Init();
    }

    /// <summary>
    /// �񵿱�� ���� �ε��մϴ�. 
    /// </summary>
    /// <param name="sceneType">�� Ÿ��</param>
    /// <param name="onCompleted">�ε� �Ϸ� �� ������ �ݹ�</param>
    /// <returns>AsyncOperation ��ü�� ��ȯ�մϴ�.</returns>
    public AsyncOperation LoadSceneAsync(eSceneType sceneType, Action<AsyncOperation> onCompleted = null) => LoadSceneAsync(sceneType.ToString(), onCompleted);

    /// <summary>
    /// �񵿱�� ���� �ε��մϴ�. 
    /// </summary>
    /// <param name="sceneName">�� �̸�</param>
    /// <param name="onCompleted">�ε� �Ϸ� �� ������ �ݹ�</param>
    /// <returns>AsyncOperation ��ü�� ��ȯ�մϴ�.</returns>
    public AsyncOperation LoadSceneAsync(string sceneName, Action<AsyncOperation> onCompleted = null)
    {
        Managers.Inst.Clear();

        // �񵿱� �� �ε�
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // �� �ε��� �Ϸ�Ǹ� ����� �� Init()�� �ݹ� ���
        operation.completed += (AsyncOperation op) => 
        {
            if (CurrentScene == null)
                Debug.LogError($"�� �ε� ����: {sceneName}�� null�Դϴ�.");
            else
                CurrentScene.Init();

            onCompleted?.Invoke(op); 
        };

        return operation;
    }

}

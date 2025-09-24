using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Scene;
    public IScene CurrentScene { get; private set; }

    /// <summary>
    /// 새 씬 로드 시 씬이 자신을 등록할 때 사용합니다.
    /// </summary>
    /// <param name="scene">새로 로드된 씬 자신</param>
    public void SetCurrentScene(IScene scene) => CurrentScene = scene;

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear 합니다.");

        // 현재 씬 Clear
        CurrentScene?.Clear();
        CurrentScene = null;
    }

    //////////////

    /// <summary>
    /// 씬을 로드하고 Managers.Inst.Clear() 합니다.
    /// </summary>
    /// <param name="sceneType">씬 타입</param>
    public void LoadScene(eSceneType sceneType) => LoadScene(sceneType.ToString());

    /// <summary>
    /// 씬을 로드하고 Managers.Inst.Clear() 합니다.
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    public void LoadScene(string sceneName)
    {
        Managers.Inst.Clear();
        SceneManager.LoadScene(sceneName);

        if (CurrentScene == null)
            Debug.LogError($"씬 로드 실패: {sceneName}이 null입니다.");
        else
            CurrentScene.Init();
    }

    /// <summary>
    /// 비동기로 씬을 로드합니다. 
    /// </summary>
    /// <param name="sceneType">씬 타입</param>
    /// <param name="onCompleted">로드 완료 시 실행할 콜백</param>
    /// <returns>AsyncOperation 객체를 반환합니다.</returns>
    public AsyncOperation LoadSceneAsync(eSceneType sceneType, Action<AsyncOperation> onCompleted = null) => LoadSceneAsync(sceneType.ToString(), onCompleted);

    /// <summary>
    /// 비동기로 씬을 로드합니다. 
    /// </summary>
    /// <param name="sceneName">씬 이름</param>
    /// <param name="onCompleted">로드 완료 시 실행할 콜백</param>
    /// <returns>AsyncOperation 객체를 반환합니다.</returns>
    public AsyncOperation LoadSceneAsync(string sceneName, Action<AsyncOperation> onCompleted = null)
    {
        Managers.Inst.Clear();

        // 비동기 씬 로드
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 씬 로딩이 완료되면 실행될 씬 Init()과 콜백 등록
        operation.completed += (AsyncOperation op) => 
        {
            if (CurrentScene == null)
                Debug.LogError($"씬 로드 실패: {sceneName}이 null입니다.");
            else
                CurrentScene.Init();

            onCompleted?.Invoke(op); 
        };

        return operation;
    }

}

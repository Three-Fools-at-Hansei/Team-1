using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Scene;
    public IScene CurrentScene { get; private set; }

    /// <summary>
    /// 현재 씬의 비동기 초기화 작업(데이터 로드 등)을 나타내는 Task입니다.
    /// 다른 스크립트의 Start()에서 이 Task를 await 하여 데이터가 준비될 때까지 기다려야 합니다.
    /// </summary>
    public Task CurrentSceneInitTask { get; private set; }

    /// <summary>
    /// 새 씬 로드 시 씬이 자신을 등록할 때 사용합니다.
    /// </summary>
    /// <param name="scene">새로 로드된 씬 자신</param>
    public void SetCurrentScene(IScene scene)
    {
        CurrentScene = scene;

        Managers.Input.SwitchActionMap(Managers.Input.DefaultActionMapKey);

        // 씬 초기화 동작
        CurrentSceneInitTask = InitSceneAsync(scene);
    }

    /// <summary>
    /// 씬에 필요한 데이터를 로드하고 초기화 합니다.
    /// Managers 에 놓는 것이 기능 분리 할 수 있는 것 아닐까?
    /// </summary>
    private async Task InitSceneAsync(IScene scene)
    {
        if (scene == null) return;

        try
        {
            Debug.Log("[SceneManager] 유저 데이터 로드 시작");
            await Managers.Data.LoadUserData();

            Debug.Log("[SceneManager] 씬 데이터 로드 시작");
            var requiredFiles = scene.RequiredDataFiles;
            if (requiredFiles != null && requiredFiles.Count > 0)
                await Managers.Data.LoadDataForSceneAsync(requiredFiles);

            Debug.Log("[SceneManager] 데이터 로드 완료. Init 호출");
            Managers.GameSystem.OnDataLoaded();

            scene.Init();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneManager] 초기화 중 오류 발생: {e}");
        }
    }

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
        CurrentSceneInitTask = null;
    }

    //////////////

    /// <summary>
    /// 비동기로 씬(Scene)을 로드합니다.
    /// IProgress<float>를 통해 진행률을 보고합니다.
    /// </summary>
    /// <param name="sceneType">로드할 씬 타입</param>
    /// <param name="progress">진행률을 보고받을 IProgress<float> 구현체입니다.</param>
    public async Task LoadSceneAsync(eSceneType sceneType, IProgress<float> progress = null)
    {
        // 1. 진행도 초기화
        progress?.Report(0f);

        // 2. 이전 씬의 모든 리소스 정리
        Managers.Inst.Clear();

        string sceneName = sceneType.ToString();

        // 3. 씬 비동기 로드 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // 4. operation.progress가 0.9f에 도달할 때까지 진행률을 갱신합니다.
        while (operation.progress < 0.9f)
        {
            progress?.Report(operation.progress); // 0.0 ~ 0.9 사이의 값 보고
            await Task.Yield(); // 1프레임 대기
        }
        progress?.Report(0.9f); // 90%로 고정

        // 5. 씬 활성화 및 완료 대기
        // 현재 씬.awake() -> SetCurrentScene() -> CurrentSceneInitTask() 호출
        operation.allowSceneActivation = true;
        await operation;

        if (CurrentScene == null)
        {
            Debug.LogError($"[SceneManagerEx] 씬(Scene) 로드 실패: {sceneName}의 IScene이 null입니다.");
            progress?.Report(1f);
            return;
        }

        // 6. 데이터 로드 및 씬 초기화 대기
        if (CurrentSceneInitTask != null)
            await CurrentSceneInitTask;

        // 7. 로딩 완료
        progress?.Report(1.0f);
    }
}

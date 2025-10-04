using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Test;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() 합니다.");
    }

    void Start()
    {
        ((IScene)this).Init();
    }

    void IScene.Init()
    {
        Debug.Log("Test Scene Init() 합니다.");
        // ViewModel을 먼저 생성하고 UI 생성을 요청합니다.
        var viewModel = new PopupTestViewModel();
        _ = Managers.UI.ShowAsync<UI_PopupTest>(viewModel);
    }

    void IScene.Clear()
    {
        Debug.Log("Test Scene Clear() 합니다.");
    }
}
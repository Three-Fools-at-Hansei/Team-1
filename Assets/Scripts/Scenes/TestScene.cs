using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Test;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() �մϴ�.");
    }

    void Start()
    {
        ((IScene)this).Init();
    }

    void IScene.Init()
    {
        Debug.Log("Test Scene Init() �մϴ�.");
        // ViewModel�� ���� �����ϰ� UI ������ ��û�մϴ�.
        var viewModel = new PopupTestViewModel();
        _ = Managers.UI.ShowAsync<UI_PopupTest>(viewModel);
    }

    void IScene.Clear()
    {
        Debug.Log("Test Scene Clear() �մϴ�.");
    }
}
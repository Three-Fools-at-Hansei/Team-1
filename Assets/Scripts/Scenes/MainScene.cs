using UnityEngine;

public class MainScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Main;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Main Scene Awake() �մϴ�.");
    }
    void IScene.Init()
    {
        Debug.Log("Main Scene Init() �մϴ�.");
    }

    void IScene.Clear()
    {
        Debug.Log("Main Scene Clear() �մϴ�.");
    }

}

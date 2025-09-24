using UnityEngine;

public class MainScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Main;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Main Scene Awake() 합니다.");
    }
    void IScene.Init()
    {
        Debug.Log("Main Scene Init() 합니다.");
    }

    void IScene.Clear()
    {
        Debug.Log("Main Scene Clear() 합니다.");
    }

}

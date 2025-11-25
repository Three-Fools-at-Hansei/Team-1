using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.MainScene;
    public List<string> RequiredDataFiles => new List<string>()
    {
        "NikkeGameData.json",
        "ItemGameData.json",
        "MissionGameData.json",
    };

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Main Scene Awake() 합니다.");
    }

    async void IScene.Init()
    {
        Debug.Log("Main Scene Init() 합니다.");

        // 메인 로비 UI 표시
        await Managers.UI.ShowAsync<UI_MainLobby>(new MainLobbyViewModel());
    }

    void IScene.Clear()
    {
        Debug.Log("Main Scene Clear() 합니다.");
    }
}
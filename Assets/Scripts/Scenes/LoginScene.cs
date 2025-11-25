using UnityEngine;
using System.Collections.Generic;

public class LoginScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Login;

    public List<string> RequiredDataFiles => new()
    {
        "ItemGameData.json",
        "MissionGameData.json",
        "NikkeGameData.json",
    };

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
    }

    async void IScene.Init()
    {
        // 네트워크 매니저의 서비스 초기화 대기
        await Managers.Network.InitServicesAsync();

        Debug.Log("LoginScene Init");
        await Managers.UI.ShowAsync<UI_LoginView>(new LoginViewModel());
    }

    void IScene.Clear()
    {
        Debug.Log("LoginScene Clear");
    }
}

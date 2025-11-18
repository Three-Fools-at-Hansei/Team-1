using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Test;
    public List<string> RequiredDataFiles => new()
    {
        "NikkeGameData.json",
        "ItemGameData.json",
        "MissionGameData.json",
    };

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() 합니다.");
    }

    void IScene.Init()
    {
        Debug.Log("Test Scene Init() 합니다.");
        Debug.Log($"persistentDataPath: {Application.persistentDataPath}");

        ShowTestUI();
    }

    private async void ShowTestUI()
    {
        Debug.Log("Test Scene Init() - Player와 NPC를 생성합니다.");

        Managers.Input.SwitchActionMap("Lobby");

        // 1. "Player" 주소를 가진 Player 프리팹 생성
        GameObject playerGo = await Managers.Resource.InstantiateAsync("Player");
        if (playerGo != null)
        {
            playerGo.transform.position = Vector3.zero; // 생성 후 위치를 (0,0,0)으로 설정
        }

        // 2. "BeginnerNPC" 주소를 가진 NPC 프리팹 생성
        // 생성과 동시에 위치를 (3,0,0)으로 설정
        await Managers.Resource.InstantiateAsync("BeginnerNPC", new Vector3(3, 0, 0));

        await Managers.UI.ShowAsync<UI_PopupHello>(new HelloPopupViewModel());
    }

    void IScene.Clear()
    {
        //Debug.Log("Test Scene Clear() 합니다.");
    }
}
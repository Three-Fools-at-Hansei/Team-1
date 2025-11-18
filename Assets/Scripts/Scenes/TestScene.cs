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

    UI_TabGroupPopup popup;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() ÇÕ´Ï´Ù.");
    }

    void IScene.Init()
    {
        Debug.Log("Test Scene Init() ÇÕ´Ï´Ù.");
        Debug.Log($"persistentDataPath: {Application.persistentDataPath}");

        ShowTestUI();
    }
    
    private async void ShowTestUI()
    {

        Debug.Log("Test Scene Init() - Player¿Í NPC¸¦ »ý¼ºÇÕ´Ï´Ù.");

        Managers.Input.SwitchActionMap("Lobby");

        // ±ÔÁ¤ #3, #7 ÁØ¼ö: Manager¸¦ ÅëÇØ ¾îµå·¹½ººí ÁÖ¼Ò·Î ÇÁ¸®ÆÕÀ» »ý¼ºÇÕ´Ï´Ù.

        // 1. "Player" ÁÖ¼Ò¸¦ °¡Áø Player ÇÁ¸®ÆÕ »ý¼º
        GameObject playerGo = await Managers.Resource.InstantiateAsync("Player");
        if (playerGo != null)
        {
            playerGo.transform.position = Vector3.zero; // »ý¼º ÈÄ À§Ä¡¸¦ (0,0,0)À¸·Î ¼³Á¤
        }

        // 2. "BeginnerNPC" ÁÖ¼Ò¸¦ °¡Áø NPC ÇÁ¸®ÆÕ »ý¼º
        // »ý¼º°ú µ¿½Ã¿¡ À§Ä¡¸¦ (3,0,0)À¸·Î ¼³Á¤
        await Managers.Resource.InstantiateAsync("BeginnerNPC", new Vector3(3, 0, 0));

        await Managers.UI.ShowAsync<UI_PopupHello>(new HelloPopupViewModel());
    }

    void IScene.Clear()
    {
        //Debug.Log("Test Scene Clear() ÇÕ´Ï´Ù.");

        Managers.UI.Close(popup);
    }
}
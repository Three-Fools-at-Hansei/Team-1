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
        popup = await Managers.UI.ShowAsync<UI_TabGroupPopup>(new TabGroupPopupViewModel());
    }

    void IScene.Clear()
    {
        //Debug.Log("Test Scene Clear() 합니다.");

        Managers.UI.Close(popup);
    }
}
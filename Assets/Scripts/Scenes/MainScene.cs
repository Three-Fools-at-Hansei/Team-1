using System.Collections.Generic;
using System.Threading.Tasks;
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

    void IScene.Init()
    {
        Debug.Log("Main Scene Init() 합니다.");
        
        ShowLobbyView();
    }

    void IScene.Clear()
    {
        Debug.Log("Main Scene Clear() 합니다.");
    }

    private async void ShowLobbyView()
    {
        Debug.Log("[MainScene] ShowLobbyView() 시작");
        
        // 로비 ViewModel 생성
        LobbyViewModel viewModel = new LobbyViewModel();
        Debug.Log("[MainScene] LobbyViewModel 생성 완료");
        
        UI_LobbyView view = await Managers.UI.ShowAsync<UI_LobbyView>(viewModel);
        
        if (view == null)
            Debug.LogError("[MainScene] UI_LobbyView 로드 실패!");
        else
            Debug.Log($"[MainScene] UI_LobbyView 로드 성공: {view.gameObject.name}");
    }
}
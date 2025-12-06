using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CombatScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.CombatScene;

    public List<string> RequiredDataFiles => new()
    {
        "MonsterGameData.json",
        "WaveGameData.json",
        "RewardGameData.json"
    };

    void Awake()
    {
        Managers.Input.DefaultActionMapKey = "Lobby"; // or "Player"
        Managers.Scene.SetCurrentScene(this);
    }

    async void IScene.Init()
    {
        Debug.Log("Combat Scene Init() - 전투 준비");

        // 1. Core 생성 (호스트가 네트워크 오브젝트로 스폰하지 않았다면 로컬 생성)
        // 주의: Core가 NetworkObject라면 호스트에서만 생성해야 함.
        // 여기서는 테스트를 위해 로컬 리소스로 생성한다고 가정하거나, 
        // CombatGameManager가 관리하도록 할 수 있습니다.

        if (NetworkManager.Singleton.IsServer)
        {
            GameObject coreGo = await Managers.Resource.InstantiateAsync("Core");
            if (coreGo != null)
            {
                coreGo.transform.position = Vector3.zero;
                var netObj = coreGo.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned) netObj.Spawn();
            }
        }

        // 2. HUD 표시
        await Managers.UI.ShowAsync<UI_CombatHUD>(new CombatHUDViewModel());
    }

    void IScene.Clear()
    {
        Debug.Log("Combat Scene Clear()");
    }
}
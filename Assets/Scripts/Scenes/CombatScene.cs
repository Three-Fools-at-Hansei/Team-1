using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CombatScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.CombatScene;

    public List<string> RequiredDataFiles => new()
    {
        "NikkeGameData.json",
        "ItemGameData.json",
        "MissionGameData.json",
    };

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
    }

    void IScene.Init()
    {
        Debug.Log("Combat Scene Init() - 전투 시작!");

        // NGO에서는 NetworkManager가 플레이어를 자동 스폰합니다.
        // 따라서 별도로 InstantiateAsync("Player")를 호출할 필요가 없습니다.
        // 다만, 카메라 세팅이나 UI 초기화 등은 여기서 수행해야 합니다.
    }

    void IScene.Clear()
    {
        Debug.Log("Combat Scene Clear()");
    }
}
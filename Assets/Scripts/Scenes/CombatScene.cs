using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CombatScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.CombatScene;

    public List<string> RequiredDataFiles => new()
    {
        "MonsterGameData.json",
    };

    void Awake()
    {
        Managers.Input.DefaultActionMapKey = "Lobby";

        Managers.Scene.SetCurrentScene(this);
    }

    void IScene.Init()
    {
        Debug.Log("Combat Scene Init() - 전투 시작!");

        // NGO에서는 NetworkManager가 플레이어를 자동 스폰합니다.
        // 따라서 별도로 InstantiateAsync("Player")를 호출할 필요가 없습니다.
        // 다만, 카메라 세팅이나 UI 초기화 등은 여기서 수행해야 합니다.
        ShowTestUI();
    }

    private async void ShowTestUI()
    {
        Debug.Log("Test Scene Init() - Player와 NPC를 생성합니다.");


        // 1. Core 프리팹 생성
        GameObject coreGo = await Managers.Resource.InstantiateAsync("Core");
        if (coreGo != null)
        {
            coreGo.transform.position = Vector3.zero; // 생성 후 위치를 (0,0,0)으로 설정
            Debug.Log("[TestScene] Core 생성 완료");
        }
        else
            Debug.LogError("[TestScene] Core 생성 실패 - Addressable 주소 'Core'를 확인하세요");
    }

    void IScene.Clear()
    {
        Debug.Log("Combat Scene Clear()");
    }
}
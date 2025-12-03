using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

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

    private async void ShowTestUI() // async 키워드 확인
    {
        Debug.Log("Test Scene Init() - Player와 NPC를 생성합니다.");
        // 1. NetworkManager 초기화 대기
        while (NetworkManager.Singleton == null)
        {
            await Task.Yield();
        }

        // 2. 테스트 모드: 호스트 시작 (로컬 연결 강제 설정)
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[CombatScene] 테스트 모드: 로컬 호스트를 시작합니다.");

            // [중요] 테스트 시에는 Relay가 아닌 로컬 직접 연결(127.0.0.1)을 사용하도록 강제함
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("127.0.0.1", 7777);
            }

            bool started = NetworkManager.Singleton.StartHost();
            if (!started)
            {
                Debug.LogError("[CombatScene] 호스트 시작 실패!");
                return;
            }
        }

        // 호스트(서버)인 경우에만 생성
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject coreGo = await Managers.Resource.InstantiateAsync("Core");
            if (coreGo != null)
            {
                coreGo.transform.position = Vector3.zero;

                // 네트워크 스폰
                var netObj = coreGo.GetComponent<NetworkObject>();
                if (netObj != null) netObj.Spawn();
            }
        }
    }

    void IScene.Clear()
    {
        Debug.Log("Combat Scene Clear()");
    }
}
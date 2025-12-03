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
        _ = InitializeCombatSceneAsync();
    }

    private async Task InitializeCombatSceneAsync()
    {
        Debug.Log("[CombatScene] 전투 씬 초기화 시작 - Core 생성 준비");

        // 1. NetworkManager 초기화 대기
        int maxWaitFrames = 300; // 최대 5초 대기 (60fps 기준)
        int waitFrames = 0;
        while (NetworkManager.Singleton == null && waitFrames < maxWaitFrames)
        {
            await Task.Yield();
            waitFrames++;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[CombatScene] NetworkManager.Singleton이 초기화되지 않았습니다!");
            return;
        }

        Debug.Log($"[CombatScene] NetworkManager 확인 완료. IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}, IsHost: {NetworkManager.Singleton.IsHost}");

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

            // StartHost() 후 서버 상태가 설정될 때까지 대기
            waitFrames = 0;
            while (!NetworkManager.Singleton.IsServer && waitFrames < maxWaitFrames)
            {
                await Task.Yield();
                waitFrames++;
            }
        }

        // 3. NGO 씬 로딩 완료 및 서버 상태 확실히 대기
        // NGO 씬 로딩 후 NetworkManager 상태가 완전히 설정될 때까지 추가 대기
        waitFrames = 0;
        while (waitFrames < 60) // 약 1초 추가 대기
        {
            await Task.Yield();
            waitFrames++;
        }

        // 4. 호스트(서버)인 경우에만 Core 생성
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[CombatScene] 서버 모드 확인 - Core 생성 시작");
            
            GameObject coreGo = await Managers.Resource.InstantiateAsync("Core");
            if (coreGo != null)
            {
                coreGo.transform.position = Vector3.zero;

                // 네트워크 스폰
                var netObj = coreGo.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                    Debug.Log("[CombatScene] Core 생성 및 네트워크 스폰 완료");
                }
                else
                {
                    Debug.LogWarning("[CombatScene] Core에 NetworkObject 컴포넌트가 없습니다.");
                }
            }
            else
            {
                Debug.LogError("[CombatScene] Core 생성 실패 - Addressable 주소 'Core'를 확인하세요");
            }
        }
        else
        {
            Debug.Log($"[CombatScene] 클라이언트 모드입니다. Core는 서버에서만 생성됩니다. (IsServer: {NetworkManager.Singleton.IsServer})");
        }
    }

    void IScene.Clear()
    {
        Debug.Log("Combat Scene Clear()");
    }
}
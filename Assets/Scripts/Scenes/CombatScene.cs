using System.Collections.Generic;
using System.Threading.Tasks;
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

    // 해제 시 사용하기 위해 등록된 프리팹 원본을 보관
    private List<GameObject> _pooledPrefabs = new List<GameObject>();

    void Awake()
    {
        Managers.Input.DefaultActionMapKey = "Lobby"; // or "Player"
        Managers.Scene.SetCurrentScene(this);
    }

    async void IScene.Init()
    {
        Debug.Log("Combat Scene Init() - 전투 준비");

        // 매니저를 통한 네트워크 풀 등록
        await InitNetworkObjectPools();

        // 1. Core 생성 (호스트가 네트워크 오브젝트로 스폰하지 않았다면 로컬 생성)
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

    private async Task InitNetworkObjectPools()
    {
        _pooledPrefabs.Clear();

        // 1. Bullet 프리팹 로드 및 등록
        GameObject bulletPrefab = await Managers.Resource.LoadAsync<GameObject>("Bullet");
        if (bulletPrefab != null)
        {
            Managers.Pool.RegisterNetworkPrefab(bulletPrefab);
            _pooledPrefabs.Add(bulletPrefab);
        }

        // 2. 몬스터 프리팹 일괄 로드 및 등록
        var monsterTable = Managers.Data.GetTable<MonsterGameData>();
        if (monsterTable != null)
        {
            HashSet<string> uniquePaths = new HashSet<string>();
            foreach (var data in monsterTable.Values)
            {
                if (!string.IsNullOrEmpty(data.prefabPath))
                    uniquePaths.Add(data.prefabPath);
            }

            foreach (string path in uniquePaths)
            {
                GameObject enemyPrefab = await Managers.Resource.LoadAsync<GameObject>(path);
                if (enemyPrefab != null)
                {
                    Managers.Pool.RegisterNetworkPrefab(enemyPrefab);
                    _pooledPrefabs.Add(enemyPrefab);
                }
            }
        }

        Debug.Log($"[CombatScene] 총 {_pooledPrefabs.Count}개의 네트워크 프리팹 풀링 등록 완료.");
    }

    void IScene.Clear()
    {
        Debug.Log("Combat Scene Clear()");

        // 등록했던 네트워크 풀 핸들러 해제
        foreach (var prefab in _pooledPrefabs)
        {
            Managers.Pool.UnregisterNetworkPrefab(prefab);
        }
        _pooledPrefabs.Clear();
    }
}
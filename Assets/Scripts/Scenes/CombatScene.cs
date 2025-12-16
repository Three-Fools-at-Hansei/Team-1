using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

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

        // [수정] 로딩 팝업 닫기 처리
        // 씬 로드 및 데이터 초기화가 끝났으므로 로딩 화면을 제거합니다.
        // (메인 로비나 팝업에서 생성된 UI_LoadingPopup을 찾아서 종료 요청)
        var loadingPopup = FindAnyObjectByType<UI_LoadingPopup>();
        if (loadingPopup != null && loadingPopup.ViewModel is LoadingViewModel loadingVM)
        {
            // 100% 진행률 보고 -> LoadingViewModel에서 OnCloseRequested 이벤트 발생 
            // -> UI_LoadingPopup이 1초 대기 -> FadeOut -> Close 자동 수행
            loadingVM.Report(1.0f);
        }

        // 플레이어 시네머신 카메라 찾아서 활성화
        // (플레이어 프리팹 내부에 있거나 씬에 배치된 카메라)
        var followCam = FindFirstObjectByType<CinemachineCamera>();
        if (followCam != null)
        {
            Managers.Camera.RegisterCamera("PlayerFollowCam", followCam);
            Managers.Camera.Activate("PlayerFollowCam");
            Debug.Log("[CombatScene] PlayerFollowCam 등록 및 활성화 완료");
        }
        else
        {
            Debug.LogWarning("[CombatScene] 씬에 CinemachineCamera가 없습니다.");
        }

        // 네트워크 풀 등록 (총알, 몬스터 등)
        await InitNetworkObjectPools();

        // 1. Core 생성 (호스트가 네트워크 오브젝트로 스폰하지 않았다면 로컬 생성)
        // 호스트인 경우에만 생성하고 Spawn() 호출
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject coreGo = await Managers.Resource.InstantiateAsync("Core");
            if (coreGo != null)
            {
                coreGo.transform.position = Vector3.zero;
                var netObj = coreGo.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                    netObj.Spawn();
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
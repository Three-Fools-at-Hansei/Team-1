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
        var loadingPopup = FindAnyObjectByType<UI_LoadingPopup>();
        if (loadingPopup != null && loadingPopup.ViewModel is LoadingViewModel loadingVM)
        {
            loadingVM.Report(1.0f);
        }

        // 플레이어 시네머신 카메라 찾아서 활성화
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

        // [변경] Core 생성 로직 제거 -> CombatGameManager.StartGame()으로 이동
        // 기존 코어 생성 코드는 삭제되었습니다.

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
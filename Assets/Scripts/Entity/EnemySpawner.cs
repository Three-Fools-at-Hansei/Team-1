using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Task.Delay 사용을 위해 추가
using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    private Transform[] _spawnPoints;
    private List<GameObject> _activeEnemies = new List<GameObject>();

    // [New] 재시도 설정 상수로 정의
    private const int MAX_RETRY_COUNT = 3;
    private const int RETRY_DELAY_MS = 200; // 0.2초 대기

    private void Awake()
    {
        InitializeSpawnPoints();
    }

    private void InitializeSpawnPoints()
    {
        var points = Utils.FindChild<Transform>(gameObject, null, true)
            .GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains("Point_"))
            .OrderBy(t => {
                string numStr = t.name.Replace("Point_", "");
                return int.TryParse(numStr, out int index) ? index : 999;
            })
            .ToArray();

        _spawnPoints = points;
        Debug.Log($"[EnemySpawner] 스폰 포인트 {points.Length}개 로드 완료.");
    }

    /// <summary>
    /// 재시도 로직이 포함된 몬스터 스폰 메서드
    /// </summary>
    public async void SpawnEnemy(int spawnerIdx, int monsterId)
    {
        if (!IsServer) return;

        int tryCount = 0;

        while (tryCount < MAX_RETRY_COUNT)
        {
            tryCount++;

            try
            {
                // 1. 유효성 검사
                if (_spawnPoints == null || _spawnPoints.Length == 0) return;

                int index = Mathf.Clamp(spawnerIdx, 0, _spawnPoints.Length - 1);
                Transform point = _spawnPoints[index];

                MonsterGameData data = Managers.Data.Get<MonsterGameData>(monsterId);
                if (data == null)
                {
                    Debug.LogError($"[EnemySpawner] ID {monsterId} 몬스터 데이터가 없습니다. (중단)");
                    return; // 데이터 오류는 재시도해도 의미가 없으므로 즉시 종료
                }

                // 2. 리소스 로드 및 생성 (비동기)
                // 네트워크 문제 등으로 여기서 예외가 발생할 수 있음
                GameObject go = await Managers.Resource.InstantiateAsync(data.prefabPath, point.position, point.rotation);

                if (go != null)
                {
                    // 3. 네트워크 스폰 및 초기화
                    var netObj = go.GetComponent<NetworkObject>();
                    if (netObj != null && !netObj.IsSpawned) netObj.Spawn();

                    var enemy = go.GetComponent<Enemy>();
                    if (enemy != null) enemy.Init(data);

                    _activeEnemies.Add(go);

                    // 성공 시 루프 종료
                    return;
                }
                else
                {
                    // 로드된 객체가 null인 경우 (리소스 매니저 내부 오류 등) 예외를 던져 재시도 유도
                    throw new System.Exception($"Resource load returned null for {data.prefabPath}");
                }
            }
            catch (System.Exception e)
            {
                // 실패 로그 출력
                Debug.LogWarning($"[EnemySpawner] 스폰 실패 ({tryCount}/{MAX_RETRY_COUNT})... 오류: {e.Message}");

                // 최대 시도 횟수에 도달했는지 확인
                if (tryCount >= MAX_RETRY_COUNT)
                {
                    Debug.LogError($"[EnemySpawner] 몬스터(ID:{monsterId}) 스폰 최종 실패. 웨이브 진행에 오차가 발생할 수 있습니다.\n{e.StackTrace}");

                    // (선택) 웨이브 매니저에게 알림이 필요한 경우 여기서 처리
                    // 예: CombatGameManager.Instance.OnSpawnFailed(monsterId);
                }
                else
                {
                    // 잠시 대기 후 재시도
                    await Task.Delay(RETRY_DELAY_MS);
                }
            }
        }
    }

    public int GetActiveEnemyCount()
    {
        _activeEnemies.RemoveAll(x => x == null || !x.activeInHierarchy);
        return _activeEnemies.Count;
    }

    public void ClearAllEnemies()
    {
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null && enemy.TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
                netObj.Despawn();
        }
        _activeEnemies.Clear();
    }
}
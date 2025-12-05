using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    private Transform[] _spawnPoints;
    private List<GameObject> _activeEnemies = new List<GameObject>();

    private void Awake()
    {
        InitializeSpawnPoints();
    }

    /// <summary>
    /// Point_0 ~ Point_15 이름 순으로 정렬하여 스폰 포인트 초기화
    /// </summary>
    private void InitializeSpawnPoints()
    {
        // 1. 모든 자식 중 이름에 "Point_"가 포함된 Transform 찾기
        var points = Utils.FindChild<Transform>(gameObject, null, true)
            .GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains("Point_"))
            .OrderBy(t => {
                // "Point_12" -> 12 추출하여 정렬
                string numStr = t.name.Replace("Point_", "");
                return int.TryParse(numStr, out int index) ? index : 999;
            })
            .ToArray();

        _spawnPoints = points;
        Debug.Log($"[EnemySpawner] 스폰 포인트 {points.Length}개 로드 완료.");
    }

    /// <summary>
    /// CombatGameManager가 호출: 특정 위치에 특정 몬스터 스폰
    /// </summary>
    public async void SpawnEnemy(int spawnerIdx, int monsterId)
    {
        if (!IsServer) return; // 호스트만 스폰 권한

        if (_spawnPoints == null || _spawnPoints.Length == 0) return;

        // 인덱스 안전 장치
        int index = Mathf.Clamp(spawnerIdx, 0, _spawnPoints.Length - 1);
        Transform point = _spawnPoints[index];

        // 몬스터 데이터 조회
        MonsterGameData data = Managers.Data.Get<MonsterGameData>(monsterId);
        if (data == null)
        {
            Debug.LogError($"[EnemySpawner] ID {monsterId} 몬스터 데이터가 없습니다.");
            return;
        }

        // 리소스 로드 및 생성
        GameObject go = await Managers.Resource.InstantiateAsync(data.prefabPath, point.position, point.rotation);

        if (go != null)
        {
            // 네트워크 스폰
            var netObj = go.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned) netObj.Spawn();

            // 적 스탯 초기화 (Entity.Init 같은 메서드가 있다면 호출)
            var enemy = go.GetComponent<Enemy>();
            // 예: enemy.Init(data); 

            _activeEnemies.Add(go);

            // 사망 시 리스트에서 제거하기 위한 로직 필요 (Enemy 스크립트 수정 시 이벤트 연결 권장)
        }
    }

    public int GetActiveEnemyCount()
    {
        // null이 된 오브젝트(Destroy됨) 정리 후 개수 반환
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

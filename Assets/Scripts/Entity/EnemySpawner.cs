using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 적(몬스터) 스포너
/// 호스트에서만 스폰을 관리하며, NetworkManager를 통해 동기화됩니다.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private string _enemyPrefabKey = "Enemy";
    [SerializeField] private float _spawnInterval = 3f; // 스폰 간격 (초)
    [SerializeField] private int _maxAliveEnemies = 5; // 살아있을 수 있는 몬스터의 최대 수량 (이 수량이면 스폰 일시중지)
    [SerializeField] private int _maxSpawnPerRound = 10; // 이번 라운드에 스폰되는 최대 수량
    [SerializeField] private bool _autoStart = true; // 자동 시작 여부

    [Header("스폰 포인트")]
    [SerializeField] private Transform[] _spawnPoints; // 스폰 포인트 배열 (자동으로 초기화됨)

    [Header("웨이브 설정")]
    [SerializeField] private bool _useWaveSystem = false; // 웨이브 시스템 사용 여부
    [SerializeField] private int _enemiesPerWave = 5; // 웨이브당 적 수
    [SerializeField] private float _waveInterval = 10f; // 웨이브 간격 (초)

    private List<GameObject> _spawnedEnemies = new List<GameObject>();
    private Coroutine _spawnCoroutine;
    private bool _isSpawning = false;
    private int _currentWave = 0;
    private int _enemiesSpawnedInWave = 0;
    private int _spawnedInCurrentRound = 0; // 현재 라운드에서 스폰된 수량

    private void Awake()
    {
        // 스폰 포인트 초기화
        InitializeSpawnPoints();
    }

    private void Start()
    {
        // 네트워크가 활성화된 경우 호스트만 스폰 시작
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                if (_autoStart)
                {
                    StartSpawning();
                }
            }
        }
        else
        {
            // 네트워크가 없는 경우 (싱글 플레이어) 자동 시작
            if (_autoStart)
            {
                StartSpawning();
            }
        }
    }

    /// <summary>
    /// 스폰 포인트 초기화
    /// "Point"라는 이름의 자식 오브젝트들을 스폰 포인트로 사용합니다.
    /// </summary>
    private void InitializeSpawnPoints()
    {
        // GetComponentsInChildren로 모든 자식 Transform 가져오기
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        // "Point"라는 이름을 가진 자식 오브젝트들만 필터링
        List<Transform> pointList = new List<Transform>();
        foreach (Transform child in allChildren)
        {
            // 자기 자신은 제외하고, 이름이 "Point"인 것만 추가
            if (child != transform && child.name.Contains("Point"))
            {
                pointList.Add(child);
            }
        }

        if (pointList.Count > 0)
        {
            _spawnPoints = pointList.ToArray();
            Debug.Log($"[EnemySpawner] {pointList.Count}개의 스폰 포인트를 찾았습니다.");
        }
        else
        {
            // Point가 없으면 모든 자식 Transform 사용 (자기 자신 제외)
            List<Transform> childPoints = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    childPoints.Add(child);
                }
            }

            if (childPoints.Count > 0)
            {
                _spawnPoints = childPoints.ToArray();
                Debug.LogWarning($"[EnemySpawner] 'Point'라는 이름의 자식 오브젝트를 찾지 못했습니다. {childPoints.Count}개의 자식 오브젝트를 스폰 포인트로 사용합니다.");
            }
            else
            {
                // 자식이 없으면 자신의 위치를 스폰 포인트로 사용
                Debug.LogWarning($"[EnemySpawner] 스폰 포인트가 없습니다. {gameObject.name}의 위치를 사용합니다.");
                _spawnPoints = new Transform[] { transform };
            }
        }
    }

    /// <summary>
    /// 스폰 시작
    /// </summary>
    public void StartSpawning()
    {
        if (_isSpawning)
        {
            Debug.LogWarning("[EnemySpawner] 이미 스폰이 진행 중입니다.");
            return;
        }

        _isSpawning = true;
        _currentWave = 0;
        _enemiesSpawnedInWave = 0;
        _spawnedInCurrentRound = 0;

        if (_useWaveSystem)
        {
            _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
        }
        else
        {
            _spawnCoroutine = StartCoroutine(SpawnContinuousCoroutine());
        }

        Debug.Log($"[EnemySpawner] 스폰 시작 - 간격: {_spawnInterval}초, 최대 살아있는 수: {_maxAliveEnemies}, 라운드당 최대 스폰: {_maxSpawnPerRound}마리");
    }

    /// <summary>
    /// 스폰 중지
    /// </summary>
    public void StopSpawning()
    {
        if (!_isSpawning)
            return;

        _isSpawning = false;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        Debug.Log("[EnemySpawner] 스폰 중지");
    }

    /// <summary>
    /// 지속적인 스폰 코루틴 (웨이브 시스템 미사용)
    /// 1마리씩 스폰하며, 살아있는 몬스터가 최대치에 도달하면 일시 중지합니다.
    /// 라운드당 최대 스폰 수량에 도달하면 라운드를 종료합니다.
    /// </summary>
    private IEnumerator SpawnContinuousCoroutine()
    {
        while (_isSpawning)
        {
            // 사망한 적들을 리스트에서 제거
            CleanupDeadEnemies();
            
            // 라운드당 최대 스폰 수량 체크
            if (_spawnedInCurrentRound >= _maxSpawnPerRound)
            {
                Debug.Log($"[EnemySpawner] 라운드 완료! 총 {_spawnedInCurrentRound}마리 스폰됨. 스폰 중지.");
                StopSpawning();
                yield break;
            }
            
            // 현재 살아있는 몬스터 수 체크
            int currentAliveCount = _spawnedEnemies.Count;
            
            // 살아있는 몬스터가 최대치 미만이면 스폰
            if (currentAliveCount < _maxAliveEnemies)
            {
                Debug.Log($"[EnemySpawner] 스폰 시도... (살아있는: {currentAliveCount}/{_maxAliveEnemies}, 라운드 스폰: {_spawnedInCurrentRound}/{_maxSpawnPerRound})");
                
                // 1마리씩 스폰 (코루틴에서 비동기 작업을 기다림)
                yield return StartCoroutine(SpawnEnemyCoroutine());
            }
            else
            {
                // 살아있는 몬스터가 최대치에 도달하면 일시 중지
                Debug.Log($"[EnemySpawner] 살아있는 몬스터가 최대치({_maxAliveEnemies})에 도달. 스폰 일시 중지. (현재: {currentAliveCount}/{_maxAliveEnemies})");
            }

            // 스폰 간격만큼 대기
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    /// <summary>
    /// 웨이브 시스템 스폰 코루틴
    /// </summary>
    private IEnumerator SpawnWaveCoroutine()
    {
        while (_isSpawning)
        {
            _currentWave++;
            _enemiesSpawnedInWave = 0;
            Debug.Log($"[EnemySpawner] 웨이브 {_currentWave} 시작");

            // 웨이브당 적 스폰
            while (_enemiesSpawnedInWave < _enemiesPerWave && _isSpawning)
            {
                CleanupDeadEnemies();
                if (_spawnedEnemies.Count >= _maxAliveEnemies)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                SpawnEnemy();
                _enemiesSpawnedInWave++;

                yield return new WaitForSeconds(_spawnInterval);
            }

            Debug.Log($"[EnemySpawner] 웨이브 {_currentWave} 완료. 다음 웨이브까지 대기...");
            yield return new WaitForSeconds(_waveInterval);
        }
    }

    /// <summary>
    /// 적 스폰 코루틴 (비동기 작업을 안전하게 처리)
    /// </summary>
    private IEnumerator SpawnEnemyCoroutine()
    {
        //// 네트워크 체크 (호스트/서버만 스폰)
        //if (NetworkManager.Singleton != null)
        //{
        //    if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
        //    {
        //        Debug.LogWarning("[EnemySpawner] 호스트/서버가 아닙니다. 스폰을 건너뜁니다.");
        //        yield break;
        //    }
        //}

        // Managers.Resource null 체크
        if (Managers.Resource == null)
        {
            Debug.LogError("[EnemySpawner] Managers.Resource가 null입니다. 스폰을 건너뜁니다.");
            yield break;
        }

        // 스폰 포인트 선택
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogError("[EnemySpawner] 스폰 포인트가 없습니다.");
            yield break;
        }

        Debug.Log($"[EnemySpawner] 프리팹 로드 시작: {_enemyPrefabKey}");

        // 비동기 작업을 코루틴에서 기다림
        var loadTask = Managers.Resource.InstantiateAsync(
            _enemyPrefabKey,
            position: spawnPoint.position,
            rotation: spawnPoint.rotation
        );

        // Task가 완료될 때까지 대기 (yield는 try-catch 밖에서)
        while (!loadTask.IsCompleted)
        {
            yield return null;
        }

        // Task 완료 후 예외 확인
        GameObject enemyObj = null;
        if (loadTask.IsFaulted)
        {
            Debug.LogError($"[EnemySpawner] 프리팹 로드 중 예외 발생: {loadTask.Exception?.GetBaseException()?.Message}");
            if (loadTask.Exception != null)
            {
                Debug.LogError($"[EnemySpawner] 스택 트레이스: {loadTask.Exception.StackTrace}");
            }
            yield break;
        }

        enemyObj = loadTask.Result;

        if (enemyObj == null)
        {
            Debug.LogError($"[EnemySpawner] 적 스폰 실패: {_enemyPrefabKey} - InstantiateAsync가 null을 반환했습니다.");
            yield break;
        }

        Debug.Log($"[EnemySpawner] 프리팹 인스턴스화 완료: {enemyObj.name}");

        // 네트워크 오브젝트인 경우 스폰
        if (NetworkManager.Singleton != null)
        {
            NetworkObject networkObject = enemyObj.GetComponent<NetworkObject>();
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn();
                Debug.Log($"[EnemySpawner] 네트워크 오브젝트 스폰 완료");
            }
        }

        _spawnedEnemies.Add(enemyObj);
        _spawnedInCurrentRound++;
        Debug.Log($"[EnemySpawner] 적 스폰 완료: {enemyObj.name} at {spawnPoint.position} (살아있는: {_spawnedEnemies.Count}/{_maxAliveEnemies}, 라운드 스폰: {_spawnedInCurrentRound}/{_maxSpawnPerRound})");
    }

    /// <summary>
    /// 적 스폰 (레거시 메서드 - 호환성을 위해 유지)
    /// </summary>
    private async void SpawnEnemy()
    {
        try
        {
            Debug.Log($"[EnemySpawner] SpawnEnemy() 시작");
            
            //// 네트워크 체크 (호스트/서버만 스폰)
            //if (NetworkManager.Singleton != null)
            //{
            //    if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
            //    {
            //        Debug.LogWarning("[EnemySpawner] 호스트/서버가 아닙니다. 스폰을 건너뜁니다.");
            //        return;
            //    }
            //}

            // Managers.Resource null 체크
            if (Managers.Resource == null)
            {
                Debug.LogError("[EnemySpawner] Managers.Resource가 null입니다. 스폰을 건너뜁니다.");
                return;
            }

            // 스폰 포인트 선택
            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogError("[EnemySpawner] 스폰 포인트가 없습니다.");
                return;
            }

            Debug.Log($"[EnemySpawner] 프리팹 로드 시작: {_enemyPrefabKey}");
            
            // 적 프리팹 로드 및 인스턴스화
            GameObject enemyObj = await Managers.Resource.InstantiateAsync(
                _enemyPrefabKey,
                position: spawnPoint.position,
                rotation: spawnPoint.rotation
            );

            if (enemyObj == null)
            {
                Debug.LogError($"[EnemySpawner] 적 스폰 실패: {_enemyPrefabKey} - InstantiateAsync가 null을 반환했습니다.");
                return;
            }

            Debug.Log($"[EnemySpawner] 프리팹 인스턴스화 완료: {enemyObj.name}");

            // 네트워크 오브젝트인 경우 스폰
            if (NetworkManager.Singleton != null)
            {
                NetworkObject networkObject = enemyObj.GetComponent<NetworkObject>();
                if (networkObject != null && !networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                    Debug.Log($"[EnemySpawner] 네트워크 오브젝트 스폰 완료");
                }
            }

            _spawnedEnemies.Add(enemyObj);
            _spawnedInCurrentRound++;
            Debug.Log($"[EnemySpawner] 적 스폰 완료: {enemyObj.name} at {spawnPoint.position} (살아있는: {_spawnedEnemies.Count}/{_maxAliveEnemies}, 라운드 스폰: {_spawnedInCurrentRound}/{_maxSpawnPerRound})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EnemySpawner] 스폰 중 예외 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 랜덤 스폰 포인트 선택
    /// </summary>
    private Transform GetRandomSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            return transform;
        }

        return _spawnPoints[Random.Range(0, _spawnPoints.Length)];
    }

    /// <summary>
    /// 사망한 적들을 리스트에서 제거
    /// 비활성화된 오브젝트(풀로 반환된 것)도 제거합니다.
    /// </summary>
    private void CleanupDeadEnemies()
    {
        int beforeCount = _spawnedEnemies.Count;
        
        _spawnedEnemies.RemoveAll(enemy =>
        {
            // null 체크
            if (enemy == null)
                return true;

            // 비활성화된 오브젝트는 풀로 반환된 것이므로 제거
            if (!enemy.activeSelf)
                return true;

            // Enemy 컴포넌트 확인
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent == null)
                return false;

            // 체력이 0 이하인 경우 제거
            if (enemyComponent.Hp <= 0)
                return true;

            return false;
        });

        int removedCount = beforeCount - _spawnedEnemies.Count;
        if (removedCount > 0)
        {
            Debug.Log($"[EnemySpawner] {removedCount}개의 사망한 적을 리스트에서 제거했습니다. (살아있는: {_spawnedEnemies.Count}/{_maxAliveEnemies})");
        }
    }

    /// <summary>
    /// 모든 적 제거
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in _spawnedEnemies)
        {
            if (enemy != null)
            {
                Managers.Pool?.Despawn(enemy);
            }
        }
        _spawnedEnemies.Clear();
    }

    private void OnDestroy()
    {
        StopSpawning();
    }

    // 에디터에서 스폰 포인트 시각화
    private void OnDrawGizmos()
    {
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (var point in _spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
        else
        {
            // Awake 전에도 시각화를 위해 자식 오브젝트 확인
            Gizmos.color = Color.yellow;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.activeSelf && child.name.Contains("Point"))
                {
                    Gizmos.DrawWireSphere(child.position, 0.5f);
                }
            }
        }
    }
}


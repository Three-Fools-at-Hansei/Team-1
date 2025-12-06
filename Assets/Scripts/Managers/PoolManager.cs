using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;

public class PoolManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Pool;

    private readonly Dictionary<int, IObjectPool<GameObject>> _pools = new();

    // NGO에 등록된 프리팹 핸들러의 해시를 직접 추적 관리하기 위한 집합
    private readonly HashSet<uint> _registeredNetPrefabHashes = new();

    private Transform _root;

    private Transform Root
    {
        get
        {
            if (_root == null)
            {
                GameObject rootGo = GameObject.Find("@PoolRoot") ?? new GameObject { name = "@PoolRoot" };
                _root = rootGo.transform;
            }
            return _root;
        }
    }

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();

        _pools.Clear();
        _registeredNetPrefabHashes.Clear();

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    /// <summary>
    /// 프리팹 원본을 받아 풀에서 GameObject 인스턴스를 가져옵니다.
    /// 만약 해당 프리팹의 풀이 없다면 새로 생성합니다.
    /// </summary>
    /// <param name="prefab">인스턴스화할 프리팹 원본</param>
    /// <param name="position">배치될 위치</param>
    /// <param name="rotation">초기 회전값</param>
    /// <param name="parent">부모 Transform</param>
    /// <param name="defaultCapacity">풀의 기본 용량</param>
    /// <param name="maxSize">풀의 최대 용량</param>
    /// <returns>풀에서 나온 활성화된 GameObject 인스턴스</returns>
    public GameObject Spawn(GameObject prefab, Vector3? position = null, Quaternion? rotation = null, Transform parent = null, int defaultCapacity = 10, int maxSize = 50)
    {
        int key = prefab.GetInstanceID();

        if (!_pools.TryGetValue(key, out var pool))
        {
            // [수정] 풀 생성 시점에 이 프리팹이 NetworkObject인지 미리 확인합니다.
            bool isNetworkObject = prefab.GetComponent<NetworkObject>() != null;

            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject go = Object.Instantiate(prefab, Root);
                    go.name = prefab.name;

                    go.GetOrAddComponent<Poolable>().PoolKey = key;
                    return go;
                },
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go =>
                {
                    if (go != null)
                    {
                        go.SetActive(false);

                        // [핵심 수정] NetworkObject가 아닐 경우에만 부모를 Root로 정리합니다.
                        // NetworkObject는 Despawn 과정 중 부모 변경 시 에러가 발생하므로 그대로 둡니다.
                        if (!isNetworkObject)
                        {
                            go.transform.SetParent(Root);
                        }
                    }
                },
                actionOnDestroy: go => Object.Destroy(go),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
            _pools.Add(key, pool);
        }

        GameObject go = pool.Get();

        if (go.transform is RectTransform rectTransform)
        {
            RectTransform prefabRectTransform = prefab.transform as RectTransform;
            rectTransform.localPosition = prefabRectTransform.localPosition; // z값까지 초기화
            rectTransform.localScale = prefabRectTransform.localScale;
            rectTransform.localRotation = prefabRectTransform.localRotation;
            rectTransform.sizeDelta = prefabRectTransform.sizeDelta;
            rectTransform.anchoredPosition = prefabRectTransform.anchoredPosition;
        }

        // 재사용 시점에 부모를 다시 설정해주므로, NetworkObject도 이 시점엔 안전하게 부모가 변경됩니다.
        go.transform.SetParent(parent, false);

        // 값이 있을 경우 UI가 아닌 오브젝트로 판단
        if (position.HasValue || rotation.HasValue)
        {
            Vector3 finalPosition = position ?? go.transform.position;
            Quaternion finalRotation = rotation ?? go.transform.rotation;
            go.transform.SetPositionAndRotation(finalPosition, finalRotation);
        }

        return go;
    }

    /// <summary>
    /// 사용이 끝난 GameObject를 풀에 반환합니다.
    /// </summary>
    /// <param name="go">반환할 GameObject</param>
    public void Despawn(GameObject go)
    {
        if (go == null)
            return;

        if (go.TryGetComponent<Poolable>(out var poolable) && _pools.TryGetValue(poolable.PoolKey, out var pool))
        {
            pool.Release(go);
        }
        else
        {
            Debug.LogWarning($"[PoolManager] 오브젝트 '{go.name}'는 풀에 할당되지 않았습니다. Destroy를 호출합니다.");
            Object.Destroy(go);
        }
    }

    // ========================================================================
    // Network Object Pooling Extensions
    // ========================================================================

    /// <summary>
    /// 특정 프리팹을 NGO의 Custom Prefab Handler(NetworkObjectPool)에 등록합니다.
    /// </summary>
    public void RegisterNetworkPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        if (!prefab.TryGetComponent(out NetworkObject netObj))
        {
            Debug.LogWarning($"[PoolManager] {prefab.name}에는 NetworkObject 컴포넌트가 없어 네트워크 풀링을 등록할 수 없습니다.");
            return;
        }

        if (NetworkManager.Singleton == null || NetworkManager.Singleton.PrefabHandler == null)
        {
            Debug.LogError("[PoolManager] NetworkManager가 초기화되지 않았습니다.");
            return;
        }

        uint hash = netObj.PrefabIdHash;
        var handler = NetworkManager.Singleton.PrefabHandler;

        // 직접 관리하는 HashSet을 통해 중복 확인
        if (_registeredNetPrefabHashes.Contains(hash))
        {
            handler.RemoveHandler(hash);
            _registeredNetPrefabHashes.Remove(hash);
        }

        // NetworkObjectPool 생성 및 등록
        handler.AddHandler(hash, new NetworkObjectPool(prefab));
        _registeredNetPrefabHashes.Add(hash); // 등록 목록에 추가

        Debug.Log($"[PoolManager] 네트워크 풀 핸들러 등록 완료: {prefab.name} (Hash: {hash})");
    }

    /// <summary>
    /// 등록된 네트워크 프리팹 핸들러를 해제합니다.
    /// </summary>
    public void UnregisterNetworkPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        if (prefab.TryGetComponent(out NetworkObject netObj))
        {
            uint hash = netObj.PrefabIdHash;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.PrefabHandler != null)
            {
                // HashSet 확인 후 제거
                if (_registeredNetPrefabHashes.Contains(hash))
                {
                    NetworkManager.Singleton.PrefabHandler.RemoveHandler(hash);
                    _registeredNetPrefabHashes.Remove(hash);
                }
            }
        }
    }
}
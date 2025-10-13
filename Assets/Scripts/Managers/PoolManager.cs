using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Pool;

    private readonly Dictionary<int, IObjectPool<GameObject>> _pools = new();
    private Transform _root;

    public void Init()
    {
        if (_root == null)
        {
            GameObject root = GameObject.Find("@PoolRoot") ?? new GameObject { name = "@PoolRoot" };
            Object.DontDestroyOnLoad(root);
            _root = root.transform;
        }
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();

        _pools.Clear();
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
            pool = new ObjectPool<GameObject>(
                createFunc: () =>
                {
                    GameObject go = Object.Instantiate(prefab, _root);
                    go.name = prefab.name;

                    go.GetOrAddComponent<Poolable>().PoolKey = key;
                    return go;
                },
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go =>
                {
                    go.transform.SetParent(_root);
                    go.SetActive(false);
                },
                actionOnDestroy: go => Object.Destroy(go),
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
            _pools.Add(key, pool);
        }

        GameObject go = pool.Get();
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
}
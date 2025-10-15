using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Pool;

    private readonly Dictionary<int, IObjectPool<GameObject>> _pools = new();
    private Transform _root;

    /// <summary>
    /// 현재 씬의 Pool Root Transform을 반환합니다.
    /// Root가 없거나 씬 전환으로 인해 파괴된 경우, 현재 씬에 새로 생성합니다.
    /// </summary>
    private Transform Root
    {
        get
        {
            if (_root == null)
            {
                GameObject rootGo = GameObject.Find("@PoolRoot") ?? new GameObject { name = "@PoolRoot" };
                _root = rootGo.transform;
                // Why: @PoolRoot는 씬에 종속적인 오브젝트들을 담는 컨테이너이므로,
                // 씬이 파괴될 때 함께 파괴되어야 메모리 누수를 막을 수 있습니다.
                // 따라서 DontDestroyOnLoad를 호출하지 않습니다.
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

        // Why: 씬이 전환될 때 Managers에 의해 Clear가 호출됩니다.
        // 이때 _root 참조를 null로 설정해야, 다음 씬에서 Root 프로퍼티가 호출될 때
        // 새로운 씬의 @PoolRoot를 찾거나 생성하게 되어 씬별로 풀이 올바르게 관리됩니다.
        _root = null;

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
                    // 수정 이유: _root 필드 대신 Root 프로퍼티를 사용하여 항상 현재 씬에 종속적인 @PoolRoot를 참조하도록 합니다.
                    GameObject go = Object.Instantiate(prefab, Root);
                    go.name = prefab.name;

                    go.GetOrAddComponent<Poolable>().PoolKey = key;
                    return go;
                },
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go =>
                {
                    // 수정 이유: 오브젝트를 풀에 반환할 때, Root 프로퍼티를 통해 현재 활성화된 씬의 @PoolRoot 하위로 이동시킵니다.
                    // 이렇게 하면 DontDestroyOnLoad 문제가 발생하지 않습니다.
                    if (go != null) go.transform.SetParent(Root);
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

        // UI 오브젝트일 경우, 재사용 직전에 UI의 rect transform 초기화
        // (풀 반납 시의 transform 초기화)
        if (go.transform is RectTransform rectTransform)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

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
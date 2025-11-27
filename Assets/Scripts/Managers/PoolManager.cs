using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Pool;

    private readonly Dictionary<int, IObjectPool<GameObject>> _pools = new();
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
        Debug.Log($"{ManagerType} Manager Init �մϴ�.");
    }

    public void Update() { }

    public void Clear()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();

        _pools.Clear();
        Debug.Log($"{ManagerType} Manager Clear �մϴ�.");
    }

    /// <summary>
    /// ������ ������ �޾� Ǯ���� GameObject �ν��Ͻ��� �����ɴϴ�.
    /// ���� �ش� �������� Ǯ�� ���ٸ� ���� �����մϴ�.
    /// </summary>
    /// <param name="prefab">�ν��Ͻ�ȭ�� ������ ����</param>
    /// <param name="position">��ġ�� ��ġ</param>
    /// <param name="rotation">�ʱ� ȸ����</param>
    /// <param name="parent">�θ� Transform</param>
    /// <param name="defaultCapacity">Ǯ�� �⺻ �뷮</param>
    /// <param name="maxSize">Ǯ�� �ִ� �뷮</param>
    /// <returns>Ǯ���� ���� Ȱ��ȭ�� GameObject �ν��Ͻ�</returns>
    public GameObject Spawn(GameObject prefab, Vector3? position = null, Quaternion? rotation = null, Transform parent = null, int defaultCapacity = 10, int maxSize = 50)
    {
        int key = prefab.GetInstanceID();

        if (!_pools.TryGetValue(key, out var pool))
        {
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
                        go.transform.SetParent(Root);
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
        Debug.Log($"[PoolManager] Spawn: prefab={prefab.name}, go={go.name}, parent={parent?.name ?? "null"}");

        if (go.transform is RectTransform rectTransform)
        {
            RectTransform prefabRectTransform = prefab.transform as RectTransform;
            rectTransform.localPosition = prefabRectTransform.localPosition; // z������ �ʱ�ȭ
            rectTransform.localScale = prefabRectTransform.localScale;
            rectTransform.localRotation = prefabRectTransform.localRotation;
        }

        // parent가 null이 아닐 때만 SetParent 호출 (null이면 씬 루트에 유지)
        if (parent != null)
        {
            Debug.Log($"[PoolManager] SetParent 호출: {go.name} -> {parent.name}");
            go.transform.SetParent(parent, false);
        }
        else
        {
            Debug.Log($"[PoolManager] parent가 null이므로 SetParent 호출 안 함: {go.name}");
        }
        
        // ���� ���� ��� UI�� �ƴ� ������Ʈ�� �Ǵ�
        if (position.HasValue || rotation.HasValue)
        {
            Vector3 finalPosition = position ?? go.transform.position;
            Quaternion finalRotation = rotation ?? go.transform.rotation;
            go.transform.SetPositionAndRotation(finalPosition, finalRotation);
        }

        return go;
    }

    /// <summary>
    /// ����� ���� GameObject�� Ǯ�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="go">��ȯ�� GameObject</param>
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
            Debug.LogWarning($"[PoolManager] ������Ʈ '{go.name}'�� Ǯ�� �Ҵ���� �ʾҽ��ϴ�. Destroy�� ȣ���մϴ�.");
            Object.Destroy(go);
        }
    }
}
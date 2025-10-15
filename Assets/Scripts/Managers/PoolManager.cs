using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Pool;

    private readonly Dictionary<int, IObjectPool<GameObject>> _pools = new();
    private Transform _root;

    /// <summary>
    /// ���� ���� Pool Root Transform�� ��ȯ�մϴ�.
    /// Root�� ���ų� �� ��ȯ���� ���� �ı��� ���, ���� ���� ���� �����մϴ�.
    /// </summary>
    private Transform Root
    {
        get
        {
            if (_root == null)
            {
                GameObject rootGo = GameObject.Find("@PoolRoot") ?? new GameObject { name = "@PoolRoot" };
                _root = rootGo.transform;
                // Why: @PoolRoot�� ���� �������� ������Ʈ���� ��� �����̳��̹Ƿ�,
                // ���� �ı��� �� �Բ� �ı��Ǿ�� �޸� ������ ���� �� �ֽ��ϴ�.
                // ���� DontDestroyOnLoad�� ȣ������ �ʽ��ϴ�.
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

        // Why: ���� ��ȯ�� �� Managers�� ���� Clear�� ȣ��˴ϴ�.
        // �̶� _root ������ null�� �����ؾ�, ���� ������ Root ������Ƽ�� ȣ��� ��
        // ���ο� ���� @PoolRoot�� ã�ų� �����ϰ� �Ǿ� ������ Ǯ�� �ùٸ��� �����˴ϴ�.
        _root = null;

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
                    // ���� ����: _root �ʵ� ��� Root ������Ƽ�� ����Ͽ� �׻� ���� ���� �������� @PoolRoot�� �����ϵ��� �մϴ�.
                    GameObject go = Object.Instantiate(prefab, Root);
                    go.name = prefab.name;

                    go.GetOrAddComponent<Poolable>().PoolKey = key;
                    return go;
                },
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go =>
                {
                    // ���� ����: ������Ʈ�� Ǯ�� ��ȯ�� ��, Root ������Ƽ�� ���� ���� Ȱ��ȭ�� ���� @PoolRoot ������ �̵���ŵ�ϴ�.
                    // �̷��� �ϸ� DontDestroyOnLoad ������ �߻����� �ʽ��ϴ�.
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

        // UI ������Ʈ�� ���, ���� ������ UI�� rect transform �ʱ�ȭ
        // (Ǯ �ݳ� ���� transform �ʱ�ȭ)
        if (go.transform is RectTransform rectTransform)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        go.transform.SetParent(parent, false);

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
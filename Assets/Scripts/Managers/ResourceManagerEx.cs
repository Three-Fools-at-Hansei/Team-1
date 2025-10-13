using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManagerEx : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Resource;

    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init �մϴ�.");
    }

    public void Update() { }

    public void Clear()
    {
        // �� ��ȯ �� �ε��ߴ� ��� �������� �޸𸮿��� ����
        foreach (var handle in _prefabHandles.Values)
            Addressables.Release(handle);

        _prefabHandles.Clear();

        Debug.Log($"{ManagerType} Manager Clear �մϴ�.");
    }

    /// <summary>
    /// �������� �񵿱������� �ν��Ͻ�ȭ�մϴ�. ���������� PoolManager�� ����մϴ�.
    /// </summary>
    /// <param name="key">�ν��Ͻ�ȭ�� �������� Addressable �ּ�</param>
    /// <param name="position">������ ��ġ</param>
    /// <param name="rotation">������ ȸ����</param>
    /// <param name="parent">�θ� Transform</param>
    /// <param name="defaultCapacity">������ Ǯ�� �⺻ �뷮</param>
    /// <param name="maxSize">������ Ǯ�� �ִ� �뷮</param>
    /// <returns>������ GameObject�� �����ϴ� Task. ������ �ε� ���� �� null�� ��ȯ�մϴ�.</returns>
    public async Task<GameObject> InstantiateAsync(string key, Vector3? position = null, Quaternion? rotation = null, Transform parent = null, int defaultCapacity = 10, int maxSize = 50)
    {
        // LoadPrefabAsyncTask�� await �Ͽ� �������� �ε�� ������ �񵿱������� ����մϴ�.
        GameObject prefab = await LoadPrefabAsyncTask(key);

        if (prefab == null)
        {
            Debug.LogError($"[ResourceManager] ��üȭ�� �����߽��ϴ�. addressable key: {key}");
            return null;
        }

        // ������ �ε尡 �Ϸ�� �� PoolManager�� ���� ���������� ��ü�� �����ϰ� ��ȯ�մϴ�.
        GameObject go = Managers.Pool.Spawn(prefab, position, rotation, parent, defaultCapacity, maxSize);
        return go;
    }

    /// <summary>
    /// ����� ���� GameObject�� Ǯ�� �ݳ��մϴ�.
    /// Ǯ�� �ݳ��� �� ���� ��� Destroy�մϴ�.
    /// </summary>
    /// <param name="go">Ǯ�� �ݳ��� GameObject</param>
    public void Destroy(GameObject go)
    {
        Managers.Pool.Despawn(go);
    }

    /// <summary>
    /// Addressable �ּҸ� �̿��� �������� �񵿱� �ε��մϴ�.
    /// </summary>
    /// <param name="key">Addressable �ּ�</param>
    /// <returns>�ε�� ������ GameObject�� �����ϴ� Task. ���� �� null�� �����մϴ�.</returns>
    private Task<GameObject> LoadPrefabAsyncTask(string key)
    {
        // �̹� �ε� ��û�� �־��� �ڵ����� Ȯ���մϴ�.
        if (_prefabHandles.TryGetValue(key, out var handle))
        {
            // Why: AsyncOperationHandle.Task�� �۾��� �ϷḦ ��Ÿ���� Task�� ��ȯ�ϹǷ�,
            // �̹� �ε尡 ���� ���̰ų� �Ϸ�� ��� �ش� Task�� ��� ��ȯ�Ͽ� �ߺ� ó���� �����մϴ�.
            Debug.Log($"[ResourceManager] ������ �ε尡 ���� ���̰ų� �Ϸ�Ǿ����ϴ�. addressables key: {key}");
            return handle.Task;
        }

        // ���ο� �ε� ��û�� �����ϰ� ��� ��ųʸ��� �߰��Ͽ�, ���� ������ ���� �ٸ� ��û�� �ߺ����� �ε��ϴ� ���� �����մϴ�.
        var newHandle = Addressables.LoadAssetAsync<GameObject>(key);
        _prefabHandles.Add(key, newHandle);

        newHandle.Completed += (op) =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ResourceManager] ������ �ε带 �����߽��ϴ�. addressables key: {key} - {op.OperationException}");
                // �ε忡 ������ �ڵ��� ��ųʸ����� �����Ͽ� �޸� ������ �����ϰ�, ���� ��û �� ��õ��� ����մϴ�.
                _prefabHandles.Remove(key);
            }
        };

        return newHandle.Task;
    }
}
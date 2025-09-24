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
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        // 씬 전환 시 로드했던 모든 프리팹을 메모리에서 해제
        foreach (var handle in _prefabHandles.Values)
            Addressables.Release(handle);

        _prefabHandles.Clear();

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    /// <summary>
    /// 프리팹을 비동기적으로 인스턴스화합니다. 내부적으로 PoolManager를 사용합니다.
    /// </summary>
    /// <param name="key">인스턴스화할 프리팹의 Addressable 주소</param>
    /// <param name="position">생성될 위치</param>
    /// <param name="rotation">생성될 회전값</param>
    /// <param name="parent">부모 Transform</param>
    /// <param name="defaultCapacity">생성할 풀의 기본 용량</param>
    /// <param name="maxSize">생성할 풀의 최대 용량</param>
    /// <returns>생성된 GameObject를 포함하는 Task. 프리팹 로드 실패 시 null을 반환합니다.</returns>
    public async Task<GameObject> InstantiateAsync(string key, Vector3? position = null, Quaternion? rotation = null, Transform parent = null, int defaultCapacity = 10, int maxSize = 50)
    {
        // LoadPrefabAsyncTask를 await 하여 프리팹이 로드될 때까지 비동기적으로 대기합니다.
        GameObject prefab = await LoadPrefabAsyncTask(key);

        if (prefab == null)
        {
            Debug.LogError($"[ResourceManager] 객체화에 실패했습니다. addressable key: {key}");
            return null;
        }

        // 프리팹 로드가 완료된 후 PoolManager를 통해 동기적으로 객체를 생성하고 반환합니다.
        GameObject go = Managers.Pool.Spawn(prefab, position, rotation, parent, defaultCapacity, maxSize);
        return go;
    }

    /// <summary>
    /// 사용이 끝난 GameObject를 풀에 반납합니다.
    /// 풀에 반납할 수 없을 경우 Destroy합니다.
    /// </summary>
    /// <param name="go">풀에 반납할 GameObject</param>
    public void Destroy(GameObject go)
    {
        Managers.Pool.Despawn(go);
    }

    /// <summary>
    /// Addressable 주소를 이용해 프리팹을 비동기 로드합니다.
    /// </summary>
    /// <param name="key">Addressable 주소</param>
    /// <returns>로드된 프리팹 GameObject를 포함하는 Task. 실패 시 null을 포함합니다.</returns>
    private Task<GameObject> LoadPrefabAsyncTask(string key)
    {
        // 이미 로드 요청이 있었던 핸들인지 확인합니다.
        if (_prefabHandles.TryGetValue(key, out var handle))
        {
            // Why: AsyncOperationHandle.Task는 작업의 완료를 나타내는 Task를 반환하므로,
            // 이미 로드가 진행 중이거나 완료된 경우 해당 Task를 즉시 반환하여 중복 처리를 방지합니다.
            Debug.Log($"[ResourceManager] 프리팹 로드가 진행 중이거나 완료되었습니다. addressables key: {key}");
            return handle.Task;
        }

        // 새로운 로드 요청을 생성하고 즉시 딕셔너리에 추가하여, 동일 프레임 내의 다른 요청이 중복으로 로드하는 것을 방지합니다.
        var newHandle = Addressables.LoadAssetAsync<GameObject>(key);
        _prefabHandles.Add(key, newHandle);

        newHandle.Completed += (op) =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ResourceManager] 프리팹 로드를 실패했습니다. addressables key: {key} - {op.OperationException}");
                // 로드에 실패한 핸들은 딕셔너리에서 제거하여 메모리 누수를 방지하고, 다음 요청 시 재시도를 허용합니다.
                _prefabHandles.Remove(key);
            }
        };

        return newHandle.Task;
    }
}
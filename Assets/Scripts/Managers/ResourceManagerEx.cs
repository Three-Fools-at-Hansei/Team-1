using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManagerEx : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Resource;

    private readonly Dictionary<string, AsyncOperationHandle> _handles = new();

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        // 씬 전환 시 로드했던 모든 프리팹을 메모리에서 해제
        foreach (var handle in _handles.Values)
            Addressables.Release(handle);

        _handles.Clear();

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    /////////

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
        GameObject prefab = await LoadAsync<GameObject>(key);

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
    /// 지정된 주소(key)의 에셋을 비동기적으로 로드합니다.
    /// 이미 로드되었거나 로딩 중인 경우, 기존 작업을 반환하여 중복 로딩을 방지합니다.
    /// </summary>
    /// <typeparam name="T">로드할 에셋의 타입 (TextAsset, Sprite 등등)</typeparam>
    /// <param name="key">로드할 에셋의 Addressable 주소</param>
    /// <returns>로드가 완료되면 해당 에셋을 반환하는 Task</returns>
    public async Task<T> LoadAsync<T>(string key) where T : class
    {
        // 이미 로드 요청이 있었는지 확인하여 중복 작업을 막습니다.
        if (_handles.TryGetValue(key, out var handle))
        {
            // 기존 핸들을 원하는 타입<T>으로 변환하여 Task를 반환합니다.
            return await handle.Convert<T>().Task;
        }

        var newHandle = Addressables.LoadAssetAsync<T>(key);
        _handles.Add(key, newHandle);

        newHandle.Completed += (op) =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ResourceManager] 에셋 로드 실패. key: {key} - {op.OperationException}");
                _handles.Remove(key);
            }
        };

        return await newHandle.Task;
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
}
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Netcode for GameObjects(NGO)의 스폰/디스폰 처리를
/// 프로젝트의 PoolManager로 우회시켜주는 핸들러입니다.
/// </summary>
public class NetworkObjectPool : INetworkPrefabInstanceHandler
{
    private readonly GameObject _prefab;

    public NetworkObjectPool(GameObject prefab)
    {
        _prefab = prefab;
    }

    /// <summary>
    /// NGO가 객체 생성을 요청할 때 호출됩니다. (클라이언트 측)
    /// </summary>
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        // 실제 인스턴스화는 PoolManager를 통해 수행
        GameObject go = Managers.Pool.Spawn(_prefab, position, rotation);

        // 생성된 객체의 NetworkObject 반환
        return go.GetComponent<NetworkObject>();
    }

    /// <summary>
    /// NGO가 객체 파괴를 요청할 때 호출됩니다. (클라이언트 측)
    /// </summary>
    public void Destroy(NetworkObject networkObject)
    {
        // 실제 파괴 대신 PoolManager를 통해 반환(Despawn) 수행
        Managers.Pool.Despawn(networkObject.gameObject);
    }
}
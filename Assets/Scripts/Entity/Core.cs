using System;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 코어 오브젝트
/// </summary>
public class Core : Entity
{
    public static Core Instance { get; private set; }

    public event Action CoreDestroyed;
    protected override Color DamageTextColor => Color.cyan;


    protected override void Awake()
    {
        base.Awake();
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Core] 이미 다른 Core 인스턴스가 존재합니다.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public override void Attack()
    {
        // 코어는 공격하지 않음
    }

    protected override void Die()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            CombatGameManager.Instance?.TriggerDefeat();
        }
        OnCoreDestroyed();
    }

    private void OnCoreDestroyed()
    {
        Debug.Log("[Core] 코어가 파괴되었습니다.");
        CoreDestroyed?.Invoke();

        // 코어 파괴 시 비활성화 (서버에서 끄면 클라이언트도 꺼짐)
        gameObject.SetActive(false);
    }
}


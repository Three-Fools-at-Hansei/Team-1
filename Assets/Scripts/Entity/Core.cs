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

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Core] 이미 다른 Core 인스턴스가 존재합니다.");
        }

        Instance = this;
    }


    public override void Attack()
    {
        // 코어는 공격하지 않음
    }

    public override void TakeDamage(int damage)
    {
        if (IsDead())
            return;

        _hp = Mathf.Max(0, _hp - damage);
        UpdateHealthBar();

        // 서버에서만 사망 체크 및 게임 오버 트리거
        if (IsDead())
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                CombatGameManager.Instance?.TriggerDefeat();
            }
            OnCoreDestroyed();
        }
    }

    private void OnCoreDestroyed()
    {
        Debug.Log("[Core] 코어가 파괴되었습니다.");
        CoreDestroyed?.Invoke();
        gameObject.SetActive(false);
    }
}


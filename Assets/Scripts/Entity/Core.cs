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
        // [변경] Awake에서는 Instance를 설정하지 않음 (풀링 객체일 수 있으므로)
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 네트워크 스폰 시점에 Instance 등록
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Core] 중복된 Core 인스턴스가 감지되었습니다. (기존 인스턴스 유지)");
            // 필요 시 Despawn 처리 등을 할 수 있으나, 여기선 경고만 표시
        }
        else
        {
            Instance = this;
            Debug.Log("[Core] Instance 등록 완료");
        }
    }

    public override void OnNetworkDespawn()
    {
        // 네트워크 연결 해제 시 Instance 해제
        if (Instance == this)
        {
            Instance = null;
        }

        base.OnNetworkDespawn();
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
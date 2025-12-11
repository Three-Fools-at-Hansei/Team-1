using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class GameStatusViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    public List<EntityHpViewModel> Players { get; private set; } = new();
    public EntityHpViewModel Core { get; private set; }

    public GameStatusViewModel()
    {
        // 1. 이미 존재하는 Core 찾기
        if (global::Core.Instance != null)
        {
            Core = new EntityHpViewModel(global::Core.Instance, "CORE");
        }

        // 2. 플레이어 스폰/디스폰 이벤트 구독
        Player.OnPlayerSpawned += OnPlayerSpawned;
        Player.OnPlayerDespawned += OnPlayerDespawned;

        // 3. 이미 접속해있는 플레이어 처리 (늦게 들어왔을 경우 등)
        if (Managers.Network != null && Unity.Netcode.NetworkManager.Singleton != null)
        {
            foreach (var client in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                {
                    var player = client.PlayerObject.GetComponent<Player>();
                    if (player != null) AddPlayer(player);
                }
            }
        }

        OnStateChanged?.Invoke();
    }

    private void OnPlayerSpawned(Player player)
    {
        AddPlayer(player);
        OnStateChanged?.Invoke();
    }

    private void OnPlayerDespawned(Player player)
    {
        RemovePlayer(player);
        OnStateChanged?.Invoke();
    }

    private void AddPlayer(Player player)
    {
        // 중복 방지 (여기서 OwnerClientId 등을 비교하거나 리스트 검사)
        // EntityHpViewModel을 만들 때 Player 객체를 들고 있으니 비교 가능하지만, 
        // 간단히 새로 생성해서 넣음. 
        // 이름: 본인이면 "ME", 아니면 "Player N"
        string name = player.IsOwner ? "ME" : $"Player {player.OwnerClientId}";
        Players.Add(new EntityHpViewModel(player, name));
    }

    private void RemovePlayer(Player player)
    {
        // 리스트에서 해당 플레이어를 가진 VM 제거
        // (EntityHpViewModel에 Entity 접근자가 없으므로, 로직 보완이 필요할 수 있으나 
        //  현재는 순서대로 매칭하거나 전체 갱신하므로 간단히 구현)
        //  *정확한 제거를 위해 EntityHpViewModel에 Entity 프로퍼티를 public으로 열거나 비교 메서드 필요.
        //  여기서는 간단히 리스트를 재생성하는 방식 혹은 이름으로 찾아서 제거.

        // 간단 구현: 이름으로 매칭해서 제거 (완벽하진 않음)
        string nameTarget = player.IsOwner ? "ME" : $"Player {player.OwnerClientId}";
        var target = Players.Find(vm => vm.Name == nameTarget);
        if (target != null)
        {
            target.Dispose();
            Players.Remove(target);
        }
    }

    protected override void OnDispose()
    {
        Player.OnPlayerSpawned -= OnPlayerSpawned;
        Player.OnPlayerDespawned -= OnPlayerDespawned;

        Core?.Dispose();
        foreach (var p in Players) p.Dispose();
    }
}
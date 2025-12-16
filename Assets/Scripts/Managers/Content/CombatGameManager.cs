using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum eGameState
{
    Waiting,            // ��� �� (�ο� ����)
    WaveInProgress,     // ���� ���̺� ���� ��
    RewardSelection,    // ���̺� Ŭ���� �� ���� ����
    Victory,            // ��� ���̺� Ŭ���� (�¸�)
    Defeat              // ���� ���� (�й�)
}

public class CombatGameManager : NetworkBehaviour
{
    public static CombatGameManager Instance { get; private set; }

    [Header("Settings")]
    public int MaxPlayers = 2; // ���� ���ۿ� �ʿ��� �÷��̾� ��

    // ���� ���� �� ���̺� ���� ����ȭ
    public NetworkVariable<eGameState> GameState = new NetworkVariable<eGameState>(eGameState.Waiting);
    public NetworkVariable<int> CurrentWave = new NetworkVariable<int>(1);

    // ���� ���� ����ġ (�⺻�� 1.0f, �������� ����)
    public NetworkVariable<float> EnemyAtkMultiplier = new NetworkVariable<float>(1.0f);
    public NetworkVariable<float> EnemySpeedMultiplier = new NetworkVariable<float>(1.0f);
    public NetworkVariable<float> EnemyHpMultiplier = new NetworkVariable<float>(1.0f);

    // ���� ���� ����
    private EnemySpawner _spawner;
    private HashSet<ulong> _selectedRewardClients = new HashSet<ulong>();

    // ����� �÷��̾� ��� (ID)
    private HashSet<ulong> _deadPlayers = new HashSet<ulong>();

    // [UI Reference] ����â HUD
    private UI_GameStatusHUD _statusHUD;
    private UI_StatHUD _statHUD;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameState.Value = eGameState.Waiting;
            _spawner = FindAnyObjectByType<EnemySpawner>();

            // Ŭ���̾�Ʈ ����/���� �̺�Ʈ ����
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // ���� ���� �̺�Ʈ ���� (Server/Client ���)
        GameState.OnValueChanged += OnGameStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        GameState.OnValueChanged -= OnGameStateChanged;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[CombatGameManager] �÷��̾� ���� (ID: {clientId}). ���� �ο�: {NetworkManager.Singleton.ConnectedClientsIds.Count}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // �÷��̾ ������ �� ���� ���� ��� �� ����� ��Ͽ��� ����
        if (_selectedRewardClients.Contains(clientId))
            _selectedRewardClients.Remove(clientId);

        if (_deadPlayers.Contains(clientId))
            _deadPlayers.Remove(clientId);

        Debug.Log($"[CombatGameManager] �÷��̾� ���� ���� (ID: {clientId}). ��⿭ ����.");
    }

    // ========================================================================
    // Game Flow Logic (Server Only)
    // ========================================================================

    /// <summary>
    /// ȣ��Ʈ�� HUD�� '���� ����' ��ư�� ������ �� ȣ��
    /// [����] �񵿱� ������ ���� async void�� ����
    /// </summary>
    public async void StartGame()
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClientsIds.Count < MaxPlayers)
        {
            Debug.LogWarning($"[CombatGameManager] �ο� ���� ({NetworkManager.Singleton.ConnectedClientsIds.Count}/{MaxPlayers}). ������ ���� �����մϴ�.");
        }

        // [�߰�] �ھ� ���� ���� (StartGame ������ ����)
        if (Core.Instance == null)
        {
            GameObject coreGo = await Managers.Resource.InstantiateAsync("Core");
            if (coreGo != null)
            {
                coreGo.transform.position = Vector3.zero;
                var netObj = coreGo.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                    netObj.Spawn(); // ��Ʈ��ũ ���� -> Ŭ���̾�Ʈ�鿡�� ���� ��Ŷ ����

                Debug.Log("[CombatGameManager] �ھ� ���� �� ���� �Ϸ�.");
            }
            else
            {
                Debug.LogError("[CombatGameManager] �ھ� ������ �ε� ����!");
                return; // �ھ ������ ���� ���� �Ұ�
            }
        }

        StartCoroutine(CoGameLoop());
    }

    /// <summary>
    /// ��ü ���� ���� (���̺� -> Ŭ���� -> ���� -> ���� ���̺�)
    /// </summary>
    private IEnumerator CoGameLoop()
    {
        // [�߰�] �ھ� ���� ��Ŷ�� Ŭ���̾�Ʈ�� �����ϰ� �ʱ�ȭ�� �ð��� Ȯ�� (������ġ)
        yield return new WaitForSeconds(1.0f);

        Debug.Log("[GameLoop] ���� ���� ����");

        // ���� �ʱ�ȭ
        _deadPlayers.Clear();
        CurrentWave.Value = 1;

        // ����� ���
        Managers.Sound.PlayBGM("BGM");

        while (true)
        {
            // 1. ���� ���̺� ������ ��������
            WaveGameData waveData = Managers.Data.Get<WaveGameData>(CurrentWave.Value);

            // �����Ͱ� ������ ��� ���̺긦 Ŭ������ ������ ���� -> �¸�
            if (waveData == null)
            {
                Debug.Log("[GameLoop] ��� ���̺� Ŭ����! �¸�!");
                SetGameState(eGameState.Victory);
                yield break;
            }

            // 2. ���̺� ���� ����
            Debug.Log($"[GameLoop] Wave {CurrentWave.Value} ����");
            SetGameState(eGameState.WaveInProgress);

            // �����ʿ��� ���� ���� ���� (�ڷ�ƾ ���)
            yield return StartCoroutine(CoProcessWave(waveData));

            // 3. �� ���� ���
            yield return new WaitUntil(() => _spawner.GetActiveEnemyCount() == 0);
            Debug.Log($"[GameLoop] Wave {CurrentWave.Value} �� ���� Ȯ��.");

            // 4. ���� ���� �ܰ�
            Debug.Log("[GameLoop] ���� ���� �ܰ� ����");

            _selectedRewardClients.Clear(); // [����] ���� ��� �ʱ�ȭ
            SetGameState(eGameState.RewardSelection);

            // ���� ���� ��� �÷��̾ �����ߴ��� Ȯ�� (Deadlock ���� ���� �����)
            yield return new WaitUntil(CheckAllRewardsSelected);

            Debug.Log("[GameLoop] ��� �÷��̾� ���� ���� �Ϸ�.");

            // 5. ���� ���̺� �غ�
            CurrentWave.Value++;
            yield return new WaitForSeconds(3f); // 3�� ���� �ð�
        }
    }

    /// <summary>
    /// ���� ������ ���� ���� ������ ���� ���Ͽ� ���� ���� ����
    /// </summary>
    private bool CheckAllRewardsSelected()
    {
        // ���� ���� ��� Ŭ���̾�Ʈ ��ȸ
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // ����� �÷��̾�� ���� ���� ��󿡼� ����
            if (_deadPlayers.Contains(client.ClientId))
                continue;

            // ������ �ִµ� ���� �������� �ʾҴٸ� false
            if (!_selectedRewardClients.Contains(client.ClientId))
                return false;
        }

        return true;
    }

    /// <summary>
    /// ���̺� �����͸� ������� �ð� �帧�� ���� �� ����
    /// </summary>
    private IEnumerator CoProcessWave(WaveGameData data)
    {
        float timer = 0f;
        var events = new Queue<SpawnEventData>(data.events);

        while (events.Count > 0)
        {
            timer += Time.deltaTime;

            while (events.Count > 0 && events.Peek().time <= timer)
            {
                var evt = events.Dequeue();
                if (_spawner != null)
                {
                    _spawner.SpawnEnemy(evt.spawnerIdx, evt.monsterId);
                }
            }

            yield return null;
        }
    }

    private void SetGameState(eGameState newState)
    {
        GameState.Value = newState;
    }

    // ========================================================================
    // Defeat & Death Logic (Server Only)
    // ========================================================================

    /// <summary>
    /// �÷��̾� ��� �� ȣ�� (Player.cs���� ȣ��)
    /// </summary>
    public void OnPlayerDied(ulong clientId)
    {
        if (!IsServer) return;

        if (!_deadPlayers.Contains(clientId))
        {
            _deadPlayers.Add(clientId);
            Debug.Log($"[CombatGameManager] Player {clientId} ���. (���� �����: {_deadPlayers.Count}/{NetworkManager.Singleton.ConnectedClientsIds.Count})");

            // ��� �÷��̾ ����ߴ��� Ȯ��
            if (_deadPlayers.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Debug.Log("[CombatGameManager] ���� ���. �й� ó��.");
                TriggerDefeat();
            }
        }
    }

    /// <summary>
    /// �й� ���� ���� �� ȣ�� (Core �ı� or ���� ���)
    /// </summary>
    public void TriggerDefeat()
    {
        if (IsServer)
        {
            // ���� ���� ����
            StopAllCoroutines();

            // �� ��� ����
            if (_spawner != null)
                _spawner.ClearAllEnemies();

            // ���� ���� -> Defeat
            SetGameState(eGameState.Defeat);
        }
    }

    // ========================================================================
    // Reward Logic (Server -> Client)
    // ========================================================================

    /// <summary>
    /// Ŭ���̾�Ʈ�� ������ �������� �� ȣ�� (ServerRpc)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SelectRewardServerRpc(int rewardId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (_deadPlayers.Contains(clientId)) return;

        // �ߺ� ���� ����
        if (_selectedRewardClients.Contains(clientId))
        {
            Debug.LogWarning($"[CombatGameManager] Client({clientId})�� �̹� ������ �����߽��ϴ�.");
            return;
        }

        _selectedRewardClients.Add(clientId);

        Debug.Log($"[CombatGameManager] Client({clientId}) ���� ����: {rewardId}. (�Ϸ�: {_selectedRewardClients.Count}/{NetworkManager.Singleton.ConnectedClientsIds.Count})");

        // ���� ���� ȿ�� ����
        ApplyRewardEffect(rewardId, clientId);
    }

    /// <summary>
    /// ���� �����Ϳ� ���� ȿ���� �����ϰ� Ŭ���̾�Ʈ�� ����ȭ
    /// </summary>
    private void ApplyRewardEffect(int rewardId, ulong targetClientId)
    {
        RewardGameData reward = Managers.Data.Get<RewardGameData>(rewardId);
        if (reward == null)
        {
            Debug.LogError($"[CombatGameManager] ���� ������(ID:{rewardId})�� ã�� �� �����ϴ�.");
            return;
        }

        // ���� ����� Ÿ�� ó��
        if (reward.targetType == "Team")
        {
            switch (reward.effectType)
            {
                case "EnemyAtkDown":
                    EnemyAtkMultiplier.Value = Mathf.Max(0.1f, EnemyAtkMultiplier.Value - reward.value);
                    Debug.Log($"[Reward] �� ���ݷ� ���� ����. ���� ����: {EnemyAtkMultiplier.Value}");
                    return; // ó�� �Ϸ�

                case "EnemySpeedDown":
                    EnemySpeedMultiplier.Value = Mathf.Max(0.1f, EnemySpeedMultiplier.Value - reward.value);
                    Debug.Log($"[Reward] �� �̵��ӵ� ���� ����. ���� ����: {EnemySpeedMultiplier.Value}");
                    return;

                case "EnemyHpDown":
                    EnemyHpMultiplier.Value = Mathf.Max(0.1f, EnemyHpMultiplier.Value - reward.value);
                    Debug.Log($"[Reward] �� �ִ�ü�� ���� ����. ���� ����: {EnemyHpMultiplier.Value}");
                    return;
            }
        }

        // 1. ȿ�� ���� ��� ã�� (�÷��̾� �� �ھ�)
        List<Entity> targets = new List<Entity>();

        if (reward.targetType == "Team")
        {
            if (reward.effectType == "CoreHeal")
            {
                // �ھ� ȸ��
                if (Core.Instance != null) targets.Add(Core.Instance);
            }
            else
            {
                // �� ��ü (��� �÷��̾�)
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    if (client.PlayerObject != null)
                        targets.Add(client.PlayerObject.GetComponent<Player>());
                }
            }
        }
        else // Individual
        {
            // ������ ������ ���� �÷��̾�
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var client))
            {
                if (client.PlayerObject != null)
                    targets.Add(client.PlayerObject.GetComponent<Player>());
            }
        }

        // 2. ȿ�� ���� �� ����ȭ
        foreach (var target in targets)
        {
            if (target == null) continue;

            // ���� �� ������ ����
            ApplyStatChange(target, reward);
        }
    }

    /// <summary>
    /// ���� ���� ��� ����
    /// </summary>
    private void ApplyStatChange(Entity target, RewardGameData reward)
    {
        // ���� �����ϸ� NetworkVariable�� �ڵ����� �����մϴ�.
        switch (reward.effectType)
        {
            case "MaxHp":
                target.IncreaseMaxHp((int)reward.value);
                break;
            case "Atk":
                target.IncreaseAttackPower((int)reward.value);
                break;
            case "AtkSpeed":
                target.IncreaseAttackSpeed(reward.value);
                break;
            case "MoveSpeed":
                target.IncreaseMoveSpeed(reward.value);
                break;
            case "Heal":
            case "CoreHeal":
                int healAmount = (reward.effectType == "CoreHeal") ? (int)reward.value : Mathf.FloorToInt(target.MaxHp * (reward.value / 100f));
                if (healAmount < 1) healAmount = 1;
                target.Heal(healAmount);
                break;
        }
    }


    // ========================================================================
    // State Change Handler (UI Trigger & Sound)
    // ========================================================================

    private async void OnGameStateChanged(eGameState previous, eGameState current)
    {
        Debug.Log($"[CombatGameManager] GameState ����: {previous} -> {current}");

        // ���� ���� �� �κ� ��� (���� ����) - ȣ��Ʈ�� ����
        if (IsServer && current == eGameState.WaveInProgress)
        {
            _ = Managers.Network.SetLobbyLockStateAsync(true);
        }

        switch (current)
        {
            case eGameState.WaveInProgress:
                {
                    // [UI] ���� ���� �� ����â(HUD) �� ���� HUD ǥ�� (Waiting���� �Ѿ���� ��)
                    if (previous == eGameState.Waiting)
                    {
                        if (_statusHUD == null)
                        {
                            _statusHUD = await Managers.UI.ShowAsync<UI_GameStatusHUD>(new GameStatusViewModel());
                        }

                        // [�߰�] ���� HUD ����
                        if (_statHUD == null)
                        {
                            _statHUD = await Managers.UI.ShowAsync<UI_StatHUD>(new StatHUDViewModel());
                        }
                    }
                    break;
                }
            case eGameState.RewardSelection:
                {
                    // [Sound] ������/���� ȿ���� (BGM ����)
                    Managers.Sound.PlaySFX("LevelUp");

                    bool isLocalPlayerDead = false;
                    if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
                    {
                        var player = NetworkManager.Singleton.LocalClient.PlayerObject?.GetComponent<Player>();
                        if (player != null && player.Hp <= 0)
                            isLocalPlayerDead = true;
                    }

                    if (!isLocalPlayerDead)
                    {
                        await Managers.UI.ShowAsync<UI_RewardPopup>(new RewardPopupViewModel());
                    }
                    else
                    {
                        Debug.Log("[Reward] ��� �����̹Ƿ� ���� ������ �ǳʶݴϴ�.");
                    }
                    break;
                }
            case eGameState.Victory:
                {
                    // [Sound] �¸� ȿ���� 
                    Managers.Sound.StopBGM();
                    Managers.Sound.PlaySFX("Win");

                    // [UI] ����â ����
                    if (_statusHUD != null)
                    {
                        Managers.UI.Close(_statusHUD);
                        _statusHUD = null;
                    }
                    if (_statHUD != null)
                    {
                        Managers.UI.Close(_statHUD);
                        _statHUD = null;
                    }

                    var vm = new CombatResultViewModel();
                    vm.SetResult(eCombatResult.Victory);
                    await Managers.UI.ShowAsync<UI_PopupCombatResult>(vm);
                    break;
                }
            case eGameState.Defeat:
                {
                    // [Sound] �й� ȿ���� (BGM ����)
                    Managers.Sound.StopBGM();
                    Managers.Sound.PlaySFX("Lose");

                    // [UI] ����â ����
                    if (_statusHUD != null)
                    {
                        Managers.UI.Close(_statusHUD);
                        _statusHUD = null;
                    }
                    if (_statHUD != null)
                    {
                        Managers.UI.Close(_statHUD);
                        _statHUD = null;
                    }

                    var vm = new CombatResultViewModel();
                    vm.SetResult(eCombatResult.Defeat);
                    await Managers.UI.ShowAsync<UI_PopupCombatResult>(vm);
                    break;
                }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum eGameState
{
    Waiting,            // 대기 중 (인원 모집)
    WaveInProgress,     // 전투 웨이브 진행 중
    RewardSelection,    // 웨이브 클리어 후 보상 선택
    Victory,            // 모든 웨이브 클리어 (승리)
    Defeat              // 게임 오버 (패배)
}

public class CombatGameManager : NetworkBehaviour
{
    public static CombatGameManager Instance { get; private set; }

    [Header("Settings")]
    public int MaxPlayers = 2; // 게임 시작에 필요한 플레이어 수

    // 게임 상태 및 웨이브 정보 동기화
    public NetworkVariable<eGameState> GameState = new NetworkVariable<eGameState>(eGameState.Waiting);
    public NetworkVariable<int> CurrentWave = new NetworkVariable<int>(1);

    // 적군 스탯 보정치 (기본값 1.0f, 보상으로 감소)
    public NetworkVariable<float> EnemyAtkMultiplier = new NetworkVariable<float>(1.0f);
    public NetworkVariable<float> EnemySpeedMultiplier = new NetworkVariable<float>(1.0f);
    public NetworkVariable<float> EnemyHpMultiplier = new NetworkVariable<float>(1.0f);

    // 내부 관리 변수
    private EnemySpawner _spawner;
    private HashSet<ulong> _selectedRewardClients = new HashSet<ulong>();

    // 사망한 플레이어 목록 (ID)
    private HashSet<ulong> _deadPlayers = new HashSet<ulong>();

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

            // 클라이언트 접속/해제 이벤트 연결
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // 상태 변경 이벤트 연결 (Server/Client 모두)
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
        Debug.Log($"[CombatGameManager] 플레이어 접속 (ID: {clientId}). 현재 인원: {NetworkManager.Singleton.ConnectedClientsIds.Count}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // 플레이어가 나갔을 때 보상 선택 목록 및 사망자 목록에서 제거
        if (_selectedRewardClients.Contains(clientId))
            _selectedRewardClients.Remove(clientId);

        if (_deadPlayers.Contains(clientId))
            _deadPlayers.Remove(clientId);

        Debug.Log($"[CombatGameManager] 플레이어 연결 종료 (ID: {clientId}). 대기열 갱신.");
    }

    // ========================================================================
    // Game Flow Logic (Server Only)
    // ========================================================================

    /// <summary>
    /// 호스트가 HUD의 '게임 시작' 버튼을 눌렀을 때 호출
    /// </summary>
    public void StartGame()
    {
        if (!IsServer) return;

        // 테스트 편의를 위해 인원이 부족해도 시작은 가능하게 하되 경고 출력
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < MaxPlayers)
        {
            Debug.LogWarning($"[CombatGameManager] 인원 부족 ({NetworkManager.Singleton.ConnectedClientsIds.Count}/{MaxPlayers}). 게임을 강제 시작합니다.");
        }

        StartCoroutine(CoGameLoop());
    }

    /// <summary>
    /// 전체 게임 루프 (웨이브 -> 클리어 -> 보상 -> 다음 웨이브)
    /// </summary>
    private IEnumerator CoGameLoop()
    {
        Debug.Log("[GameLoop] 게임 루프 시작");

        // 상태 초기화
        _deadPlayers.Clear();
        CurrentWave.Value = 1;

        while (true)
        {
            // 1. 현재 웨이브 데이터 가져오기
            WaveGameData waveData = Managers.Data.Get<WaveGameData>(CurrentWave.Value);

            // 데이터가 없으면 모든 웨이브를 클리어한 것으로 간주 -> 승리
            if (waveData == null)
            {
                Debug.Log("[GameLoop] 모든 웨이브 클리어! 승리!");
                SetGameState(eGameState.Victory);
                yield break;
            }

            // 2. 웨이브 전투 시작
            Debug.Log($"[GameLoop] Wave {CurrentWave.Value} 시작");
            SetGameState(eGameState.WaveInProgress);

            // 스포너에게 스폰 명령 전달 (코루틴 대기)
            yield return StartCoroutine(CoProcessWave(waveData));

            // 3. 적 전멸 대기
            yield return new WaitUntil(() => _spawner.GetActiveEnemyCount() == 0);
            Debug.Log($"[GameLoop] Wave {CurrentWave.Value} 적 전멸 확인.");

            // 4. 보상 선택 단계
            Debug.Log("[GameLoop] 보상 선택 단계 진입");

            _selectedRewardClients.Clear(); // [수정] 선택 목록 초기화
            SetGameState(eGameState.RewardSelection);

            // 접속 중인 모든 플레이어가 선택했는지 확인 (Deadlock 방지 로직 적용됨)
            yield return new WaitUntil(CheckAllRewardsSelected);

            Debug.Log("[GameLoop] 모든 플레이어 보상 선택 완료.");

            // 5. 다음 웨이브 준비
            CurrentWave.Value++;
            yield return new WaitForSeconds(3f); // 3초 정비 시간
        }
    }

    /// <summary>
    /// 현재 접속자 수와 보상 선택자 수를 비교하여 진행 여부 결정
    /// </summary>
    private bool CheckAllRewardsSelected()
    {
        int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        int selectedCount = _selectedRewardClients.Count;

        if (connectedCount == 0) return true; // 아무도 없으면 진행 (혹은 종료)

        return selectedCount >= connectedCount;
    }

    /// <summary>
    /// 웨이브 데이터를 기반으로 시간 흐름에 따라 적 스폰
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
    /// 플레이어 사망 시 호출 (Player.cs에서 호출)
    /// </summary>
    public void OnPlayerDied(ulong clientId)
    {
        if (!IsServer) return;

        if (!_deadPlayers.Contains(clientId))
        {
            _deadPlayers.Add(clientId);
            Debug.Log($"[CombatGameManager] Player {clientId} 사망. (현재 사망자: {_deadPlayers.Count}/{NetworkManager.Singleton.ConnectedClientsIds.Count})");

            // 모든 플레이어가 사망했는지 확인
            if (_deadPlayers.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count)
            {
                Debug.Log("[CombatGameManager] 전원 사망. 패배 처리.");
                TriggerDefeat();
            }
        }
    }

    /// <summary>
    /// 패배 조건 충족 시 호출 (Core 파괴 or 전원 사망)
    /// </summary>
    public void TriggerDefeat()
    {
        if (IsServer)
        {
            // 게임 루프 정지
            StopAllCoroutines();

            // 적 모두 제거
            if (_spawner != null)
                _spawner.ClearAllEnemies();

            // 상태 변경 -> Defeat
            SetGameState(eGameState.Defeat);
        }
    }

    // ========================================================================
    // Reward Logic (Server -> Client)
    // ========================================================================

    /// <summary>
    /// 클라이언트가 보상을 선택했을 때 호출 (ServerRpc)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SelectRewardServerRpc(int rewardId, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // 중복 선택 방지
        if (_selectedRewardClients.Contains(clientId))
        {
            Debug.LogWarning($"[CombatGameManager] Client({clientId})는 이미 보상을 선택했습니다.");
            return;
        }

        _selectedRewardClients.Add(clientId);

        Debug.Log($"[CombatGameManager] Client({clientId}) 보상 선택: {rewardId}. (완료: {_selectedRewardClients.Count}/{NetworkManager.Singleton.ConnectedClientsIds.Count})");

        // 실제 보상 효과 적용
        ApplyRewardEffect(rewardId, clientId);
    }

    /// <summary>
    /// 보상 데이터에 따라 효과를 적용하고 클라이언트에 동기화
    /// </summary>
    private void ApplyRewardEffect(int rewardId, ulong targetClientId)
    {
        RewardGameData reward = Managers.Data.Get<RewardGameData>(rewardId);
        if (reward == null)
        {
            Debug.LogError($"[CombatGameManager] 보상 데이터(ID:{rewardId})를 찾을 수 없습니다.");
            return;
        }

        // 적군 디버프 타입 처리
        if (reward.targetType == "Team")
        {
            switch (reward.effectType)
            {
                case "EnemyAtkDown":
                    EnemyAtkMultiplier.Value = Mathf.Max(0.1f, EnemyAtkMultiplier.Value - reward.value);
                    Debug.Log($"[Reward] 적 공격력 감소 적용. 현재 비율: {EnemyAtkMultiplier.Value}");
                    return; // 처리 완료

                case "EnemySpeedDown":
                    EnemySpeedMultiplier.Value = Mathf.Max(0.1f, EnemySpeedMultiplier.Value - reward.value);
                    Debug.Log($"[Reward] 적 이동속도 감소 적용. 현재 비율: {EnemySpeedMultiplier.Value}");
                    return;

                case "EnemyHpDown":
                    EnemyHpMultiplier.Value = Mathf.Max(0.1f, EnemyHpMultiplier.Value - reward.value);
                    Debug.Log($"[Reward] 적 최대체력 감소 적용. 현재 비율: {EnemyHpMultiplier.Value}");
                    return;
            }
        }

        // 1. 효과 적용 대상 찾기 (플레이어 및 코어)
        List<Entity> targets = new List<Entity>();

        if (reward.targetType == "Team")
        {
            if (reward.effectType == "CoreHeal")
            {
                // 코어 회복
                if (Core.Instance != null) targets.Add(Core.Instance);
            }
            else
            {
                // 팀 전체 (모든 플레이어)
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    if (client.PlayerObject != null)
                        targets.Add(client.PlayerObject.GetComponent<Player>());
                }
            }
        }
        else // Individual
        {
            // 보상을 선택한 개인 플레이어
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var client))
            {
                if (client.PlayerObject != null)
                    targets.Add(client.PlayerObject.GetComponent<Player>());
            }
        }

        // 2. 효과 적용 및 동기화
        foreach (var target in targets)
        {
            if (target == null) continue;

            // 서버 측 데이터 변경
            ApplyStatChange(target, reward);
        }
    }

    /// <summary>
    /// 실제 스탯 계산 로직
    /// </summary>
    private void ApplyStatChange(Entity target, RewardGameData reward)
    {
        // 값만 변경하면 NetworkVariable이 자동으로 전파합니다.
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
    // State Change Handler (UI Trigger)
    // ========================================================================

    private async void OnGameStateChanged(eGameState previous, eGameState current)
    {
        Debug.Log($"[CombatGameManager] GameState 변경: {previous} -> {current}");

        // 상태 변경에 따른 UI 자동 팝업 처리
        if (current == eGameState.RewardSelection)
        {
            // 보상 선택 팝업 표시 (로컬)
            await Managers.UI.ShowAsync<UI_RewardPopup>(new RewardPopupViewModel());
        }
        else if (current == eGameState.Victory)
        {
            var vm = new CombatResultViewModel();
            vm.SetResult(eCombatResult.Victory);
            await Managers.UI.ShowAsync<UI_PopupCombatResult>(vm);
        }
        else if (current == eGameState.Defeat)
        {
            var vm = new CombatResultViewModel();
            vm.SetResult(eCombatResult.Defeat);
            await Managers.UI.ShowAsync<UI_PopupCombatResult>(vm);
        }
    }
}
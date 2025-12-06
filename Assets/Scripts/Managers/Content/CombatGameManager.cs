using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum eGameState
{
    Waiting,
    WaveInProgress,
    RewardSelection,
    Victory,
    Defeat
}

public class CombatGameManager : NetworkBehaviour
{
    public static CombatGameManager Instance { get; private set; }

    [Header("Settings")]
    public int MaxPlayers = 2;

    // 게임 상태 동기화
    public NetworkVariable<eGameState> GameState = new NetworkVariable<eGameState>(eGameState.Waiting);
    public NetworkVariable<int> CurrentWave = new NetworkVariable<int>(1);

    private EnemySpawner _spawner;
    private int _rewardSelectionCount = 0;

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

            // 플레이어 접속 이벤트 구독
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        // 상태 변경 구독 (UI 갱신 등)
        GameState.OnValueChanged += OnGameStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        GameState.OnValueChanged -= OnGameStateChanged;
    }

    private void OnClientConnected(ulong clientId)
    {
        // 2명이 모이면 게임 시작 버튼 활성화 등의 로직을 UI에 전달 가능
        Debug.Log($"[CombatGameManager] 플레이어 접속. 현재 인원: {NetworkManager.Singleton.ConnectedClientsIds.Count}");
    }

    // --- Host Controls ---

    /// <summary>
    /// 호스트가 게임 시작 버튼을 누르면 호출
    /// </summary>
    public void StartGame()
    {
        if (!IsServer) return;
        // 테스트를 위해 인원 체크는 로그로만 남김
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < MaxPlayers)
            Debug.LogWarning("플레이어 수가 부족하지만 게임을 강제로 시작합니다.");

        StartCoroutine(CoGameLoop());
    }

    private IEnumerator CoGameLoop()
    {
        // 1. 게임 시작
        Debug.Log("[GameLoop] 게임 시작!");
        SetGameState(eGameState.WaveInProgress); // 바로 전투 상태로 진입 (웨이브 1)
        CurrentWave.Value = 1;

        while (true) // 웨이브 루프
        {
            // 1. 웨이브 데이터 확인
            WaveGameData waveData = Managers.Data.Get<WaveGameData>(CurrentWave.Value);
            if (waveData == null)
            {
                // 더 이상 웨이브가 없으면 승리
                SetGameState(eGameState.Victory);
                yield break;
            }

            Debug.Log($"[GameLoop] Wave {CurrentWave.Value} 진행 중...");
            SetGameState(eGameState.WaveInProgress);

            // 2. 웨이브 스폰 실행
            yield return StartCoroutine(CoProcessWave(waveData));

            // 3. 적 전멸 대기
            yield return new WaitUntil(() => _spawner.GetActiveEnemyCount() == 0);
            Debug.Log($"[GameLoop] Wave {CurrentWave.Value} 클리어!");

            // 4. 보상 선택 단계 진입
            _rewardSelectionCount = 0;
            SetGameState(eGameState.RewardSelection);

            Debug.Log("[GameLoop] 보상 선택 대기 중...");
            // 모든 접속자가 선택할 때까지 대기
            yield return new WaitUntil(() => _rewardSelectionCount >= NetworkManager.Singleton.ConnectedClientsIds.Count);

            Debug.Log("[GameLoop] 모든 플레이어 보상 선택 완료. 다음 웨이브로.");

            // 5. 다음 웨이브
            CurrentWave.Value++;
            yield return new WaitForSeconds(3f); // 잠시 대기
        }
    }

    private IEnumerator CoProcessWave(WaveGameData data)
    {
        float timer = 0f;
        var events = new Queue<SpawnEventData>(data.events);

        while (events.Count > 0)
        {
            timer += Time.deltaTime;

            // 현재 시간보다 같거나 작은 이벤트 모두 실행
            while (events.Count > 0 && events.Peek().time <= timer)
            {
                var evt = events.Dequeue();
                _spawner.SpawnEnemy(evt.spawnerIdx, evt.monsterId);
            }

            yield return null;
        }
    }

    private void SetGameState(eGameState newState)
    {
        GameState.Value = newState;
    }

    public void TriggerDefeat()
    {
        if (IsServer)
        {
            StopAllCoroutines();
            _spawner.ClearAllEnemies();
            SetGameState(eGameState.Defeat);
        }
    }

    // --- Client RPCs ---

    /// <summary>
    /// 클라이언트가 보상을 선택했을 때 서버에 알림
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SelectRewardServerRpc(int rewardId, ServerRpcParams rpcParams = default)
    {
        _rewardSelectionCount++;
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[CombatGameManager] Client({clientId}) 보상 선택 완료 (ID: {rewardId}). 진행: {_rewardSelectionCount}/{NetworkManager.Singleton.ConnectedClientsIds.Count}");

        // TODO: 여기서 실제 스탯(RewardGameData)을 UserDataModel이나 Player 객체에 적용
        // RewardGameData reward = Managers.Data.Get<RewardGameData>(rewardId);
        // if (reward != null) { ... }
    }

    // --- State Change Handler ---

    private void OnGameStateChanged(eGameState previous, eGameState current)
    {
        Debug.Log($"[CombatGameManager] 상태 변경: {previous} -> {current}");

        // 상태 변경 시 UI 팝업 자동 호출
        if (current == eGameState.RewardSelection)
        {
            // Managers.UI.ShowAsync<UI_RewardPopup>(new RewardPopupViewModel());
        }
    }
}
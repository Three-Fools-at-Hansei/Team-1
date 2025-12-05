using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

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
    private int _rewardSelectionCount = 0; // 보상 선택한 플레이어 수

    private void Awake()
    {
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

        // TODO: 2명 체크 로직 (테스트를 위해 주석 처리 가능)
        // if (NetworkManager.Singleton.ConnectedClientsIds.Count < MaxPlayers) return;

        StartCoroutine(CoGameLoop());
    }

    private IEnumerator CoGameLoop()
    {
        // 1. 게임 시작
        Debug.Log("[GameLoop] 게임 시작!");
        CurrentWave.Value = 1;

        while (true) // 웨이브 루프
        {
            // 2. 웨이브 데이터 로드
            WaveGameData waveData = Managers.Data.Get<WaveGameData>(CurrentWave.Value);
            if (waveData == null)
            {
                // 더 이상 웨이브가 없으면 승리
                SetGameState(eGameState.Victory);
                yield break;
            }

            // 3. 웨이브 진행 (전투)
            SetGameState(eGameState.WaveInProgress);
            yield return StartCoroutine(CoProcessWave(waveData));

            // 4. 웨이브 클리어 대기 (모든 적 처치)
            yield return new WaitUntil(() => _spawner.GetActiveEnemyCount() == 0);
            Debug.Log($"[GameLoop] 웨이브 {CurrentWave.Value} 클리어!");

            // 5. 보상 선택 단계
            SetGameState(eGameState.RewardSelection);
            _rewardSelectionCount = 0;

            // 모든 플레이어가 선택할 때까지 대기
            // (실제로는 타임아웃 등 안전장치 필요)
            yield return new WaitUntil(() => _rewardSelectionCount >= NetworkManager.Singleton.ConnectedClientsIds.Count);

            Debug.Log("[GameLoop] 모든 플레이어 보상 선택 완료. 다음 웨이브로.");

            // 6. 다음 웨이브 준비
            CurrentWave.Value++;
            yield return new WaitForSeconds(3f); // 잠시 대기
        }
    }

    private IEnumerator CoProcessWave(WaveGameData data)
    {
        float timer = 0f;

        // 시간순 정렬 (혹시 모르니)
        var events = new Queue<SpawnEventData>(data.events); // 이미 시간순이라 가정

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

    // --- Client RPCs ---

    /// <summary>
    /// 클라이언트가 보상을 선택했을 때 서버에 알림
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SelectRewardServerRpc(int rewardId)
    {
        _rewardSelectionCount++;
        Debug.Log($"[CombatGameManager] 플레이어 보상 선택 확인. ({_rewardSelectionCount}명 완료)");

        // 실제 보상 적용 로직 (UserData 갱신)은 여기서 처리
    }

    // --- State Change Handler ---

    private void OnGameStateChanged(eGameState previous, eGameState current)
    {
        Debug.Log($"[CombatGameManager] 상태 변경: {previous} -> {current}");

        // 상태에 따라 UI 팝업 등을 띄우는 로직은 여기서 Event로 전파하거나
        // 각 UI ViewModel에서 GameState를 구독하여 처리

        if (current == eGameState.RewardSelection)
        {
            // 보상 팝업 오픈 (로컬)
            // Managers.UI.ShowAsync<UI_RewardPopup>();
        }
        else if (current == eGameState.Victory)
        {
            Debug.Log("VICTORY POPUP OPEN");
            // Managers.UI.ShowAsync<UI_VictoryPopup>();
        }
        else if (current == eGameState.Defeat)
        {
            Debug.Log("DEFEAT POPUP OPEN");
            // Managers.UI.ShowAsync<UI_DefeatPopup>();
        }
    }

    public void TriggerDefeat()
    {
        if (IsServer)
        {
            StopAllCoroutines();
            SetGameState(eGameState.Defeat);
            _spawner.ClearAllEnemies();
        }
    }
}
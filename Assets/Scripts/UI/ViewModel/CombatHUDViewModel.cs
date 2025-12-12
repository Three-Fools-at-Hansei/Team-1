using System;
using System.Threading.Tasks; // Task 사용을 위해 추가
using UI;
using Unity.Netcode;
using UnityEngine;

public class CombatHUDViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    public string WaveText { get; private set; } = "Wave -";
    public string StatusText { get; private set; } = "대기 중...";
    public string RoomCodeText { get; private set; } = string.Empty;

    public bool IsStartButtonVisible { get; private set; } = false;
    public bool IsLobbyButtonVisible { get; private set; } = false; // 결과 화면용 (기존)
    public bool IsReturnButtonVisible { get; private set; } = false; // [New] 대기실 퇴장용

    public CombatHUDViewModel()
    {
        if (CombatGameManager.Instance != null)
        {
            CombatGameManager.Instance.GameState.OnValueChanged += OnGameStateChanged;
            CombatGameManager.Instance.CurrentWave.OnValueChanged += OnWaveChanged;

            // 초기 상태 반영
            UpdateState(CombatGameManager.Instance.GameState.Value);
            UpdateWave(CombatGameManager.Instance.CurrentWave.Value);
        }

        // [New] 네트워크 연결 끊김(호스트 종료 등) 감지
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        string code = Managers.Network.CurrentLobbyCode;
        if (!string.IsNullOrEmpty(code))
        {
            RoomCodeText = $"CODE: {code}";
        }
        else
        {
            RoomCodeText = "";
        }

        Player.OnLocalPlayerDeadStateChanged += OnLocalPlayerDead;
    }

    private void OnGameStateChanged(eGameState prev, eGameState curr) => UpdateState(curr);
    private void OnWaveChanged(int prev, int curr) => UpdateWave(curr);

    private void OnLocalPlayerDead(bool isDead)
    {
        if (isDead)
        {
            StatusText = "사망했습니다.";
            OnStateChanged?.Invoke();
        }
    }

    private void UpdateState(eGameState state)
    {
        // 호스트이면서 대기 상태일 때만 시작 버튼 활성화
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        IsStartButtonVisible = (state == eGameState.Waiting) && isHost;

        // 게임 종료 상태일 때 결과창 로비 버튼 활성화 (기존 유지)
        IsLobbyButtonVisible = (state == eGameState.Victory || state == eGameState.Defeat);

        // [New] 대기 상태(Waiting)일 때만 '돌아가기' 버튼 활성화
        IsReturnButtonVisible = (state == eGameState.Waiting);

        switch (state)
        {
            case eGameState.Waiting: StatusText = "참가자 대기 중..."; break;
            case eGameState.WaveInProgress: StatusText = "전투 중!"; break;
            case eGameState.RewardSelection: StatusText = "보상 선택 중..."; break;
            case eGameState.Victory: break;
            case eGameState.Defeat: break;
        }

        OnStateChanged?.Invoke();
    }

    private void UpdateWave(int wave)
    {
        WaveText = $"Wave {wave}";
        OnStateChanged?.Invoke();
    }

    public void OnClickStartGame()
    {
        if (CombatGameManager.Instance != null)
            CombatGameManager.Instance.StartGame();
    }

    /// <summary>
    /// [New] 대기실에서 로비로 돌아가기 버튼 클릭 시
    /// </summary>
    public void OnClickReturnToLobby()
    {
        _ = GoToLobbyAsync();
    }

    /// <summary>
    /// [New] 클라이언트 연결 종료 시 호출 (호스트가 방을 나갔을 때 등)
    /// </summary>
    private void OnClientDisconnected(ulong clientId)
    {
        // 내 클라이언트 ID가 연결 해제된 경우 (서버 셧다운 포함)
        if (NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[CombatHUDViewModel] 연결이 종료되었습니다. 로비로 이동합니다.");
            _ = GoToLobbyAsync();
        }
    }

    /// <summary>
    /// [New] 로비 이동 로직 (CombatResultPopup과 동일한 로직)
    /// </summary>
    private async Task GoToLobbyAsync()
    {
        // 이미 메인 씬으로 이동 중이거나 이동했다면 무시
        if (Managers.Scene.CurrentScene.SceneType == eSceneType.MainScene) return;

        // 네트워크 종료 및 데이터 정리
        Managers.Network.Clear();

        // 메인(로비) 씬으로 이동
        await Managers.Scene.LoadSceneAsync(eSceneType.MainScene);
    }

    // (기존 메서드 유지 - 필요 시 사용)
    public async void OnClickGoLobby()
    {
        await GoToLobbyAsync();
    }

    protected override void OnDispose()
    {
        if (CombatGameManager.Instance != null)
        {
            CombatGameManager.Instance.GameState.OnValueChanged -= OnGameStateChanged;
            CombatGameManager.Instance.CurrentWave.OnValueChanged -= OnWaveChanged;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        Player.OnLocalPlayerDeadStateChanged -= OnLocalPlayerDead;
    }
}
using System;
using UI;
using Unity.Netcode;
using UnityEngine;

public class CombatHUDViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    public string WaveText { get; private set; } = "Wave -";
    public string StatusText { get; private set; } = "대기 중...";
    public bool IsStartButtonVisible { get; private set; } = false;
    public bool IsLobbyButtonVisible { get; private set; } = false;

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

        // [추가] 로컬 플레이어 사망 이벤트 구독
        Player.OnLocalPlayerDeadStateChanged += OnLocalPlayerDead;
    }

    private void OnGameStateChanged(eGameState prev, eGameState curr) => UpdateState(curr);
    private void OnWaveChanged(int prev, int curr) => UpdateWave(curr);

    // [추가] 사망 상태 변경 핸들러
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

        // 게임 종료 상태일 때 로비 버튼 활성화
        IsLobbyButtonVisible = (state == eGameState.Victory || state == eGameState.Defeat);

        switch (state)
        {
            case eGameState.Waiting: StatusText = "참가자 대기 중..."; break;
            case eGameState.WaveInProgress: StatusText = "전투 중!"; break;
            case eGameState.RewardSelection: StatusText = "보상 선택 중..."; break;
            // [수정] 승리/패배 시에는 HUD 텍스트를 업데이트하지 않음 (요구사항 1)
            case eGameState.Victory:
                // StatusText = "승리!"; 
                break;
            case eGameState.Defeat:
                // StatusText = "패배..."; 
                break;
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

    public async void OnClickGoLobby()
    {
        // 네트워크 종료 및 로비 씬 이동
        Managers.Network.Clear(); // Shutdown
        await Managers.Scene.LoadSceneAsync(eSceneType.MainScene);
    }

    protected override void OnDispose()
    {
        if (CombatGameManager.Instance != null)
        {
            CombatGameManager.Instance.GameState.OnValueChanged -= OnGameStateChanged;
            CombatGameManager.Instance.CurrentWave.OnValueChanged -= OnWaveChanged;
        }

        Player.OnLocalPlayerDeadStateChanged -= OnLocalPlayerDead;
    }
}
using System;
using UI;
using Unity.Netcode;
using UnityEngine;

public class StatHUDViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    private Player _localPlayer;

    // View에서 바인딩할 텍스트 속성
    public string AtkText => _localPlayer != null ? $"{_localPlayer.AttackPower}" : "-";
    public string AtkSpeedText => _localPlayer != null ? $"{_localPlayer.AttackSpeed:F2}" : "-";
    public string MoveSpeedText => _localPlayer != null ? $"{_localPlayer.MoveSpeed:F1}" : "-";

    public StatHUDViewModel()
    {
        InitializeLocalPlayer();
    }

    private void InitializeLocalPlayer()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
        {
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (playerObj != null)
            {
                _localPlayer = playerObj.GetComponent<Player>();
                if (_localPlayer != null)
                {
                    SubscribeEvents();
                    OnStateChanged?.Invoke();
                }
            }
        }
    }

    private void SubscribeEvents()
    {
        if (_localPlayer == null) return;

        // 스탯 변경 감지 (NetworkVariable)
        _localPlayer.NetAttackPower.OnValueChanged += OnStatChangedInt;
        _localPlayer.NetAttackSpeed.OnValueChanged += OnStatChangedFloat;
        _localPlayer.NetMoveSpeed.OnValueChanged += OnStatChangedFloat;
    }

    // NetworkVariable 콜백은 (prev, curr) 값을 주지만, 우리는 갱신 신호만 필요함
    private void OnStatChangedInt(int prev, int curr) => OnStateChanged?.Invoke();
    private void OnStatChangedFloat(float prev, float curr) => OnStateChanged?.Invoke();

    protected override void OnDispose()
    {
        if (_localPlayer != null)
        {
            _localPlayer.NetAttackPower.OnValueChanged -= OnStatChangedInt;
            _localPlayer.NetAttackSpeed.OnValueChanged -= OnStatChangedFloat;
            _localPlayer.NetMoveSpeed.OnValueChanged -= OnStatChangedFloat;
        }
        base.OnDispose();
    }
}
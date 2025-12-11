using System;
using UI;
using UnityEngine;

/// <summary>
/// Setting 팝업의 ViewModel입니다.
/// 사운드 볼륨 조절 기능을 제공합니다.
/// </summary>
public class SettingPopupViewModel : IViewModel
{
    public event Action OnStateChanged;

    /// <summary>
    /// 사운드 볼륨 (0.0 ~ 1.0)
    /// </summary>
    public float SoundVolume
    {
        get => Managers.Sound?.Volume ?? 1f;
        set
        {
            if (Managers.Sound != null)
            {
                Debug.Log($"[ViewModel] SoundVolume Property Set: {value}");
                Managers.Sound.Volume = value;
                OnStateChanged?.Invoke();
            }
            else
            {
                Debug.LogError("[ViewModel] Managers.Sound is NULL!");
            }
        }
    }

    /// <summary>
    /// 볼륨을 퍼센트로 반환 (0 ~ 100)
    /// </summary>
    public int SoundVolumePercent => Mathf.RoundToInt(SoundVolume * 100f);

    public SettingPopupViewModel()
    {
        // 초기 볼륨 로드
        if (Managers.Sound != null)
            OnStateChanged?.Invoke();
    }

    /// <summary>
    /// 슬라이더에서 호출되는 볼륨 변경 메서드
    /// </summary>
    public void OnSoundVolumeChanged(float value)
    {
        SoundVolume = value;
    }
}
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
    /// 

    //사운드 매니저 디스카드해서 오류난거 주석
    //public float SoundVolume
    //{
    //    get => Managers.Sound?.Volume ?? 1f;
    //    set
    //    {
    //        if (Managers.Sound != null)
    //        {
    //            Managers.Sound.Volume = value;
    //            OnStateChanged?.Invoke();
    //        }
    //    }
    //}

    /// <summary>
    /// 볼륨을 퍼센트로 반환 (0 ~ 100)
    /// </summary>
    //public int SoundVolumePercent => Mathf.RoundToInt(SoundVolume * 100f);  <- 이ㅣ것 또한 사운드 매니저 디스카드해서 오류난거

    public SettingPopupViewModel()
    {
        // 초기 볼륨 로드
        if (Managers.Sound != null)
        {
            OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// 슬라이더에서 호출되는 볼륨 변경 메서드
    /// </summary>
    public void OnSoundVolumeChanged(float value)
    {
        // SoundVolume = value;  <- 이ㅣ것 또한 사운드 매니저 디스카드해서 오류난거
    }
}




using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Setting 버튼 클릭 시 표시되는 팝업입니다.
/// 사운드 볼륨 조절 기능을 제공합니다.
/// </summary>
public class UI_SettingPopup : UI_Popup
{
    [Header("Buttons")]
    [SerializeField] private Button _closeButton;

    [Header("Sound Settings")]
    [SerializeField] private TMP_Text _soundLabelText; // "소리" 레이블
    [SerializeField] private Slider _soundVolumeSlider; // 소리 볼륨 슬라이더

    private SettingPopupViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);

        // 슬라이더 이벤트 연결
        if (_soundVolumeSlider != null)
        {
            _soundVolumeSlider.minValue = 0f;
            _soundVolumeSlider.maxValue = 1f;
            _soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        }
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = (SettingPopupViewModel)viewModel;
        base.SetViewModel(viewModel);
    }

    private void OnClickClose()
    {
        Managers.UI.Close(this);
    }

    private void OnSoundVolumeChanged(float value)
    {
        _viewModel?.OnSoundVolumeChanged(value);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        // 슬라이더 값 업데이트 (이벤트 발생 없이)
        if (_soundVolumeSlider != null)
        {
           // _soundVolumeSlider.SetValueWithoutNotify(_viewModel.SoundVolume); <- 사운드 매니저 디스카드해서 오류난거 주석임~
        }

        // "소리" 레이블 텍스트 설정 (필요시)
        if (_soundLabelText != null && string.IsNullOrEmpty(_soundLabelText.text))
        {
            _soundLabelText.text = "소리";
        }
    }
}




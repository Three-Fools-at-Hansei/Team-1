using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_SettingPopup : UI_Popup
{
    [Header("Buttons")]
    [SerializeField] private Button _closeButton;

    [Header("Sound Settings")]
    [SerializeField] private TMP_Text _soundLabelText;
    [SerializeField] private Slider _soundVolumeSlider;

    private SettingPopupViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);

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
        Managers.Sound.PlaySFX("Select");
        Managers.UI.Close(this);
    }

    private void OnSoundVolumeChanged(float value)
    {
        _viewModel?.OnSoundVolumeChanged(value);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        // 슬라이더 UI 갱신 (무한 루프 방지용 SetValueWithoutNotify 사용)
        if (_soundVolumeSlider != null)
        {
            _soundVolumeSlider.SetValueWithoutNotify(_viewModel.SoundVolume);
        }

        if (_soundLabelText != null && string.IsNullOrEmpty(_soundLabelText.text))
        {
            _soundLabelText.text = "소리";
        }
    }
}
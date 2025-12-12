using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 추가

public class UI_SettingPopup : UI_Popup
{
    [Header("Buttons")]
    [SerializeField] private Button _closeButton;

    [Header("Sound Settings")]
    [SerializeField] private TMP_Text _soundLabelText;
    [SerializeField] private Slider _soundVolumeSlider;

    private SettingPopupViewModel _viewModel;

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

    protected override void Awake()
    {
        base.Awake();

        // [연출] 초기화
        _fadeIn = new FadeInUIAnimation(0.3f, Ease.OutQuad);
        _fadeOut = new FadeOutUIAnimation(0.2f, Ease.InQuad);
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        if (_closeButton != null)
            _closeButton.onClick.AddListener(OnClickClose);

        if (_soundVolumeSlider != null)
        {
            _soundVolumeSlider.minValue = 0f;
            _soundVolumeSlider.maxValue = 1f;
            _soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        }
    }

    // [연출] 등장 애니메이션
    private void OnEnable()
    {
        _fadeIn?.ExecuteAsync(_canvasGroup);
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = (SettingPopupViewModel)viewModel;
        base.SetViewModel(viewModel);
    }

    // [연출] 닫기 애니메이션 대기 후 Close
    private async void OnClickClose()
    {
        Managers.Sound.PlaySFX("Select");

        if (_fadeOut != null)
        {
            await _fadeOut.ExecuteAsync(_canvasGroup);
        }

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
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 메인 화면 View입니다.
/// Play, Setting, Exit 버튼과 버전 정보를 표시합니다.
/// </summary>
public class UI_LobbyView : UI_View
{
    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _exitButton;

    [Header("Version Info")]
    [SerializeField] private TMP_Text _versionText;

    private LobbyViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        // 버튼 이벤트 연결
        if (_playButton != null)
            _playButton.onClick.AddListener(OnClickPlay);

        if (_settingButton != null)
            _settingButton.onClick.AddListener(OnClickSetting);

        if (_exitButton != null)
            _exitButton.onClick.AddListener(OnClickExit);
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = (LobbyViewModel)viewModel;
        base.SetViewModel(viewModel);
    }

    private void OnClickPlay()
    {
        _viewModel?.OnClickPlay();
    }

    private void OnClickSetting()
    {
        _viewModel?.OnClickSetting();
    }

    private void OnClickExit()
    {
        _viewModel?.OnClickExit();
    }

    protected override void OnStateChanged()
    {
        if (_viewModel != null && _versionText != null)
        {
            _versionText.text = _viewModel.VersionText;
        }
    }
}




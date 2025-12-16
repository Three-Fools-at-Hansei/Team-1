using System.Threading.Tasks;
using DG.Tweening; // DOTween 필요
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoadingPopup : UI_DontDestroyPopup
{
    [Header("UI Components")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TMP_Text _progressText;

    private LoadingViewModel _viewModel;

    // [연출] 애니메이션 객체
    private IUIAnimation _fadeIn;
    private IUIAnimation _fadeOut;

    protected override void Awake()
    {
        base.Awake();

        // [연출] 초기화 (부드러운 등/퇴장을 위해 Ease 설정)
        _fadeIn = new FadeInUIAnimation(0.5f, Ease.OutQuad);
        _fadeOut = new FadeOutUIAnimation(0.5f, Ease.InQuad);

        // 생성 시점에는 보이지 않도록 투명 처리
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        // [연출] 팝업 활성화 시 Fade In 시작
        _fadeIn?.ExecuteAsync(_canvasGroup);
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        // 안전한 이벤트 구독 관리를 위해 기존 연결 해제
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        base.SetViewModel(viewModel);
        _viewModel = viewModel as LoadingViewModel;

        // 새 ViewModel 이벤트 구독
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested += OnCloseRequested;
        }
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        // UI 갱신
        if (_progressBar != null)
            _progressBar.value = _viewModel.Progress;

        if (_progressText != null)
            _progressText.text = _viewModel.LoadingText;
    }

    /// <summary>
    /// ViewModel에서 Progress가 100%가 되었을 때 호출됨
    /// </summary>
    private async void OnCloseRequested()
    {
        // [연출] Fade Out 실행 (완료될 때까지 대기)
        if (_fadeOut != null)
        {
            await _fadeOut.ExecuteAsync(_canvasGroup);
        }

        // 연출 종료 후 UI 닫기 (Destroy)
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }
        base.OnDestroy();
    }
}
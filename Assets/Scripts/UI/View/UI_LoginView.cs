using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginView : UI_View, ILoginView
{
    [SerializeField] private TMP_InputField _idInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private Button _submitButton;
    [SerializeField] private Button _signupButton;
    [SerializeField] private TMP_Text _helperText;
    [SerializeField] private Image _submitButtonBg;
    [SerializeField] private Image _topIcon;

    private LoginViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

    }

  
    public override void SetViewModel(IViewModel viewModel)
    {
        base.SetViewModel(viewModel);
        
        // 기존 이벤트 제거
        UnbindEvents();

        _viewModel = viewModel as LoginViewModel;
        
        if (_viewModel != null)
        {

            
            // 이벤트 바인딩 (View는 이벤트를 듣고 ViewModel에 전달만 함)
            BindEvents();
            
            // ViewModel에 View 연결
            _viewModel.SetView(this);
        }
    }

    private void BindEvents()
    {
        if (_idInput != null)
            _idInput.onValueChanged.AddListener(id => _viewModel?.OnChangeId(id));

        if (_passwordInput != null)
        {
            _passwordInput.onValueChanged.AddListener(pw => _viewModel?.OnChangePassword(pw));
            _passwordInput.onSubmit.AddListener(_ => _viewModel?.OnPressEnter());
        }

        if (_submitButton != null)
            _submitButton.onClick.AddListener(async () => { if (_viewModel != null) await _viewModel.OnClickSubmit(); });

        if (_signupButton != null)
            _signupButton.onClick.AddListener(() => _viewModel?.OnClickSignup());
    }

    private void UnbindEvents()
    {
        if (_idInput != null)
            _idInput.onValueChanged.RemoveAllListeners();

        if (_passwordInput != null)
        {
            _passwordInput.onValueChanged.RemoveAllListeners();
            _passwordInput.onSubmit.RemoveAllListeners();
        }

        if (_submitButton != null)
            _submitButton.onClick.RemoveAllListeners();

        if (_signupButton != null)
            _signupButton.onClick.RemoveAllListeners();
    }



    // ILoginView 인터페이스 구현 (ViewModel이 호출하는 세터)
    public void SetIdInputText(string text)
    {

    }

    public void SetPasswordInputText(string text)
    {

    }

    public void SetHelperText(string text)
    {
        if (_helperText != null)
            _helperText.text = text ?? "";
    }

    public void SetSubmitButtonEnabled(bool enabled)
    {
        if (_submitButton != null)
            _submitButton.interactable = enabled;
    }

    public void SetSubmitButtonColor(Color color)
    {
        if (_submitButtonBg != null)
            _submitButtonBg.color = color;
    }

    public void SetTopIconSprite(Sprite sprite)
    {
        if (_topIcon != null)
        {
            _topIcon.sprite = sprite;
            _topIcon.enabled = (sprite != null);
        }
    }

    // UI_View 추상 메서드 (사용하지 않지만 필수 구현)
    protected override void OnStateChanged() { }

    protected override void OnDestroy()
    {
        UnbindEvents();
        base.OnDestroy();
    }
}

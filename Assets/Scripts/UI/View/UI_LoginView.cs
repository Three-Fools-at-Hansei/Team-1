using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_LoginView : UI_View, ILoginView
{
    [Header("Login Layout")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private TMP_InputField _loginIdInput;
    [SerializeField] private TMP_InputField _loginPasswordInput;
    [SerializeField] private Button _loginSubmitButton;
    [SerializeField] private Image _loginSubmitButtonBg;
    [SerializeField] private Button _openSignupButton;

    [Header("Signup Layout")]
    [SerializeField] private GameObject _signupPanel;
    [SerializeField] private TMP_InputField _signupNicknameInput;
    [SerializeField] private TMP_InputField _signupIdInput;
    [SerializeField] private TMP_InputField _signupPasswordInput;
    [SerializeField] private TMP_InputField _signupPasswordConfirmInput;
    [SerializeField] private Button _signupSubmitButton;
    [SerializeField] private Image _signupSubmitButtonBg;
    [SerializeField] private Button _backToLoginButton;

    [Header("Common UI")]
    [SerializeField] private TMP_Text _helperText;
    [SerializeField] private Image _topIcon;

    private LoginViewModel _viewModel;
    private bool _isLoginLayout = true;
    private bool _lastSubmitEnabled;
    private Color _lastSubmitColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        ShowLoginLayout();
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = (LoginViewModel)viewModel;
        BindEvents();
        _viewModel.SetView(this);
        base.SetViewModel(viewModel);
    }

    private void BindEvents()
    {
        // Input Fields
        _loginIdInput.onValueChanged.AddListener(value => _viewModel.OnChangeLoginId(value));
        _loginPasswordInput.onValueChanged.AddListener(value => _viewModel.OnChangeLoginPassword(value));

        // 엔터키 입력
        _loginPasswordInput.onSubmit.AddListener(_ =>
        {
            Managers.Sound.PlaySFX("Select"); 
            _viewModel.OnPressEnter();
        });

        // Login Buttons
        _loginSubmitButton.onClick.AddListener(async () =>
        {
            Managers.Sound.PlaySFX("Select");
            await _viewModel.OnClickSubmit();
        });

        _openSignupButton.onClick.AddListener(() =>
        {
            Managers.Sound.PlaySFX("Select");
            OnTogglePanel();
        });

        // Signup Input Fields
        _signupNicknameInput.onValueChanged.AddListener(value => _viewModel.OnChangeSignupNickname(value));
        _signupIdInput.onValueChanged.AddListener(value => _viewModel.OnChangeSignupId(value));
        _signupPasswordInput.onValueChanged.AddListener(value => _viewModel.OnChangeSignupPassword(value));
        _signupPasswordConfirmInput.onValueChanged.AddListener(value => _viewModel.OnChangeSignupPasswordConfirm(value));

        // Signup Buttons
        _signupSubmitButton.onClick.AddListener(async () =>
        {
            Managers.Sound.PlaySFX("Select");
            await _viewModel.OnClickSubmit();
        });

        _backToLoginButton.onClick.AddListener(() =>
        {
            Managers.Sound.PlaySFX("Select");
            OnTogglePanel();
        });
    }

    private void OnTogglePanel()
    {
        if (_isLoginLayout)
        {
            _viewModel.OnRequestSignupMode();
        }
        else
        {
            _viewModel.OnRequestLoginMode();
        }
    }

    public void SetIdInputText(string text)
    {
        if (_loginIdInput.text != text)
            _loginIdInput.SetTextWithoutNotify(text);
    }

    public void SetPasswordInputText(string text)
    {
        if (_loginPasswordInput.text != text)
            _loginPasswordInput.SetTextWithoutNotify(text);
    }

    public void SetSignupNicknameText(string text)
    {
        if (_signupNicknameInput.text != text)
            _signupNicknameInput.SetTextWithoutNotify(text);
    }

    public void SetSignupIdText(string text)
    {
        if (_signupIdInput.text != text)
            _signupIdInput.SetTextWithoutNotify(text);
    }

    public void SetSignupPasswordText(string text)
    {
        if (_signupPasswordInput.text != text)
            _signupPasswordInput.SetTextWithoutNotify(text);
    }

    public void SetSignupPasswordConfirmText(string text)
    {
        if (_signupPasswordConfirmInput.text != text)
            _signupPasswordConfirmInput.SetTextWithoutNotify(text);
    }

    public void SetHelperText(string text)
    {
        _helperText.text = text ?? string.Empty;
    }

    public void SetSubmitButtonEnabled(bool enabled)
    {
        _lastSubmitEnabled = enabled;
        _loginSubmitButton.interactable = _isLoginLayout && enabled;
        _signupSubmitButton.interactable = !_isLoginLayout && enabled;
    }

    public void SetSubmitButtonColor(Color color)
    {
        _lastSubmitColor = color;
        _loginSubmitButtonBg.color = color;
        _signupSubmitButtonBg.color = color;
    }

    public void SetTopIconSprite(Sprite sprite)
    {
        _topIcon.sprite = sprite;
        _topIcon.enabled = sprite != null;
    }

    public void ShowLoginLayout()
    {
        _isLoginLayout = true;
        _loginPanel.SetActive(true);
        _signupPanel.SetActive(false);
        SetSubmitButtonEnabled(_lastSubmitEnabled);
        SetSubmitButtonColor(_lastSubmitColor);
        UpdateBackToLoginButtonRotation();
    }

    public void ShowSignupLayout()
    {
        _isLoginLayout = false;
        _loginPanel.SetActive(false);
        _signupPanel.SetActive(true);
        SetSubmitButtonEnabled(_lastSubmitEnabled);
        SetSubmitButtonColor(_lastSubmitColor);
        UpdateBackToLoginButtonRotation();
    }

    private void UpdateBackToLoginButtonRotation()
    {
        if (_backToLoginButton != null)
        {
            RectTransform buttonRect = _backToLoginButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                Vector3 scale = buttonRect.localScale;
                scale.x = _isLoginLayout ? -1f : 1f;
                buttonRect.localScale = scale;
            }
        }
    }

    protected override void OnStateChanged() { }
}

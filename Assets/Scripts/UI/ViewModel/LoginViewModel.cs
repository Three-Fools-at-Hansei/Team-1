using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class LoginViewModel : UI.IViewModel
{
    private enum Mode
    {
        Login,
        Signup
    }

    private static readonly Regex SignupPasswordPolicy =
        new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,30}$", RegexOptions.Compiled);

    public event System.Action OnStateChanged;

    private readonly Color ActiveColor = new(0.0f, 0.75f, 0.3f, 1f);
    private readonly Color DisabledColor = new(0.6f, 0.6f, 0.6f, 1f);

    private Mode _mode = Mode.Login;
    private ILoginView _view;

    private string _loginId = string.Empty;
    private string _loginPassword = string.Empty;

    private string _signupNickname = string.Empty;
    private string _signupId = string.Empty;
    private string _signupPassword = string.Empty;
    private string _signupPasswordConfirm = string.Empty;

    private bool _canSubmit;

    public string HelperMessage { get; private set; } = "아이디와 비밀번호를 입력해주세요.";
    public bool IsBusy { get; private set; }
    public bool IsValid => _canSubmit;

    public void SetView(ILoginView view)
    {
        _view = view;
        Render();
    }

    #region Login Input
    public void OnChangeLoginId(string value)
    {
        _loginId = value ?? string.Empty;
        if (_mode == Mode.Login)
        {
            ValidateInputs();
            Render();
        }
    }

    public void OnChangeLoginPassword(string value)
    {
        _loginPassword = value ?? string.Empty;
        if (_mode == Mode.Login)
        {
            ValidateInputs();
            Render();
        }
    }
    #endregion

    #region Signup Input
    public void OnChangeSignupNickname(string value)
    {
        _signupNickname = value ?? string.Empty;
        if (_mode == Mode.Signup)
        {
            ValidateInputs();
            Render();
        }
    }

    public void OnChangeSignupId(string value)
    {
        _signupId = value ?? string.Empty;
        if (_mode == Mode.Signup)
        {
            ValidateInputs();
            Render();
        }
    }

    public void OnChangeSignupPassword(string value)
    {
        _signupPassword = value ?? string.Empty;
        if (_mode == Mode.Signup)
        {
            ValidateInputs();
            Render();
        }
    }

    public void OnChangeSignupPasswordConfirm(string value)
    {
        _signupPasswordConfirm = value ?? string.Empty;
        if (_mode == Mode.Signup)
        {
            ValidateInputs();
            Render();
        }
    }
    #endregion

    public void OnRequestSignupMode()
    {
        if (_mode == Mode.Signup)
            return;

        _mode = Mode.Signup;
        IsBusy = false;
        HelperMessage = "닉네임, 아이디, 비밀번호를 입력해주세요.";
        ValidateInputs();
        Render();
    }

    public void OnRequestLoginMode()
    {
        if (_mode == Mode.Login)
            return;

        _mode = Mode.Login;
        IsBusy = false;
        HelperMessage = "아이디와 비밀번호를 입력해주세요.";
        ValidateInputs();
        Render();
    }

    public void OnPressEnter()
    {
        if (_mode != Mode.Login)
            return;

        if (_canSubmit && !IsBusy)
            _ = OnClickSubmit();
    }

    public async Task OnClickSubmit()
    {
        if (!_canSubmit || IsBusy)
            return;

        if (Login.Instance == null)
        {
            Debug.LogError("[LoginViewModel] Login 서비스 인스턴스를 찾을 수 없습니다.");
            HelperMessage = "로그인 서비스를 찾을 수 없습니다.";
            Render();
            return;
        }

        if (_mode == Mode.Login)
            await ProcessLoginAsync();
        else
            await ProcessSignupAsync();
    }

    private async Task ProcessLoginAsync()
    {
        IsBusy = true;
        HelperMessage = "로그인 중...";
        Render();

        var result = await Login.Instance.SignInWithUsernamePasswordAsync(_loginId.Trim(), _loginPassword);

        if (result.IsSuccessful)
        {
            HelperMessage = string.IsNullOrEmpty(result.Message) ? "로그인 성공!" : result.Message;
            Render();
            // Managers.Scene.LoadSceneAsync();
            return;
        }

        IsBusy = false;
        HelperMessage = string.IsNullOrEmpty(result.Message) ? "아이디 또는 비밀번호를 다시 확인해주세요." : result.Message;
        Render();
    }

    private async Task ProcessSignupAsync()
    {
        IsBusy = true;
        HelperMessage = "회원가입 중...";
        Render();

        var result = await Login.Instance.SignUpWithUsernamePasswordAsync(_signupId.Trim(), _signupPassword);

        IsBusy = false;
        if (result.IsSuccessful)
        {
            HelperMessage = string.IsNullOrEmpty(result.Message) ? "회원가입이 완료되었습니다." : result.Message;

            // 회원가입 정보로 로그인 탭 자동 채우기
            _loginId = _signupId;
            _loginPassword = _signupPassword;

            // 회원가입 입력 초기화
            _signupPasswordConfirm = string.Empty;
            _signupPassword = string.Empty;

            _mode = Mode.Login;
            ValidateInputs();
            Render();
            return;
        }

        HelperMessage = string.IsNullOrEmpty(result.Message) ? "회원가입에 실패했습니다." : result.Message;
        Render();
    }

    private void ValidateInputs()
    {
        switch (_mode)
        {
            case Mode.Login:
                bool loginIdValid = !string.IsNullOrWhiteSpace(_loginId) && _loginId.Trim().Length >= 3;
                bool loginPasswordValid = !string.IsNullOrEmpty(_loginPassword) && _loginPassword.Length >= 6;

                _canSubmit = loginIdValid && loginPasswordValid;

                if (!_canSubmit)
                {
                    HelperMessage = !loginIdValid
                        ? "아이디는 3자 이상 입력해주세요."
                        : (!loginPasswordValid ? "비밀번호는 6자 이상 입력해주세요." : HelperMessage);
                }
                else
                {
                    HelperMessage = "로그인할 수 있습니다.";
                }
                break;

            case Mode.Signup:
                bool nicknameValid = !string.IsNullOrWhiteSpace(_signupNickname);
                bool signupIdValid = !string.IsNullOrWhiteSpace(_signupId) && _signupId.Trim().Length >= 3;
                bool signupPasswordValid = !string.IsNullOrEmpty(_signupPassword) && SignupPasswordPolicy.IsMatch(_signupPassword);
                bool confirmValid = signupPasswordValid && _signupPassword == _signupPasswordConfirm;

                _canSubmit = nicknameValid && signupIdValid && signupPasswordValid && confirmValid;

                if (!_canSubmit)
                {
                    if (!nicknameValid)
                        HelperMessage = "닉네임을 입력해주세요.";
                    else if (!signupIdValid)
                        HelperMessage = "아이디는 3자 이상 입력해주세요.";
                    else if (!signupPasswordValid)
                        HelperMessage = "비밀번호는 8~30자, 대문자·소문자·숫자·특수문자를 각각 1개 이상 포함해야 합니다.";
                    else if (!confirmValid)
                        HelperMessage = "비밀번호 확인이 일치하지 않습니다.";
                }
                else
                {
                    HelperMessage = "회원가입을 진행할 수 있습니다.";
                }
                break;
        }
    }

    public void Render()
    {
        if (_view == null)
            return;

        if (_mode == Mode.Login)
        {
            _view.ShowLoginLayout();
            _view.SetIdInputText(_loginId);
            _view.SetPasswordInputText(_loginPassword);
        }
        else
        {
            _view.ShowSignupLayout();
            _view.SetSignupNicknameText(_signupNickname);
            _view.SetSignupIdText(_signupId);
            _view.SetSignupPasswordText(_signupPassword);
            _view.SetSignupPasswordConfirmText(_signupPasswordConfirm);
        }

        _view.SetHelperText(HelperMessage);
        _view.SetSubmitButtonEnabled(_canSubmit && !IsBusy);
        _view.SetSubmitButtonColor((_canSubmit && !IsBusy) ? ActiveColor : DisabledColor);
    }
}

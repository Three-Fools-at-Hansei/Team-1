using System.Threading.Tasks;
using UnityEngine;

public class LoginViewModel : UI.IViewModel
{
    public event System.Action OnStateChanged;

    // 상태
    public string Id { get; private set; } = "";
    public string Password { get; private set; } = "";
    public string HelperMessage { get; private set; } = "아이디와 비밀번호를 입력해주세요.";
    public bool IsValid { get; private set; } = false;
    public bool IsBusy { get; private set; } = false;

    // 색상 정책
    private static readonly Color ActiveColor = new Color(0.0f, 0.75f, 0.3f, 1f);   // 초록색
    private static readonly Color DisabledColor = new Color(0.6f, 0.6f, 0.6f, 1f); // 회색

    private ILoginView _view;

    public void SetView(ILoginView view)
    {
        _view = view;
        Render();
    }

    // 입력 변경 처리
    public void OnChangeId(string id)
    {
        Id = id ?? "";
        ValidateInputs();
        Render();
    }

    public void OnChangePassword(string password)
    {
        Password = password ?? "";
        ValidateInputs();
        Render();
    }

    // 유효성 검사
    private void ValidateInputs()
    {
        bool idValid = !string.IsNullOrWhiteSpace(Id) && Id.Trim().Length >= 3;
        bool passwordValid = !string.IsNullOrEmpty(Password) && Password.Length >= 6;

        IsValid = idValid && passwordValid;

        if (IsValid)
        {
            HelperMessage = "로그인할 수 있습니다.";
        }
        else if (string.IsNullOrWhiteSpace(Id) && string.IsNullOrEmpty(Password))
        {
            HelperMessage = "아이디와 비밀번호를 입력해주세요.";
        }
        else if (!idValid)
        {
            HelperMessage = "아이디는 3자 이상 입력해주세요.";
        }
        else
        {
            HelperMessage = "비밀번호는 6자 이상 입력해주세요.";
        }
    }

    // Enter 키 처리
    public void OnPressEnter()
    {
        if (IsValid && !IsBusy)
        {
            _ = OnClickSubmit();
        }
    }

    // 로그인 버튼 클릭
    public async Task OnClickSubmit()
    {
        if (!IsValid || IsBusy) return;

        IsBusy = true;
        HelperMessage = "로그인 중...";
        Render();

        if (Login.Instance == null)
        {
            Debug.LogError("[LoginViewModel] Login 서비스 인스턴스를 찾을 수 없습니다.");
            HelperMessage = "로그인 서비스를 찾을 수 없습니다.";
            IsBusy = false;
            Render();
            return;
        }

        var result = await Login.Instance.SignInWithUsernamePasswordAsync(Id.Trim(), Password);

        if (result.IsSuccessful)
        {
            HelperMessage = string.IsNullOrEmpty(result.Message) ? "로그인 성공!" : result.Message;
            Render();
            Managers.Scene.LoadScene(eSceneType.Main);
            return;
        }

        IsBusy = false;
        HelperMessage = string.IsNullOrEmpty(result.Message) ? "아이디 또는 비밀번호를 다시 확인해주세요." : result.Message;
        Render();
    }

    // 회원가입 버튼 클릭
    public async void OnClickSignup()
    {
        if (IsBusy)
            return;

        if (!IsValid)
        {
            HelperMessage = "아이디와 비밀번호를 먼저 입력해주세요.";
            Render();
            return;
        }

        if (Login.Instance == null)
        {
            Debug.LogError("[LoginViewModel] Login 서비스 인스턴스를 찾을 수 없습니다.");
            HelperMessage = "회원가입 서비스를 찾을 수 없습니다.";
            Render();
            return;
        }

        IsBusy = true;
        HelperMessage = "회원가입 중...";
        Render();

        var result = await Login.Instance.SignUpWithUsernamePasswordAsync(Id.Trim(), Password);

        IsBusy = false;
        HelperMessage = string.IsNullOrEmpty(result.Message)
            ? (result.IsSuccessful ? "회원가입이 완료되었습니다." : "회원가입에 실패했습니다.")
            : result.Message;
        Render();
    }

    // View 렌더링 (ViewModel이 직접 View의 세터를 호출)
    public void Render()
    {
        if (_view == null) return;

        _view.SetHelperText(HelperMessage);
        _view.SetSubmitButtonEnabled(IsValid && !IsBusy);
        _view.SetSubmitButtonColor((IsValid && !IsBusy) ? ActiveColor : DisabledColor);
    }
}

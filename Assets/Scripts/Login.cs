using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Login : MonoBehaviour
{
    public static Login Instance { get; private set; }

    private readonly CloudSave _cloudSave = new();
    private Task _initializationTask;
    private bool _initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Unity Services 초기화를 보장합니다. 여러 번 호출해도 한 번만 초기화됩니다.
    /// </summary>
    public Task EnsureInitializedAsync()
    {
        if (_initialized)
            return Task.CompletedTask;

        return _initializationTask ??= InitializeServicesAsync();
    }

    private async Task InitializeServicesAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            _initialized = true;
            Debug.Log("Unity Services initialized.");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }

    public async Task<LoginResult> SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await EnsureInitializedAsync();

            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
                Debug.Log("이전 세션을 로그아웃했습니다.");
            }

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
            return LoginResult.Success("로그인에 성공했습니다.");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return LoginResult.Failure(MapErrorMessage(ex));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return LoginResult.Failure("로그인 중 알 수 없는 오류가 발생했습니다.");
        }
    }

    public async Task<LoginResult> SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await EnsureInitializedAsync();

            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
                Debug.Log("이전 세션을 로그아웃했습니다.");
            }

            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
            _cloudSave.SaveData(username);
            return LoginResult.Success("회원가입이 완료되었습니다.");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return LoginResult.Failure(MapErrorMessage(ex));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return LoginResult.Failure("회원가입 중 알 수 없는 오류가 발생했습니다.");
        }
    }

    private string MapErrorMessage(RequestFailedException ex)
    {
        if (ex.ErrorCode == AuthenticationErrorCodes.InvalidParameters ||
            ex.ErrorCode == CommonErrorCodes.InvalidRequest ||
            ex.ErrorCode == CommonErrorCodes.NotFound)
        {
            return "아이디 또는 비밀번호가 올바르지 않습니다.";
        }

        if (ex.ErrorCode == CommonErrorCodes.TooManyRequests)
        {
            return "잠시 후 다시 시도해주세요. (요청이 너무 많습니다)";
        }

        if (ex.ErrorCode == CommonErrorCodes.Timeout || ex.ErrorCode == CommonErrorCodes.ServiceUnavailable)
        {
            return "서버와의 연결이 원활하지 않습니다. 잠시 후 다시 시도해주세요.";
        }

        if (ex.ErrorCode == CommonErrorCodes.Conflict)
        {
            return "이미 사용 중인 아이디입니다.";
        }

        return $"요청 처리 중 오류가 발생했습니다. (코드: {ex.ErrorCode})";
    }

    public readonly struct LoginResult
    {
        public bool IsSuccessful { get; }
        public string Message { get; }

        private LoginResult(bool isSuccessful, string message)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }

        public static LoginResult Success(string message) => new LoginResult(true, message);
        public static LoginResult Failure(string message) => new LoginResult(false, message);
    }
}


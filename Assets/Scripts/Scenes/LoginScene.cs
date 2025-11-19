using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class LoginScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Login;

    public List<string> RequiredDataFiles => new()
    {
        "ItemGameData.json",
        "MissionGameData.json",
        "NikkeGameData.json",
    };

    private bool _initialized;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
    }

    async void Start()
    {
        await InitializeAsync();
    }

    async void IScene.Init()
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;

        await EnsureLoginServiceAsync();

        Debug.Log("LoginScene Init");
        await Managers.UI.ShowAsync<UI_LoginView>(new LoginViewModel());
    }

    private async Task EnsureLoginServiceAsync()
    {
        if (Login.Instance == null)
        {
            var loginGo = new GameObject("@Login");
            loginGo.AddComponent<Login>();
        }

        await Login.Instance.EnsureInitializedAsync();
    }

    void IScene.Clear()
    {
        Debug.Log("LoginScene Clear");
    }
}

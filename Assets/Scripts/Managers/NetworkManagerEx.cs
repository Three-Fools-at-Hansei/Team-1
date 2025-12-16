using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkManagerEx : IManagerBase
{
    public eManagerType ManagerType => eManagerType.Network;

    private const string JOIN_CODE_KEY = "j";
    private Lobby _currentLobby;
    private bool _isServicesInitialized = false;

    // [New] ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Îºï¿½ ï¿½Úµï¿½ (È£ï¿½ï¿½Æ®/Å¬ï¿½ï¿½ï¿½Ì¾ï¿½Æ® ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½)
    public string CurrentLobbyCode { get; private set; }

    // --- IManagerBase ï¿½ï¿½ï¿½ï¿½ ---
    public void Init()
    {
        _ = EnsureNetworkManagerExistsAsync();

        // [New] Transport ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ Ä¡ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Ìºï¿½Æ® ï¿½ï¿½ï¿½ï¿½
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        }

        Debug.Log($"{ManagerType} Manager Init.");
    }

    public void Update()
    {
        HandleLobbyHeartbeat();
    }

    public void Clear()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
            NetworkManager.Singleton.Shutdown();
        }

        _currentLobby = null;
        CurrentLobbyCode = string.Empty;
    }

    // ========================================================================
    // 1. UGS ÀÎÁõ
    // ========================================================================

    public async Task InitServicesAsync()
    {
        if (_isServicesInitialized) return;
        try
        {
            await UnityServices.InitializeAsync();
            _isServicesInitialized = true;
            Debug.Log("[Network] Unity Services ÃÊ±âÈ­ ¿Ï·á.");
        }
        catch (Exception e) { Debug.LogError($"[Network] ÃÊ±âÈ­ ½ÇÆÐ: {e.Message}"); }
    }

    public async Task<(bool success, string message)> SignInAsync(string id, string pw)
    {
        await InitServicesAsync();
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut(true);

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(id, pw);
            Debug.Log($"[Network] ·Î±×ÀÎ ¼º°ø: {AuthenticationService.Instance.PlayerId}");
            return (true, "·Î±×ÀÎ ¼º°ø");
        }
        catch (RequestFailedException ex) { return (false, MapErrorMessage(ex)); }
    }

    public async Task<(bool success, string message)> SignUpAsync(string id, string pw)
    {
        await InitServicesAsync();
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut(true);

            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(id, pw);
            Debug.Log($"[Network] È¸¿ø°¡ÀÔ ¼º°ø: {AuthenticationService.Instance.PlayerId}");
            return (true, "È¸¿ø°¡ÀÔ ¿Ï·á");
        }
        catch (RequestFailedException ex) { return (false, MapErrorMessage(ex)); }
    }

    // ========================================================================
    // 2. ¸ÖÆ¼ÇÃ·¹ÀÌ¾î ¿¬°á (Relay & Lobby)
    // ========================================================================

    public async Task<bool> StartHostAsync(int maxPlayers = 2)
    {
        await EnsureNetworkManagerExistsAsync();

        if (!AuthenticationService.Instance.IsSignedIn) return false;

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { JOIN_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, joinCode) } },
                IsLocked = false
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync($"Room_{UnityEngine.Random.Range(1000, 9999)}", maxPlayers, options);
            CurrentLobbyCode = _currentLobby.LobbyCode;
            Debug.Log($"[Network] ¹æ »ý¼º ¿Ï·á. Code: {CurrentLobbyCode}");

            if (NetworkManager.Singleton.StartHost())
            {
                LoadCombatScene();
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] ¹æ »ý¼º ½ÇÆÐ: {e.Message}");
            return false;
        }
    }

    public async Task<(bool success, string message)> JoinByCodeAsync(string roomCode)
    {
        await EnsureNetworkManagerExistsAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            return (false, "·Î±×ÀÎÀÌ ÇÊ¿äÇÕ´Ï´Ù.");

        try
        {
            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);
            CurrentLobbyCode = _currentLobby.LobbyCode;

            string joinCode = _currentLobby.Data[JOIN_CODE_KEY].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            bool started = NetworkManager.Singleton.StartClient();
            return (started, started ? "ÀÔÀå ¼º°ø" : "Å¬¶óÀÌ¾ðÆ® ½ÃÀÛ ½ÇÆÐ");
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound) return (false, "¹æÀ» Ã£À» ¼ö ¾ø½À´Ï´Ù.");
            if (e.Reason == LobbyExceptionReason.LobbyFull) return (false, "¹æÀÌ ²Ë Ã¡½À´Ï´Ù.");
            if (e.Reason == LobbyExceptionReason.LobbyConflict) return (false, "°ÔÀÓÀÌ ÁøÇà ÁßÀÌ°Å³ª ÀÔÀåÇÒ ¼ö ¾ø½À´Ï´Ù.");
            Debug.LogWarning($"[Network] Âü°¡ ½ÇÆÐ (Lobby Error): {e.Message}");
            return (false, "ÀÔÀå ½ÇÆÐ: ¾Ë ¼ö ¾ø´Â ¿À·ù");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] Âü°¡ ½ÇÆÐ: {e.Message}");
            return (false, "ÀÔÀå Áß ¿À·ù ¹ß»ý");
        }
    }

    public async Task SetLobbyLockStateAsync(bool isLocked)
    {
        if (_currentLobby == null || !NetworkManager.Singleton.IsHost) return;
        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions { IsLocked = isLocked });
        }
        catch (Exception e) { Debug.LogError($"[Network] ·Îºñ Àá±Ý ¼³Á¤ ½ÇÆÐ: {e.Message}"); }
    }

    public async Task<bool> QuickJoinAsync()
    {
        await EnsureNetworkManagerExistsAsync();
        if (!AuthenticationService.Instance.IsSignedIn) return false;

        try
        {
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            CurrentLobbyCode = _currentLobby.LobbyCode;
            string joinCode = _currentLobby.Data[JOIN_CODE_KEY].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            return NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] Âü°¡ ½ÇÆÐ: {e.Message}");
            return false;
        }
    }

    // --- ³»ºÎ À¯Æ¿¸®Æ¼ ---

    private async Task EnsureNetworkManagerExistsAsync()
    {
        if (NetworkManager.Singleton == null)
        {
            GameObject prefab = await Managers.Resource.LoadAsync<GameObject>("NetworkManager");

            if (prefab != null)
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.name = "NetworkManager";
                UnityEngine.Object.DontDestroyOnLoad(go);

                go.GetComponent<NetworkManager>().OnTransportFailure += OnTransportFailure;
                Debug.Log("[NetworkManagerEx] NetworkManager »ý¼º ¿Ï·á.");
            }
            else
            {
                Debug.LogError("[NetworkManagerEx] NetworkManager ÇÁ¸®ÆÕÀ» Ã£À» ¼ö ¾ø½À´Ï´Ù.");
                return;
            }
        }

        // [¼³Á¤ 1] Config Mismatch ¿¡·¯ ¹æÁö (ÇÊ¼ö)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = false;
        }

        // [¼³Á¤ 2] PlayerPrefab ¿¬°á º¹±¸ (ÇÊ¼ö)
        // ºôµå¿¡¼­ ÂüÁ¶°¡ À¯½ÇµÇ¾úÀ» °æ¿ì¸¦ ´ëºñÇØ ÄÚµå·Î Á÷Á¢ ¿¬°áÇÕ´Ï´Ù.
        // AddNetworkPrefabÀº ÀÌ¹Ì ¸®½ºÆ®¿¡ Á¸ÀçÇÏ¹Ç·Î È£ÃâÇÏÁö ¾Ê½À´Ï´Ù.
        if (NetworkManager.Singleton != null)
        {
            GameObject playerPrefab = await Managers.Resource.LoadAsync<GameObject>("Player");
            if (playerPrefab != null)
            {
                // NetworkManager°¡ ¾î¶² ÇÁ¸®ÆÕÀ» ÇÃ·¹ÀÌ¾î·Î ¾µÁö ÁöÁ¤
                NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefab;
                Debug.Log("[NetworkManagerEx] Player ÇÁ¸®ÆÕ ÇÊµå ÇÒ´ç ¿Ï·á.");
            }
            else
            {
                Debug.LogError("[NetworkManagerEx] Player ÇÁ¸®ÆÕ ·Îµå ½ÇÆÐ!");
            }
        }
    }

    private void LoadCombatScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("CombatScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private float _heartbeatTimer;

    private async void HandleLobbyHeartbeat()
    {
        if (_currentLobby != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = 15f;
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogWarning($"[Network] ÇÏÆ®ºñÆ® ½ÇÆÐ (Lobby Error): {e.Message}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Network] ÇÏÆ®ºñÆ® ¿À·ù: {e.Message}");
                }
            }
        }
    }

    private async void OnTransportFailure()
    {
        Debug.LogError("[Network] Àü¼Û °èÃþ ¿À·ù ¹ß»ý (¿¬°á ²÷±è). ·Îºñ·Î ÀÌµ¿ÇÕ´Ï´Ù.");
        Clear();

        if (Managers.Scene.CurrentScene != null && Managers.Scene.CurrentScene.SceneType == eSceneType.MainScene)
            return;

        var loadingVM = new LoadingViewModel();
        var loadingUI = await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);

        try
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.MainScene, loadingVM);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[NetworkManagerEx] ¾À ÀüÈ¯ ½ÇÆÐ: {ex}");
            Managers.UI.Close(loadingUI); // ½ÇÆÐ ½Ã ´Ý±â
        }
    }

    private string MapErrorMessage(RequestFailedException ex)
    {
        if (ex.ErrorCode == 409) return "ÀÌ¹Ì Á¸ÀçÇÏ´Â IDÀÔ´Ï´Ù.";
        if (ex.ErrorCode == 401) return "±ÇÇÑÀÌ ¾ø°Å³ª ¾ÆÀÌµð/ºñ¹øÀÌ Æ²·È½À´Ï´Ù.";
        return ex.Message;
    }
}
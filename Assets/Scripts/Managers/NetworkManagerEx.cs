using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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

    // [New] 현재 방의 로비 코드 (호스트/클라이언트 모두 참조 가능)
    public string CurrentLobbyCode { get; private set; }

    // --- IManagerBase 구현 ---
    public void Init()
    {
        _ = EnsureNetworkManagerExistsAsync();
        Debug.Log($"{ManagerType} Manager Init.");
    }

    public void Update()
    {
        HandleLobbyHeartbeat();
    }

    public void Clear()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        _currentLobby = null;
        CurrentLobbyCode = string.Empty; // 초기화
    }

    // ========================================================================
    // 1. UGS 인증
    // ========================================================================

    public async Task InitServicesAsync()
    {
        if (_isServicesInitialized) return;
        try
        {
            await UnityServices.InitializeAsync();
            _isServicesInitialized = true;
            Debug.Log("[Network] Unity Services 초기화 완료.");
        }
        catch (Exception e) { Debug.LogError($"[Network] 초기화 실패: {e.Message}"); }
    }

    public async Task<(bool success, string message)> SignInAsync(string id, string pw)
    {
        await InitServicesAsync();
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut(true);

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(id, pw);
            Debug.Log($"[Network] 로그인 성공: {AuthenticationService.Instance.PlayerId}");
            return (true, "로그인 성공");
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
            Debug.Log($"[Network] 회원가입 성공: {AuthenticationService.Instance.PlayerId}");
            return (true, "회원가입 완료");
        }
        catch (RequestFailedException ex) { return (false, MapErrorMessage(ex)); }
    }

    // ========================================================================
    // 2. 멀티플레이어 연결 (Relay & Lobby)
    // ========================================================================

    public async Task<bool> StartHostAsync(int maxPlayers = 2)
    {
        if (!AuthenticationService.Instance.IsSignedIn) return false;

        try
        {
            // A. Relay 할당
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // B. Transport 설정
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

            // C. Lobby 생성
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { JOIN_CODE_KEY, new DataObject(DataObject.VisibilityOptions.Member, joinCode) } },
                IsLocked = false // 초기에는 잠금 해제
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync($"Room_{UnityEngine.Random.Range(1000, 9999)}", maxPlayers, options);

            // [New] 로비 코드 저장
            CurrentLobbyCode = _currentLobby.LobbyCode;
            Debug.Log($"[Network] 방 생성 완료. Code: {CurrentLobbyCode}");

            // D. NGO Host 시작
            if (NetworkManager.Singleton.StartHost())
            {
                LoadCombatScene();
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] 방 생성 실패: {e.Message}");
            return false;
        }
    }

    // [New] 코드로 방 입장 (예외 처리 포함)
    public async Task<(bool success, string message)> JoinByCodeAsync(string roomCode)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
            return (false, "로그인이 필요합니다.");

        try
        {
            // A. 코드로 로비 검색 및 참가
            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(roomCode);

            // [New] 입장한 방의 코드 저장
            CurrentLobbyCode = _currentLobby.LobbyCode;

            string joinCode = _currentLobby.Data[JOIN_CODE_KEY].Value;

            // B. Relay 접속 설정
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            // C. Client 시작
            bool started = NetworkManager.Singleton.StartClient();
            return (started, started ? "입장 성공" : "클라이언트 시작 실패");
        }
        catch (LobbyServiceException e)
        {
            // UGS 에러 처리
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                return (false, "방을 찾을 수 없습니다.");
            if (e.Reason == LobbyExceptionReason.LobbyFull)
                return (false, "방이 꽉 찼습니다.");
            if (e.Reason == LobbyExceptionReason.LobbyConflict) // 잠긴 방 등
                return (false, "게임이 진행 중이거나 입장할 수 없습니다.");

            Debug.LogWarning($"[Network] 참가 실패 (Lobby Error): {e.Message}");
            return (false, "입장 실패: 알 수 없는 오류");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] 참가 실패: {e.Message}");
            return (false, "입장 중 오류 발생");
        }
    }

    // [New] 로비 잠금 상태 설정 (Host Only)
    public async Task SetLobbyLockStateAsync(bool isLocked)
    {
        if (_currentLobby == null || !NetworkManager.Singleton.IsHost) return;

        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                IsLocked = isLocked
            });
            Debug.Log($"[Network] 로비 잠금 상태 변경: {isLocked}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] 로비 잠금 설정 실패: {e.Message}");
        }
    }

    // 기존 QuickJoin은 유지하되 사용 빈도가 줄어들 것임
    public async Task<bool> QuickJoinAsync()
    {
        // (기존 코드 유지)
        if (!AuthenticationService.Instance.IsSignedIn) return false;

        try
        {
            // A. Lobby 검색
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            CurrentLobbyCode = _currentLobby.LobbyCode; // 코드 저장
            string joinCode = _currentLobby.Data[JOIN_CODE_KEY].Value;

            // B. Relay 접속 설정
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            // [중요] 생성자 대신 확장 메서드 사용
            transport.SetRelayServerData(joinAllocation.ToRelayServerData("dtls"));

            // C. Client 시작
            return NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] 참가 실패: {e.Message}");
            return false;
        }
    }

    // --- 내부 유틸리티 ---

    private async Task EnsureNetworkManagerExistsAsync()
    {
        if (NetworkManager.Singleton == null)
        {
            // 1. 리소스 매니저를 통해 프리팹 원본만 로드 (풀링 사용 X)
            GameObject prefab = await Managers.Resource.LoadAsync<GameObject>("NetworkManager");

            if (prefab != null)
            {
                // 2. 부모 없이 최상위에 직접 생성
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.name = "NetworkManager";
                UnityEngine.Object.DontDestroyOnLoad(go);
            }
            else
            {
                Debug.LogError("[NetworkManagerEx] NetworkManager 프리팹을 찾을 수 없습니다.");
            }
        }
    }

    private void LoadCombatScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("CombatScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private float _heartbeatTimer;
    private void HandleLobbyHeartbeat()
    {
        if (_currentLobby != null && NetworkManager.Singleton.IsHost)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer <= 0f)
            {
                _heartbeatTimer = 15f;
                LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
        }
    }

    private string MapErrorMessage(RequestFailedException ex)
    {
        if (ex.ErrorCode == 409) return "이미 존재하는 ID입니다.";
        if (ex.ErrorCode == 401) return "권한이 없거나 아이디/비번이 틀렸습니다.";
        return ex.Message;
    }
}
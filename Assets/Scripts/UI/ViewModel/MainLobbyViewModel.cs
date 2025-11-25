using System;
using UI;

public class MainLobbyViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    public string StatusText { get; private set; } = "게임 시작 대기 중";
    public bool IsInteractable { get; private set; } = true;

    // 방 생성 (Host)
    public async void CreateRoom()
    {
        SetStatus("방 생성 중...", false);

        // Managers.Network를 통해 호스트 시작
        bool success = await Managers.Network.StartHostAsync();

        if (success)
        {
            SetStatus("방 생성 성공! 전투 씬 로딩 중...", false);
            // 성공 시 NetworkManager가 자동으로 씬을 전환하므로 대기
        }
        else
        {
            SetStatus("방 생성 실패. 다시 시도해주세요.", true);
        }
    }

    // 방 참가 (Client)
    public async void JoinRoom()
    {
        SetStatus("방 찾는 중...", false);

        bool success = await Managers.Network.QuickJoinAsync();

        if (success)
        {
            SetStatus("방 참가 성공! 호스트 대기 중...", false);
        }
        else
        {
            SetStatus("참가 실패 (방이 없거나 오류 발생)", true);
        }
    }

    private void SetStatus(string msg, bool interactable)
    {
        StatusText = msg;
        IsInteractable = interactable;
        OnStateChanged?.Invoke();
    }
}
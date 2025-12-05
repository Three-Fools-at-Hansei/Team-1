using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class CombatScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.CombatScene;

    public List<string> RequiredDataFiles => new()
    {
        "NikkeGameData.json",
        "ItemGameData.json",
        "MissionGameData.json",
    };

    void Awake()
    {
        Managers.Input.DefaultActionMapKey = "Lobby";

        Managers.Scene.SetCurrentScene(this);
    }

    void IScene.Init()
    {
        Debug.Log("Combat Scene Init() - 전투 시작!");

        // NGO에서는 NetworkManager가 플레이어를 자동 스폰합니다.
        // 따라서 별도로 InstantiateAsync("Player")를 호출할 필요가 없습니다.
        // 다만, 카메라 세팅이나 UI 초기화 등은 여기서 수행해야 합니다.
        ShowTestUI();
    }

    void Update()
    {
        // Update() 호출 확인용 로그 (처음 한 번만)
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log($"[CombatScene] Update() 호출 중... (Frame: {Time.frameCount})");
        }

        // 테스트용: q키 = 승리 팝업, e키 = 패배 팝업
        // 새로운 Input System 사용
        if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                Debug.Log("[CombatScene] Q 키 입력 감지 - 승리 팝업 표시 시도");
                ShowCombatResultPopup(eCombatResult.Victory);
            }
            else if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("[CombatScene] E 키 입력 감지 - 패배 팝업 표시 시도");
                ShowCombatResultPopup(eCombatResult.Defeat);
            }
        }
    }

    private async void ShowCombatResultPopup(eCombatResult result)
    {
        try
        {
            Debug.Log($"[CombatScene] ShowCombatResultPopup 호출됨 - Result: {result}");
            
            if (Managers.UI == null)
            {
                Debug.LogError("[CombatScene] Managers.UI가 null입니다!");
                return;
            }

            CombatResultViewModel viewModel = new CombatResultViewModel();
            viewModel.SetResult(result);
            Debug.Log($"[CombatScene] ViewModel 생성 완료 - Title: {viewModel.TitleText}, Button: {viewModel.ButtonText}");

            UI_PopupCombatResult popup = await Managers.UI.ShowAsync<UI_PopupCombatResult>(viewModel);
            
            if (popup == null)
            {
                Debug.LogError("[CombatScene] 팝업 생성 실패! Addressable 주소 'UI/Popup/UI_PopupCombatResult'를 확인하세요.");
            }
            else
            {
                Debug.Log("[CombatScene] 팝업 생성 성공!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CombatScene] 팝업 생성 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }

    private async void ShowTestUI()
    {
        Debug.Log("Test Scene Init() - Player와 NPC를 생성합니다.");


        // 1. Core 프리팹 생성
        GameObject coreGo = await Managers.Resource.InstantiateAsync("Core");
        if (coreGo != null)
        {
            coreGo.transform.position = Vector3.zero; // 생성 후 위치를 (0,0,0)으로 설정
            Debug.Log("[TestScene] Core 생성 완료");
        }
        else
            Debug.LogError("[TestScene] Core 생성 실패 - Addressable 주소 'Core'를 확인하세요");
    }

    void IScene.Clear()
    {
        Debug.Log("Combat Scene Clear()");
    }
}
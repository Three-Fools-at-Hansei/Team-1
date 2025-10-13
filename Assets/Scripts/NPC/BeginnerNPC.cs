using UnityEngine;
using UnityEngine.InputSystem; // InputManager 콜백을 위해 추가

public class BeginnerNPC : MonoBehaviour
{
    private SpeechBubbleViewModel _bubbleVM;
    private DialogueWindowViewModel _dialogeVM;
    private bool _playerInRange = false;

    private async void Start()
    {
        var bubbleUI = await Managers.UI.ShowAsync<UI_SpeechBubble>();
        await System.Threading.Tasks.Task.Yield(); // 안정성을 위해 한 프레임 대기
        _bubbleVM = new SpeechBubbleViewModel();
        if (bubbleUI != null)
        {
            bubbleUI.SetViewModel(_bubbleVM);
            _bubbleVM.Init(transform, "안녕! 나는 초보자 NPC야!");
        }
    }

    // Update() 메서드는 이제 필요 없습니다.
    // private void Update() { }

    private async void OpenDialogue(InputAction.CallbackContext context)
    {
  
        
        _dialogeVM = new DialogueWindowViewModel();
        await Managers.UI.ShowAsync<UI_DialogueWindow>(_dialogeVM);
        _dialogeVM.SetScript("초보자 NPC", new string[]
        {
            "이 세계에 오신 걸 환영해요!",
            "모든 UI는 UIManager를 통해 관리돼요.",
            "MVVM 구조라 View는 멍청하고, 로직은 ViewModel이 맡아요."
        });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            _bubbleVM?.SetMessage("E키로 대화하기");
            // 플레이어가 범위에 들어오면 "Interaction" 액션에 OpenDialogue 함수를 등록
            Managers.Input.BindAction("Player_Interaction", OpenDialogue, InputActionPhase.Performed);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            _bubbleVM?.SetMessage("안녕히 가세요!");
            // 플레이어가 범위를 벗어나면 등록했던 함수를 반드시 해제
            Managers.Input.UnbindAction("Player_Interaction", OpenDialogue, InputActionPhase.Performed);
        }
    }

    private void OnDestroy()
    {
        // NPC가 파괴될 때도 만약을 위해 등록 해제
        if (_playerInRange && Managers.Inst != null)
        {
            Managers.Input.UnbindAction("Interaction", OpenDialogue, InputActionPhase.Performed);
        }
    }
}
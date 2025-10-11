using UnityEngine;
using UnityEngine.InputSystem; // InputManager �ݹ��� ���� �߰�

public class BeginnerNPC : MonoBehaviour
{
    private SpeechBubbleViewModel _bubbleVM;
    private DialogueWindowViewModel _dialogeVM;
    private bool _playerInRange = false;

    private async void Start()
    {
        var bubbleUI = await Managers.UI.ShowAsync<UI_SpeechBubble>();
        await System.Threading.Tasks.Task.Yield(); // �������� ���� �� ������ ���
        _bubbleVM = new SpeechBubbleViewModel();
        if (bubbleUI != null)
        {
            bubbleUI.SetViewModel(_bubbleVM);
            _bubbleVM.Init(transform, "�ȳ�! ���� �ʺ��� NPC��!");
        }
    }

    // Update() �޼���� ���� �ʿ� �����ϴ�.
    // private void Update() { }

    private async void OpenDialogue(InputAction.CallbackContext context)
    {
  
        
        _dialogeVM = new DialogueWindowViewModel();
        await Managers.UI.ShowAsync<UI_DialogueWindow>(_dialogeVM);
        _dialogeVM.SetScript("�ʺ��� NPC", new string[]
        {
            "�� ���迡 ���� �� ȯ���ؿ�!",
            "��� UI�� UIManager�� ���� �����ſ�.",
            "MVVM ������ View�� ��û�ϰ�, ������ ViewModel�� �þƿ�."
        });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            _bubbleVM?.SetMessage("EŰ�� ��ȭ�ϱ�");
            // �÷��̾ ������ ������ "Interaction" �׼ǿ� OpenDialogue �Լ��� ���
            Managers.Input.BindAction("Player_Interaction", OpenDialogue, InputActionPhase.Performed);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            _bubbleVM?.SetMessage("�ȳ��� ������!");
            // �÷��̾ ������ ����� ����ߴ� �Լ��� �ݵ�� ����
            Managers.Input.UnbindAction("Player_Interaction", OpenDialogue, InputActionPhase.Performed);
        }
    }

    private void OnDestroy()
    {
        // NPC�� �ı��� ���� ������ ���� ��� ����
        if (_playerInRange && Managers.Inst != null)
        {
            Managers.Input.UnbindAction("Interaction", OpenDialogue, InputActionPhase.Performed);
        }
    }
}
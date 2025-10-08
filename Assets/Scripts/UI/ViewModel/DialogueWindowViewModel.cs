using System;
using UI;

public class DialogueWindowViewModel : IViewModel
{
    public event Action OnStateChanged;
    public event Action OnCloseRequested;

    public string NpcName { get; private set; } = "�ʺ��� NPC";
    public string[] Lines { get; private set; } = new string[]
    {
        "�ȳ�, ���谡!",
        "�� ������ ���� ������.",
        "������ ���� ���� ��ó�� ������."
    };

    public int Index { get; private set; } = 0;
    public string CurrentLine => (Lines != null && Index >= 0 && Index < Lines.Length) ? Lines[Index] : string.Empty;

    public void SetScript(string npcName, string[] lines)
    {
        NpcName = npcName ?? "NPC";
        Lines = (lines == null || lines.Length == 0) ? new string[] { "" } : lines;
        Index = 0;
        OnStateChanged?.Invoke();
    }

    public void Next()
    {
        if (Lines == null || Lines.Length == 0) { Close(); return; }
        Index++;
        if (Index >= Lines.Length) { Close(); return; }
        OnStateChanged?.Invoke();
    }

    public void Close() => OnCloseRequested?.Invoke();
}

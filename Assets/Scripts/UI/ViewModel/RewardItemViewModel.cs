using System;
public class RewardItemViewModel
{
    public int ID { get; }
    public string Desc { get; }
    public string TypeText { get; }

    private Action<int> _onSelectCallback;

    public RewardItemViewModel(RewardGameData data, Action<int> onSelect)
    {
        ID = data.id;
        Desc = data.description;
        TypeText = data.targetType == "Team" ? "[TEAM]" : "[PERSONAL]";
        _onSelectCallback = onSelect;
    }

    public void Select()
    {
        _onSelectCallback?.Invoke(ID);
    }
}

using System;
using UI;

public enum eCombatResult
{
    Victory,
    Defeat
}

public class CombatResultViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    public eCombatResult Result { get; private set; }
    public string TitleText => Result == eCombatResult.Victory ? "승리" : "패배";
    public string ButtonText => Result == eCombatResult.Victory ? "다음 라운드" : "다시하기";

    public void SetResult(eCombatResult result)
    {
        Result = result;
        OnStateChanged?.Invoke();
    }
}


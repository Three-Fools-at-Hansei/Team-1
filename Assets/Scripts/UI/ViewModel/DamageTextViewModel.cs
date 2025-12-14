using UI;
using UnityEngine;

public class DamageTextViewModel : IViewModel
{
    // 1회성 연출용이므로 상태 변경 이벤트는 사용하지 않음
    public event System.Action OnStateChanged;

    public string DamageText { get; private set; }
    public Color TextColor { get; private set; }

    public DamageTextViewModel(int damage, Color color)
    {
        // Utils.FormatNumber 등을 활용할 수도 있음
        DamageText = damage.ToString();
        TextColor = color;
    }
}
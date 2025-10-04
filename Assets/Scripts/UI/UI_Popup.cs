using UI;
using UnityEngine;

/// <summary>
/// Popup 용도의 UI임을 명시하고, ActionMapKey을 가져요.
/// </summary>
public abstract class UI_Popup : UI_View
{
    public virtual string ActionMapKey => "None";
}

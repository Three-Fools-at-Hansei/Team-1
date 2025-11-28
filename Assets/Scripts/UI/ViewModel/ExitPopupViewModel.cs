using System;
using UI;

/// <summary>
/// Exit 팝업의 ViewModel입니다.
/// 종료 확인 메시지를 관리합니다.
/// </summary>
public class ExitPopupViewModel : IViewModel
{
    public event Action OnStateChanged;

    public string MessageText { get; private set; } = "정말 종료하시겠습니까?";
}




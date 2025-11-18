using System;
using UI;

public class HelloPopupViewModel : IViewModel
{
    public event Action OnStateChanged;

    public string Title { get; private set; } = "안녕하세요";
}

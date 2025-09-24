using System;

namespace UI
{
    public interface IViewModel
    {
        /// <summary>
        /// ViewModel�� ���°� ����Ǿ��� �� View�� �����ϱ� ���� �̺�Ʈ�Դϴ�.
        /// View�� �� �̺�Ʈ�� �����Ͽ� UI�� �����մϴ�.
        /// </summary>
        event Action OnStateChanged;
    }
}
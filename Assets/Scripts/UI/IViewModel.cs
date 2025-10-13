using System;

namespace UI
{
    public interface IViewModel
    {
        /// <summary>
        /// ViewModel의 상태가 변경되었을 때 View에 통지하기 위한 이벤트입니다.
        /// View는 이 이벤트를 구독하여 UI를 갱신합니다.
        /// </summary>
        event Action OnStateChanged;
    }
}
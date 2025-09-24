using System.Threading.Tasks;

/// <summary>
/// UI 연출 클래스가 구현해야 할 인터페이스입니다.
/// 모든 연출 클래스는 기본적인 등장(Enter)과 퇴장(Exit)을 반드시 구현해야 합니다.
/// </summary>
public interface IUIAnimation
{
    /// <summary>
    /// UI가 화면에 나타나는 등장 연출을 비동기적으로 재생합니다.
    /// </summary>
    Task EnterAsync();

    /// <summary>
    /// UI가 화면에서 사라지는 퇴장 연출을 비동기적으로 재생합니다.
    /// </summary>
    Task ExitAsync();
}
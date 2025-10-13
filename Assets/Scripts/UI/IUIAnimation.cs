using System.Threading.Tasks;

/// <summary>
/// UI ���� Ŭ������ �����ؾ� �� �������̽��Դϴ�.
/// ��� ���� Ŭ������ �⺻���� ����(Enter)�� ����(Exit)�� �ݵ�� �����ؾ� �մϴ�.
/// </summary>
public interface IUIAnimation
{
    /// <summary>
    /// UI�� ȭ�鿡 ��Ÿ���� ���� ������ �񵿱������� ����մϴ�.
    /// </summary>
    Task EnterAsync();

    /// <summary>
    /// UI�� ȭ�鿡�� ������� ���� ������ �񵿱������� ����մϴ�.
    /// </summary>
    Task ExitAsync();
}
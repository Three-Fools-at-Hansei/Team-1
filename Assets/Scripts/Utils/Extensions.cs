using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// ������Ʈ�� �����ɴϴ�. ���� �ش� ������Ʈ�� ���ٸ� �߰��� �� ��ȯ�մϴ�.
    /// </summary>
    /// <typeparam name="T">������ ������Ʈ�� Ÿ���Դϴ�.</typeparam>
    /// <param name="go">������Ʈ�� ������ ������Ʈ�Դϴ�.</param>
    /// <returns>�ش� Ÿ���� ������Ʈ�� ��ȯ�մϴ�.</returns>
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Utils.GetOrAddComponent<T>(go);
    }
}
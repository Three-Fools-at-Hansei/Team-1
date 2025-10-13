using UnityEngine;

public class Utils
{
    /// <summary>
    /// ������Ʈ�� �����ɴϴ�. ���� �ش� ������Ʈ�� ���ٸ� �߰��� �� ��ȯ�մϴ�.
    /// </summary>
    /// <typeparam name="T">������ ������Ʈ�� Ÿ���Դϴ�.</typeparam>
    /// <param name="go">������Ʈ�� ������ ������Ʈ�Դϴ�.</param>
    /// <returns>�ش� Ÿ���� ������Ʈ�� ��ȯ�մϴ�.</returns>
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        if (go == null)
        {
            Debug.LogError("GetOrAddComponent ����: ���� ������Ʈ�� null�Դϴ�.");
            return null;
        }

        if (!go.TryGetComponent<T>(out T component))
        {
            component = go.AddComponent<T>();
            Debug.Log($"GetOrAddComponent: [{go.name}] {component.GetType()} ������Ʈ�� ���� �����մϴ�.");
        }

        return component;
    }

    /// <summary>
    ///     �θ�-�ڽ� ���迡�� �θ� ������Ʈ�� ����ؼ� �ڽ� ������Ʈ�� Ž��
    /// </summary>
    /// <typeparam name="T">
    ///     ã�� ������Ʈ�� Ÿ�� 
    /// </typeparam>
    /// <param name="go">
    ///     �ڽ� ������Ʈ�� ��ȸ�� �θ� ������Ʈ
    /// </param>
    /// <param name="name">
    ///     ������Ʈ�� �̸����� ã��. �Է����� ���� �� Ÿ�����θ� ã�Ƽ� ��ȯ
    /// </param>
    /// <param name="recursive">
    ///     ��������� Ž���� ���ΰ�? false �� ��������� Ž������ ����.
    /// </param>
    /// <returns>
    ///     ���ǿ� ���� �ڽ� ������Ʈ�� ã�Ҵٸ�, �� ������Ʈ�� ��ȯ�Ѵ�.
    ///     ã�� ���ߴٸ�, null �� ��ȯ�Ѵ�.
    /// </returns>
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false)
        where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        // ��������� Ž������ ����.
        if (!recursive)
        {
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                Transform transform = go.transform.GetChild(i);

                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        // ��������� Ž����.
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }


        return null;
    }

    /// <summary>
    ///     �θ�-�ڽ� ���迡�� �θ� ������Ʈ�� ����ؼ� �ڽ� ������Ʈ�� Ž��
    /// </summary>
    /// <param name="go">
    ///     �ڽ� ������Ʈ�� ��ȸ�� �θ� ������Ʈ
    /// </param>
    /// <param name="name">
    ///     ������Ʈ�� �̸����� ã��. �Է����� ���� �� Ÿ�����θ� ã�Ƽ� ��ȯ
    /// </param>
    /// <param name="recursive">
    ///     ��������� Ž���� ���ΰ�? false �� ��������� Ž������ ����.
    /// </param>
    /// <returns>
    ///     ���ǿ� ���� �ڽ� ������Ʈ�� ã�Ҵٸ�, �� ������Ʈ�� ��ȯ�Ѵ�.
    ///     ã�� ���ߴٸ�, null �� ��ȯ�Ѵ�.
    /// </returns>
    public static GameObject FindChildObject(GameObject go, string name = null, bool recursive = false)
    {
        Transform tr = FindChild<Transform>(go, name, recursive);
        return tr == null ? null : tr.gameObject;
    }
}

using UnityEngine;

public class Utils
{
    /// <summary>
    /// 컴포넌트를 가져옵니다. 만약 해당 컴포넌트가 없다면 추가한 후 반환합니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트의 타입입니다.</typeparam>
    /// <param name="go">컴포넌트를 가져올 오브젝트입니다.</param>
    /// <returns>해당 타입의 컴포넌트를 반환합니다.</returns>
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        if (go == null)
        {
            Debug.LogError("GetOrAddComponent 오류: 게임 오브젝트가 null입니다.");
            return null;
        }

        if (!go.TryGetComponent<T>(out T component))
        {
            component = go.AddComponent<T>();
            Debug.Log($"GetOrAddComponent: [{go.name}] {component.GetType()} 컴포넌트가 없어 생성합니다.");
        }

        return component;
    }

    /// <summary>
    ///     부모-자식 관계에서 부모 오브젝트를 사용해서 자식 오브젝트를 탐색
    /// </summary>
    /// <typeparam name="T">
    ///     찾을 오브젝트의 타입 
    /// </typeparam>
    /// <param name="go">
    ///     자식 오브젝트를 순회할 부모 오브젝트
    /// </param>
    /// <param name="name">
    ///     오브젝트를 이름으로 찾기. 입력하지 않을 시 타입으로만 찾아서 반환
    /// </param>
    /// <param name="recursive">
    ///     재귀적으로 탐색할 것인가? false 시 재귀적으로 탐색하지 않음.
    /// </param>
    /// <returns>
    ///     조건에 맞은 자식 오브젝트를 찾았다면, 그 오브젝트를 반환한다.
    ///     찾지 못했다면, null 을 반환한다.
    /// </returns>
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false)
        where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        // 재귀적으로 탐색하지 않음.
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
        // 재귀적으로 탐색함.
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
    ///     부모-자식 관계에서 부모 오브젝트를 사용해서 자식 오브젝트를 탐색
    /// </summary>
    /// <param name="go">
    ///     자식 오브젝트를 순회할 부모 오브젝트
    /// </param>
    /// <param name="name">
    ///     오브젝트를 이름으로 찾기. 입력하지 않을 시 타입으로만 찾아서 반환
    /// </param>
    /// <param name="recursive">
    ///     재귀적으로 탐색할 것인가? false 시 재귀적으로 탐색하지 않음.
    /// </param>
    /// <returns>
    ///     조건에 맞은 자식 오브젝트를 찾았다면, 그 오브젝트를 반환한다.
    ///     찾지 못했다면, null 을 반환한다.
    /// </returns>
    public static GameObject FindChildObject(GameObject go, string name = null, bool recursive = false)
    {
        Transform tr = FindChild<Transform>(go, name, recursive);
        return tr == null ? null : tr.gameObject;
    }
}

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

    /// <summary>
    /// 숫자를 K(천), M(백만) 단위로 축약하여 포매팅된 문자열로 반환합니다.
    /// 10,000 미만: 그대로 표시 (예: 9999)
    /// 10,000 이상 1,000,000 미만: K 단위, 소수점 첫째 자리까지 표시 (예: 10.5K, 999.9K)
    /// 1,000,000 이상: M 단위, 소수점 첫째 자리까지 표시 (예: 1.2M, 123.4M)
    /// </summary>
    /// <param name="number">포매팅할 정수 값입니다.</param>
    /// <returns>K 또는 M 단위로 포매팅된 문자열입니다.</returns>
    public static string FormatNumber(int number)
    {
        if (number >= 1_000_000)
            return $"{(number / 1_000_000.0f):0.#} M";
        else if (number >= 10_000)
            return $"{(number / 1_000.0f):0.#} K";
        else
            return number.ToString();
    }
}

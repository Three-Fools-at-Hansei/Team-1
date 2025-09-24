using UnityEngine;

public static class Extensions
{
    /// <summary>
    /// 컴포넌트를 가져옵니다. 만약 해당 컴포넌트가 없다면 추가한 후 반환합니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트의 타입입니다.</typeparam>
    /// <param name="go">컴포넌트를 가져올 오브젝트입니다.</param>
    /// <returns>해당 타입의 컴포넌트를 반환합니다.</returns>
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Utils.GetOrAddComponent<T>(go);
    }
}
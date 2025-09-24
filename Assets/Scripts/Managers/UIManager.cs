using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.UI;

    private readonly Stack<UI_Popup> _popupStack = new();
    private Transform _sceneRoot;
    private Transform _dontDestroyRoot;

    /// <summary>
    /// UI Popup에 순차적으로 부여될 Sorting Order 값입니다.
    /// </summary>
    private int _sortingOrder = 50;

    public void Init()
    {
        GameObject dontDestroyGo = GameObject.Find("@UI_Root_DontDestroy") ?? new GameObject { name = "@UI_Root_DontDestroy" };
        Object.DontDestroyOnLoad(dontDestroyGo);
        _dontDestroyRoot = dontDestroyGo.transform;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject { name = "@EventSystem" };
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
            Object.DontDestroyOnLoad(eventSystemGo);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        _popupStack.Clear();
        _sortingOrder = 10;
        _sceneRoot = null;
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    /// <summary>
    /// 지정된 타입의 UI_View를 비동기적으로 로드하고 반환합니다.
    /// ResourceManagerEx를 통해 Object Pooling을 자동으로 활용합니다.
    /// </summary>
    /// <typeparam name="T">생성할 UI의 타입이며, UI_View를 상속해야 합니다.</typeparam>
    /// <param name="parent">UI가 위치할 부모 Transform입니다. null일 경우 타입에 따라 자동으로 Root가 결정됩니다.</param>
    /// <returns>생성된 UI의 인스턴스입니다.</returns>
    public async Task<T> ShowAsync<T>(Transform parent = null) where T : UI_View
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        // ResourceManagerEx를 통해 프리팹을 비동기 로드 및 풀링합니다.
        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent);
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // 부모 Transform 설정
        // parent가 null로 전달된 경우에만 UI 타입에 따라 기본 부모(SceneRoot)를 설정하는 로직을 수행합니다.
        if (parent == null)
        {
            // GetSceneRoot()가 반환하는 Transform으로 부모를 재설정합니다.
            view.transform.SetParent(GetSceneRoot(), false);

            if (view is UI_Popup)
                _popupStack.Push(view as UI_Popup);
        }

        // RectTransform 초기화
        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        // Canvas 및 Sorting Order 설정
        SetCanvas(go, view is UI_Popup);

        view.gameObject.SetActive(false); // 연출 시작 전 비활성화
        return view;
    }

    /// <summary>
    /// 씬이 전환되어도 파괴되지 않는 UI_View를 비동기적으로 로드하고 반환합니다.
    /// 이 UI는 Popup Stack으로 관리되지 않으며, 항상 최상단에 표시됩니다.
    /// </summary>
    /// <typeparam name="T">생성할 UI의 타입이며, UI_View를 상속해야 합니다.</typeparam>
    /// <param name="parent">UI가 위치할 부모 Transform입니다. null일 경우 DontDestroyRoot가 기본값으로 사용됩니다.</param>
    /// <returns>생성된 UI의 인스턴스입니다.</returns>
    public async Task<T> ShowDontDestroyAsync<T>(Transform parent = null) where T : UI_View
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        // ResourceManagerEx를 통해 프리팹을 비동기 로드 및 풀링합니다.
        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent ?? _dontDestroyRoot);
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // RectTransform 초기화
        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        // Canvas 및 Sorting Order 설정
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999999;

        return view;
    }


    /// <summary>
    /// 지정된 UI_View를 닫고 Pool에 반환합니다.
    /// 팝업의 경우, 스택의 최상단에 있을 때만 닫을 수 있습니다.
    /// </summary>
    /// <param name="view">닫을 UI_View 인스턴스입니다.</param>
    public void Close(UI_View view)
    {
        if (view == null) return;

        if (view is UI_Popup popup)
        {
            if (_popupStack.Count == 0 || _popupStack.Peek() != popup)
            {
                Debug.LogError($"[UIManager] 닫으려는 팝업({popup.name})이 스택의 최상단에 없습니다.");
                return;
            }
            _popupStack.Pop();
            _sortingOrder--;
        }

        // ResourceManagerEx를 통해 오브젝트를 풀에 반환
        Managers.Resource.Destroy(view.gameObject);
    }

    /// <summary>
    /// UI GameObject에 Canvas 컴포넌트를 설정하고 Sorting Order를 지정합니다.
    /// </summary>
    private void SetCanvas(GameObject go, bool useSortingOrder)
    {
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = useSortingOrder ? _sortingOrder++ : 0;
    }

    /// <summary>
    /// 현재 씬의 UI Root Transform을 반환합니다. 없으면 생성합니다.
    /// </summary>
    private Transform GetSceneRoot()
    {
        if (_sceneRoot == null)
        {
            GameObject rootGo = GameObject.Find("@UI_Root_Scene") ?? new GameObject { name = "@UI_Root_Scene" };
            _sceneRoot = rootGo.transform;
        }
        return _sceneRoot;
    }

    /// <summary>
    /// UI 타입에 따라 프리팹 경로를 결정합니다.
    /// </summary>
    private string GetPrefabPath<T>(string prefabName) where T : UI_View
    {
        string folder = typeof(UI_Popup).IsAssignableFrom(typeof(T)) ? "Popup" : "View";
        return $"UI/{folder}/{prefabName}";
    }

    /// <summary>
    /// 새로운 씬이 로드될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Clear();

        // 나중에 씬 마다 필요한 동작이 있다면 추가하면 좋겠죠?
    }
}
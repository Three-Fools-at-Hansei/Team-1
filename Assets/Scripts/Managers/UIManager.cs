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
    /// UI Popup�� ���������� �ο��� Sorting Order ���Դϴ�.
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

        Debug.Log($"{ManagerType} Manager Init �մϴ�.");
    }

    public void Update() { }

    public void Clear()
    {
        _popupStack.Clear();
        _sortingOrder = 10;
        _sceneRoot = null;
        Debug.Log($"{ManagerType} Manager Clear �մϴ�.");
    }

    /// <summary>
    /// ������ Ÿ���� UI_View�� �񵿱������� �ε��ϰ� ��ȯ�մϴ�.
    /// ResourceManagerEx�� ���� Object Pooling�� �ڵ����� Ȱ���մϴ�.
    /// </summary>
    /// <typeparam name="T">������ UI�� Ÿ���̸�, UI_View�� ����ؾ� �մϴ�.</typeparam>
    /// <param name="parent">UI�� ��ġ�� �θ� Transform�Դϴ�. null�� ��� Ÿ�Կ� ���� �ڵ����� Root�� �����˴ϴ�.</param>
    /// <returns>������ UI�� �ν��Ͻ��Դϴ�.</returns>
    public async Task<T> ShowAsync<T>(Transform parent = null) where T : UI_View
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        // ResourceManagerEx�� ���� �������� �񵿱� �ε� �� Ǯ���մϴ�.
        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent);
        if (go == null)
        {
            Debug.LogError($"[UIManager] ������ �ε� ����. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // �θ� Transform ����
        // parent�� null�� ���޵� ��쿡�� UI Ÿ�Կ� ���� �⺻ �θ�(SceneRoot)�� �����ϴ� ������ �����մϴ�.
        if (parent == null)
        {
            // GetSceneRoot()�� ��ȯ�ϴ� Transform���� �θ� �缳���մϴ�.
            view.transform.SetParent(GetSceneRoot(), false);

            if (view is UI_Popup)
                _popupStack.Push(view as UI_Popup);
        }

        // RectTransform �ʱ�ȭ
        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        // Canvas �� Sorting Order ����
        SetCanvas(go, view is UI_Popup);

        view.gameObject.SetActive(false); // ���� ���� �� ��Ȱ��ȭ
        return view;
    }

    /// <summary>
    /// ���� ��ȯ�Ǿ �ı����� �ʴ� UI_View�� �񵿱������� �ε��ϰ� ��ȯ�մϴ�.
    /// �� UI�� Popup Stack���� �������� ������, �׻� �ֻ�ܿ� ǥ�õ˴ϴ�.
    /// </summary>
    /// <typeparam name="T">������ UI�� Ÿ���̸�, UI_View�� ����ؾ� �մϴ�.</typeparam>
    /// <param name="parent">UI�� ��ġ�� �θ� Transform�Դϴ�. null�� ��� DontDestroyRoot�� �⺻������ ���˴ϴ�.</param>
    /// <returns>������ UI�� �ν��Ͻ��Դϴ�.</returns>
    public async Task<T> ShowDontDestroyAsync<T>(Transform parent = null) where T : UI_View
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        // ResourceManagerEx�� ���� �������� �񵿱� �ε� �� Ǯ���մϴ�.
        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent ?? _dontDestroyRoot);
        if (go == null)
        {
            Debug.LogError($"[UIManager] ������ �ε� ����. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // RectTransform �ʱ�ȭ
        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        // Canvas �� Sorting Order ����
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999999;

        return view;
    }


    /// <summary>
    /// ������ UI_View�� �ݰ� Pool�� ��ȯ�մϴ�.
    /// �˾��� ���, ������ �ֻ�ܿ� ���� ���� ���� �� �ֽ��ϴ�.
    /// </summary>
    /// <param name="view">���� UI_View �ν��Ͻ��Դϴ�.</param>
    public void Close(UI_View view)
    {
        if (view == null) return;

        if (view is UI_Popup popup)
        {
            if (_popupStack.Count == 0 || _popupStack.Peek() != popup)
            {
                Debug.LogError($"[UIManager] �������� �˾�({popup.name})�� ������ �ֻ�ܿ� �����ϴ�.");
                return;
            }
            _popupStack.Pop();
            _sortingOrder--;
        }

        // ResourceManagerEx�� ���� ������Ʈ�� Ǯ�� ��ȯ
        Managers.Resource.Destroy(view.gameObject);
    }

    /// <summary>
    /// UI GameObject�� Canvas ������Ʈ�� �����ϰ� Sorting Order�� �����մϴ�.
    /// </summary>
    private void SetCanvas(GameObject go, bool useSortingOrder)
    {
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = useSortingOrder ? _sortingOrder++ : 0;
    }

    /// <summary>
    /// ���� ���� UI Root Transform�� ��ȯ�մϴ�. ������ �����մϴ�.
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
    /// UI Ÿ�Կ� ���� ������ ��θ� �����մϴ�.
    /// </summary>
    private string GetPrefabPath<T>(string prefabName) where T : UI_View
    {
        string folder = typeof(UI_Popup).IsAssignableFrom(typeof(T)) ? "Popup" : "View";
        return $"UI/{folder}/{prefabName}";
    }

    /// <summary>
    /// ���ο� ���� �ε�� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Clear();

        // ���߿� �� ���� �ʿ��� ������ �ִٸ� �߰��ϸ� ������?
    }
}
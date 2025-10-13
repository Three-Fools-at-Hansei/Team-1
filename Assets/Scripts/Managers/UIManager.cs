using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
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

    /// <summary>
    /// Sorting Group ���� order ����
    /// </summary>
    private const int ORDER_STEP = 10;
    private string _lastActionMap = "None";
    public void Init()
    {
        GameObject dontDestroyGo = GameObject.Find("@UI_Root_DontDestroy") ?? new GameObject { name = "@UI_Root_DontDestroy" };
        Object.DontDestroyOnLoad(dontDestroyGo);
        _dontDestroyRoot = dontDestroyGo.transform;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject { name = "@EventSystem" };
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            Object.DontDestroyOnLoad(eventSystemGo);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"{ManagerType} Manager Init �մϴ�.");
    }

    public void Update() { }

    public void Clear()
    {
        _lastActionMap = "None";
        _popupStack.Clear();
        _sortingOrder = 10;
        _sceneRoot = null;
        Debug.Log($"{ManagerType} Manager Clear �մϴ�.");
    }

    /// <summary>
    /// ������ Ÿ���� UI_View�� �񵿱������� �ε��ϰ�, ������ ViewModel�� �����մϴ�.
    /// </summary>
    /// <typeparam name="TView">������ UI�� Ÿ���̸�, UI_View�� ����ؾ� �մϴ�.</typeparam>
    /// <param name="viewModel">UI�� ������ ViewModel �ν��Ͻ��Դϴ�.</param>
    /// <param name="parent">UI�� ��ġ�� �θ� Transform�Դϴ�. null�� ��� Ÿ�Կ� ���� �ڵ����� Root�� �����˴ϴ�.</param>
    /// <returns>���� �� �ʱ�ȭ�� �Ϸ�� UI�� �ν��Ͻ��Դϴ�.</returns>
    public async Task<TView> ShowAsync<TView>(IViewModel viewModel, Transform parent = null) where TView : UI_View
    {
        // �θ� ��õ��� ���� ���, ���� ���� UI ��Ʈ�� ����մϴ�.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager] ������ �ε� ����. path: {path}");
            return null;
        }

        TView view = go.GetOrAddComponent<TView>();

        // parent�� null�� ���� ���ÿ� Push�ϵ��� ������ ��Ȯȭ�մϴ�.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            _lastActionMap = Managers.Input.CurrentActionMapKey;
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group�� ������ �����մϴ�.
        SetSortingGroupOrder(go, view is UI_Popup);

        // �Է¹��� ����� �����մϴ�.
        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
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
        // �θ� ��õ��� ���� ���, ���� ���� UI ��Ʈ�� ����մϴ�.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager] ������ �ε� ����. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // parent�� null�� ���� ���ÿ� Push�ϵ��� ������ ��Ȯȭ�մϴ�.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            _lastActionMap = Managers.Input.CurrentActionMapKey;
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group�� ������ �����մϴ�.
        SetSortingGroupOrder(go, view is UI_Popup);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// ���� ��ȯ�Ǿ �ı����� �ʴ� UI_DontDestroyPopup�� �񵿱������� �ε��ϰ� ViewModel�� �����մϴ�.
    /// </summary>
    /// <typeparam name="TView">������ UI�� Ÿ���̸�, UI_DontDestroyPopup�� ����ؾ� �մϴ�.</typeparam>
    /// <param name="viewModel">UI�� ������ ViewModel �ν��Ͻ��Դϴ�.</param>
    /// <param name="parent">UI�� ��ġ�� �θ� Transform�Դϴ�. null�� ��� DontDestroyRoot�� �⺻������ ���˴ϴ�.</param>
    /// <returns>������ UI�� �ν��Ͻ��Դϴ�.</returns>
    public async Task<TView> ShowDontDestroyAsync<TView>(IViewModel viewModel, Transform parent = null) where TView : UI_DontDestroyPopup
    {
        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent != null ? parent : _dontDestroyRoot);
        if (go == null)
        {
            Debug.LogError($"[UIManager] ������ �ε� ����. path: {path}");
            return null;
        }

        TView view = go.GetOrAddComponent<TView>();

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999999;

        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// ���� ��ȯ�Ǿ �ı����� �ʴ� UI_DontDestroyPopup�� �񵿱������� �ε��ϰ� ��ȯ�մϴ�.
    /// �� UI�� Popup Stack���� �������� ������, �׻� �ֻ�ܿ� ǥ�õ˴ϴ�.
    /// </summary>
    /// <typeparam name="T">������ UI�� Ÿ���̸�, UI_DontDestroyPopup�� ����ؾ� �մϴ�.</typeparam>
    /// <param name="parent">UI�� ��ġ�� �θ� Transform�Դϴ�. null�� ��� DontDestroyRoot�� �⺻������ ���˴ϴ�.</param>
    /// <returns>������ UI�� �ν��Ͻ��Դϴ�.</returns>
    public async Task<T> ShowDontDestroyAsync<T>(Transform parent = null) where T : UI_DontDestroyPopup
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent != null ? parent : _dontDestroyRoot);
        if (go == null)
        {
            Debug.LogError($"[UIManager] ������ �ε� ����. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999999;

        view.gameObject.SetActive(true);
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

        if (view is UI_Popup popup && view is not UI_DontDestroyPopup)
        {
            if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
            {
                _popupStack.Pop();
                _sortingOrder -= ORDER_STEP;

                // ���� ��� Popup�� ActionMapKey ����
                if (_popupStack.Count > 0)
                {
                    var nextPopup = _popupStack.Peek();
                    Managers.Input.SwitchActionMap(nextPopup.ActionMapKey);
                }
                // ������ �� ��� �⺻ ����("None")
                else
                {
                    Managers.Input.SwitchActionMap(_lastActionMap);
                    _lastActionMap = "None";
                }
            }
        }

        Managers.Resource.Destroy(view.gameObject);
    }

    /// <summary>
    /// UI GameObject�� SortingGroup ������Ʈ�� �����ϰ� Sorting Order�� �����մϴ�.
    /// </summary>
    private void SetSortingGroupOrder(GameObject go, bool useSortingOrder)
    {
        SortingGroup sortingGroup = go.GetOrAddComponent<SortingGroup>();
        if (useSortingOrder)
        {
            _sortingOrder += ORDER_STEP;
            sortingGroup.sortingOrder = _sortingOrder;
        }
        else
        {
            // sortingOrder�� ������� ���� ���(UI_Popup�� ����� ���) sortingOrder�� ������� ����
            sortingGroup.sortingOrder = 0;
        }
    }

    /// <summary>
    /// ���� ���� UI Root Transform�� ��ȯ�մϴ�. ������ �����մϴ�.
    /// </summary>
    private Transform GetSceneRoot()
    {
        if (_sceneRoot == null)
        {
            GameObject rootGo = GameObject.Find("@UI_Root_Scene");
            
            //���� UI ��Ʈ�� ���� ���, Canvas�� �ʼ� ������Ʈ�� �����Ͽ� ���� �����մϴ�.
            if (rootGo == null)
            {
                rootGo = new GameObject { name = "@UI_Root_Scene" };
                rootGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                rootGo.AddComponent<UnityEngine.UI.CanvasScaler>();
                rootGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
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
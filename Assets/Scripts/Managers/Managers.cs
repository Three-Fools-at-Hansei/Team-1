using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Inst { get; private set; }

    // inst�� ���� ���� �ܿ��� ���� ������ �� �ֵ��� ������Ƽ�� �߰������.
    public static SceneManagerEx Scene { get; private set; }
    public static UIManager UI { get; private set; }
    public static DataManager Data { get; private set; }
    public static PoolManager Pool { get; private set; }
    public static CameraManager Camera { get; private set; }
    public static InputManager Input { get; private set; }
    public static SoundManager Sound { get; private set; }
    public static ResourceManagerEx Resource { get; private set; }

    private readonly IManagerBase[] _managers = new IManagerBase[(int)eManagerType.End];

    private void Awake()
    {
        if (Inst != null)
        {
            Debug.LogWarning("�ߺ��� Managers �ν��Ͻ��� �����Ǿ� �����մϴ�.");
            Destroy(gameObject);
            return;
        }

        Inst = this;
        DontDestroyOnLoad(gameObject);

        // �Ŵ������� �����ϰ� �ʱ�ȭ
        Init();
    }

    private void Init()
    {
        Scene = new SceneManagerEx();
        _managers[(int)eManagerType.Scene] = Scene;

        UI = new UIManager();
        _managers[(int)eManagerType.UI] = UI;
        
        Data = new DataManager();
        _managers[(int)eManagerType.Data] = Data;
        
        Pool = new PoolManager();
        _managers[(int)eManagerType.Pool] = Pool;

        Camera = new CameraManager();
        _managers[(int)eManagerType.Camera] = Camera;

        Input = new InputManager();
        _managers[(int)eManagerType.Input] = Input;
        
        Sound = new SoundManager();
        _managers[(int)eManagerType.Sound] = Sound;
        
        Resource = new ResourceManagerEx();
        _managers[(int)eManagerType.Resource] = Resource;

        // ��� �Ŵ����� Init() ȣ��
        foreach (IManagerBase manager in _managers)
            manager?.Init();

        Debug.Log("��� �Ŵ��� �ʱ�ȭ �Ϸ�.");
    }

    private void Update()
    {
        // ��� �Ŵ����� Update() ȣ��
        foreach (IManagerBase manager in _managers)
            manager?.Update();
    }

    /// <summary>
    /// ��� ���� �Ŵ������� ���¸� �ʱ�ȭ�մϴ�.
    /// </summary>
    public void Clear()
    {
        // ��� �Ŵ����� Clear() ȣ��
        foreach (IManagerBase manager in _managers)
            manager?.Clear();
    }
}
using System.Threading.Tasks;
using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers Inst { get; private set; }

    // inst를 통한 접근 외에도 쉽게 접근할 수 있도록 프로퍼티를 추가헀어요.
    public static SceneManagerEx Scene { get; private set; }
    public static UIManager UI { get; private set; }
    public static DataManager Data { get; private set; }
    public static PoolManager Pool { get; private set; }
    public static CameraManager Camera { get; private set; }
    public static InputManager Input { get; private set; }
    public static SoundManager Sound { get; private set; }
    public static ResourceManagerEx Resource { get; private set; }
    public static GameSystemManager GameSystem { get; private set; }
    public static NetworkManagerEx Network { get; private set; }
    
    private readonly IManagerBase[] _managers = new IManagerBase[(int)eManagerType.End];

    private void Awake()
    {
        if (Inst != null)
        {
            Debug.LogWarning("중복된 Managers 인스턴스가 생성되어 제거합니다.");
            Destroy(gameObject);
            return;
        }

        Inst = this;
        DontDestroyOnLoad(gameObject);

        // 매니저들을 생성하고 초기화
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

        GameSystem = new GameSystemManager();
        _managers[(int)eManagerType.GameSystem] = GameSystem;

        Network = new NetworkManagerEx();
        _managers[(int)eManagerType.Network] = Network;

        // 모든 매니저에 Init() 호출
        foreach (IManagerBase manager in _managers)
            manager?.Init();

        Debug.Log("모든 매니저 초기화 완료.");
    }

    private void Update()
    {
        // 모든 매니저에 Update() 호출
        foreach (IManagerBase manager in _managers)
            manager?.Update();
    }

    /// <summary>
    /// 모든 하위 매니저들의 상태를 초기화합니다.
    /// </summary>
    public void Clear()
    {
        // 모든 매니저에 Clear() 호출
        foreach (IManagerBase manager in _managers)
            manager?.Clear();
    }
}
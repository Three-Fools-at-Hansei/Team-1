//using UnityEngine;

//public class TestScene : MonoBehaviour, IScene
//{
//    eSceneType IScene.SceneType => eSceneType.Test;

//    void Awake()
//    {
//        Managers.Scene.SetCurrentScene(this);
//        Debug.Log("Test Scene Awake() 합니다.");
//    }

//    void Start()
//    {
//        ((IScene)this).Init();
//    }

//    void IScene.Init()
//    {
//        Debug.Log("Test Scene Init() 합니다.");
//        // ViewModel을 먼저 생성하고 UI 생성을 요청합니다.
//        var viewModel = new PopupTestViewModel();
//        _ = Managers.UI.ShowAsync<UI_PopupTest>(viewModel);
//    }

//    void IScene.Clear()
//    {
//        Debug.Log("Test Scene Clear() 합니다.");
//    }
//}

using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Test;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
    }

    // 씬이 시작되면 Init()을 호출해서 준비된 프리펩들을 생성합니다.
    void Start()
    {
        // SceneManagerEx가 Init을 호출해주지만, 테스트를 위해 직접 호출합니다.
        ((IScene)this).Init();
    }

    // async 키워드가 필요합니다. (await 사용 때문)
    async void IScene.Init()
    {
        Debug.Log("Test Scene Init() - Player와 NPC를 생성합니다.");

        Managers.Input.SwitchActionMap("Lobby");

        // 규정 #3, #7 준수: Manager를 통해 어드레스블 주소로 프리팹을 생성합니다.

        // 1. "Player" 주소를 가진 Player 프리팹 생성
        GameObject playerGo = await Managers.Resource.InstantiateAsync("Player");
        if (playerGo != null)
        {
            playerGo.transform.position = Vector3.zero; // 생성 후 위치를 (0,0,0)으로 설정
        }

        // 2. "BeginnerNPC" 주소를 가진 NPC 프리팹 생성
        // 생성과 동시에 위치를 (3,0,0)으로 설정
        await Managers.Resource.InstantiateAsync("BeginnerNPC", new Vector3(3, 0, 0));

        await Managers.UI.ShowAsync<UI_PopupHello>(new HelloPopupViewModel());
    }

    void IScene.Clear()
    {
        Debug.Log("Test Scene Clear() 합니다.");
    }
}
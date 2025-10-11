//using UnityEngine;

//public class TestScene : MonoBehaviour, IScene
//{
//    eSceneType IScene.SceneType => eSceneType.Test;

//    void Awake()
//    {
//        Managers.Scene.SetCurrentScene(this);
//        Debug.Log("Test Scene Awake() �մϴ�.");
//    }

//    void Start()
//    {
//        ((IScene)this).Init();
//    }

//    void IScene.Init()
//    {
//        Debug.Log("Test Scene Init() �մϴ�.");
//        // ViewModel�� ���� �����ϰ� UI ������ ��û�մϴ�.
//        var viewModel = new PopupTestViewModel();
//        _ = Managers.UI.ShowAsync<UI_PopupTest>(viewModel);
//    }

//    void IScene.Clear()
//    {
//        Debug.Log("Test Scene Clear() �մϴ�.");
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

    // ���� ���۵Ǹ� Init()�� ȣ���ؼ� �غ�� ��������� �����մϴ�.
    void Start()
    {
        // SceneManagerEx�� Init�� ȣ����������, �׽�Ʈ�� ���� ���� ȣ���մϴ�.
        ((IScene)this).Init();
    }

    // async Ű���尡 �ʿ��մϴ�. (await ��� ����)
    async void IScene.Init()
    {
        Debug.Log("Test Scene Init() - Player�� NPC�� �����մϴ�.");

        Managers.Input.SwitchActionMap("Lobby");

        // ���� #3, #7 �ؼ�: Manager�� ���� ��巹���� �ּҷ� �������� �����մϴ�.

        // 1. "Player" �ּҸ� ���� Player ������ ����
        GameObject playerGo = await Managers.Resource.InstantiateAsync("Player");
        if (playerGo != null)
        {
            playerGo.transform.position = Vector3.zero; // ���� �� ��ġ�� (0,0,0)���� ����
        }

        // 2. "BeginnerNPC" �ּҸ� ���� NPC ������ ����
        // ������ ���ÿ� ��ġ�� (3,0,0)���� ����
        await Managers.Resource.InstantiateAsync("BeginnerNPC", new Vector3(3, 0, 0));
    }

    void IScene.Clear()
    {
        Debug.Log("Test Scene Clear() �մϴ�.");
    }
}
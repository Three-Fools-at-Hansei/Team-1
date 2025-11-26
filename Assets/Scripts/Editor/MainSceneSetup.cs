using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// MainScene에 필요한 GameObject들을 자동으로 추가하는 에디터 스크립트입니다.
/// Unity 메뉴에서 "Tools/Setup MainScene"을 선택하여 사용합니다.
/// </summary>
public class MainSceneSetup
{
    [MenuItem("Tools/Setup MainScene")]
    public static void SetupMainScene()
    {
        // MainScene 열기
        string scenePath = "Assets/Scenes/MainScene.unity";
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(scenePath);
        
        if (scene == null || !scene.IsValid())
        {
            Debug.LogError($"[MainSceneSetup] 씬을 열 수 없습니다: {scenePath}");
            return;
        }

        Debug.Log("[MainSceneSetup] MainScene 설정을 시작합니다.");

        // 1. @Managers GameObject 확인 및 추가
        SetupManagers();

        // 2. MainScene GameObject 확인 및 추가
        SetupMainSceneObject();

        // 씬 저장
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[MainSceneSetup] MainScene 설정이 완료되었습니다.");
    }

    private static void SetupManagers()
    {
        // @Managers GameObject 찾기
        GameObject managersGo = GameObject.Find("@Managers");

        if (managersGo == null)
        {
            // @Managers GameObject 생성
            managersGo = new GameObject("@Managers");
            managersGo.AddComponent<Managers>();
            Debug.Log("[MainSceneSetup] @Managers GameObject를 생성했습니다.");
        }
        else
        {
            // Managers 컴포넌트 확인
            Managers managers = managersGo.GetComponent<Managers>();
            if (managers == null)
            {
                managersGo.AddComponent<Managers>();
                Debug.Log("[MainSceneSetup] @Managers에 Managers 컴포넌트를 추가했습니다.");
            }
            else
            {
                Debug.Log("[MainSceneSetup] @Managers GameObject가 이미 존재합니다.");
            }
        }
    }

    private static void SetupMainSceneObject()
    {
        // MainScene GameObject 찾기
        GameObject mainSceneGo = GameObject.Find("MainScene");

        if (mainSceneGo == null)
        {
            // MainScene GameObject 생성
            mainSceneGo = new GameObject("MainScene");
            mainSceneGo.AddComponent<MainScene>();
            Debug.Log("[MainSceneSetup] MainScene GameObject를 생성했습니다.");
        }
        else
        {
            // MainScene 컴포넌트 확인
            MainScene mainScene = mainSceneGo.GetComponent<MainScene>();
            if (mainScene == null)
            {
                mainSceneGo.AddComponent<MainScene>();
                Debug.Log("[MainSceneSetup] MainScene GameObject에 MainScene 컴포넌트를 추가했습니다.");
            }
            else
            {
                Debug.Log("[MainSceneSetup] MainScene GameObject가 이미 존재합니다.");
            }
        }
    }
}




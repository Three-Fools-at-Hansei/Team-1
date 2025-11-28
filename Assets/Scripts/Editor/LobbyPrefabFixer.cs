using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// 로비 프리팹에 누락된 컴포넌트를 추가하는 에디터 스크립트입니다.
/// Unity 메뉴에서 "Tools/Fix Lobby Prefabs"를 선택하여 사용합니다.
/// </summary>
public class LobbyPrefabFixer
{
    [MenuItem("Tools/Fix Lobby Prefabs")]
    public static void FixLobbyPrefabs()
    {
        Debug.Log("[LobbyPrefabFixer] 프리팹 수정을 시작합니다.");

        // 1. UI_LobbyView 프리팹 수정
        FixLobbyViewPrefab();

        // 2. 팝업 프리팹들 수정
        FixPlayPopupPrefab();
        FixSettingPopupPrefab();
        FixExitPopupPrefab();

        Debug.Log("[LobbyPrefabFixer] 모든 프리팹 수정이 완료되었습니다.");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void FixLobbyViewPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/View/UI_LobbyView.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFixer] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        // 프리팹 인스턴스 생성
        GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);

        // UI_LobbyView 스크립트 추가
        UI_LobbyView lobbyView = instance.GetComponent<UI_LobbyView>();
        if (lobbyView == null)
        {
            lobbyView = instance.AddComponent<UI_LobbyView>();
            Debug.Log("[LobbyPrefabFixer] UI_LobbyView 컴포넌트를 추가했습니다.");
        }

        // RectTransform 확인 및 추가
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = instance.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            Debug.Log("[LobbyPrefabFixer] RectTransform을 추가했습니다.");
        }

        // 프리팹 저장
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        PrefabUtility.UnloadPrefabContents(instance);

        Debug.Log($"[LobbyPrefabFixer] UI_LobbyView 프리팹 수정 완료: {prefabPath}");
    }

    private static void FixPlayPopupPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_PlayPopup.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFixer] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);

        UI_PlayPopup popup = instance.GetComponent<UI_PlayPopup>();
        if (popup == null)
        {
            popup = instance.AddComponent<UI_PlayPopup>();
            Debug.Log("[LobbyPrefabFixer] UI_PlayPopup 컴포넌트를 추가했습니다.");
        }

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        PrefabUtility.UnloadPrefabContents(instance);

        Debug.Log($"[LobbyPrefabFixer] UI_PlayPopup 프리팹 수정 완료: {prefabPath}");
    }

    private static void FixSettingPopupPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_SettingPopup.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFixer] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);

        UI_SettingPopup popup = instance.GetComponent<UI_SettingPopup>();
        if (popup == null)
        {
            popup = instance.AddComponent<UI_SettingPopup>();
            Debug.Log("[LobbyPrefabFixer] UI_SettingPopup 컴포넌트를 추가했습니다.");
        }

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        PrefabUtility.UnloadPrefabContents(instance);

        Debug.Log($"[LobbyPrefabFixer] UI_SettingPopup 프리팹 수정 완료: {prefabPath}");
    }

    private static void FixExitPopupPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_ExitPopup.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFixer] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);

        UI_ExitPopup popup = instance.GetComponent<UI_ExitPopup>();
        if (popup == null)
        {
            popup = instance.AddComponent<UI_ExitPopup>();
            Debug.Log("[LobbyPrefabFixer] UI_ExitPopup 컴포넌트를 추가했습니다.");
        }

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        PrefabUtility.UnloadPrefabContents(instance);

        Debug.Log($"[LobbyPrefabFixer] UI_ExitPopup 프리팹 수정 완료: {prefabPath}");
    }
}




using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// 로비 프리팹의 Inspector 필드를 자동으로 연결하는 에디터 스크립트입니다.
/// Unity 메뉴에서 "Tools/Connect Lobby Prefab Fields"를 선택하여 사용합니다.
/// </summary>
public class LobbyPrefabFieldConnector
{
    [MenuItem("Tools/Connect Lobby Prefab Fields")]
    public static void ConnectLobbyPrefabFields()
    {
        Debug.Log("[LobbyPrefabFieldConnector] 프리팹 필드 연결을 시작합니다.");

        // 1. UI_LobbyView 프리팹 필드 연결
        ConnectLobbyViewFields();

        // 2. UI_PlayPopup 프리팹 필드 연결
        ConnectPlayPopupFields();

        // 3. UI_SettingPopup 프리팹 필드 연결
        ConnectSettingPopupFields();

        // 4. UI_ExitPopup 프리팹 필드 연결
        ConnectExitPopupFields();

        Debug.Log("[LobbyPrefabFieldConnector] 모든 프리팹 필드 연결이 완료되었습니다.");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConnectLobbyViewFields()
    {
        string prefabPath = "Assets/Prefabs/UI/View/UI_LobbyView.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        UI_LobbyView lobbyView = prefab.GetComponent<UI_LobbyView>();
        if (lobbyView == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] UI_LobbyView 컴포넌트를 찾을 수 없습니다: {prefabPath}");
            return;
        }

        // 버튼 찾기
        Button playButton = FindComponentInChildren<Button>(prefab.transform, "PlayButton");
        Button settingButton = FindComponentInChildren<Button>(prefab.transform, "SettingButton");
        Button exitButton = FindComponentInChildren<Button>(prefab.transform, "ExitButton");
        TMP_Text versionText = FindComponentInChildren<TMP_Text>(prefab.transform, "VersionText");

        // SerializedObject를 사용하여 필드 연결
        SerializedObject serializedObject = new SerializedObject(lobbyView);
        
        if (playButton != null)
        {
            serializedObject.FindProperty("_playButton").objectReferenceValue = playButton;
            Debug.Log("[LobbyPrefabFieldConnector] PlayButton 연결 완료");
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabFieldConnector] PlayButton을 찾을 수 없습니다.");
        }

        if (settingButton != null)
        {
            serializedObject.FindProperty("_settingButton").objectReferenceValue = settingButton;
            Debug.Log("[LobbyPrefabFieldConnector] SettingButton 연결 완료");
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabFieldConnector] SettingButton을 찾을 수 없습니다.");
        }

        if (exitButton != null)
        {
            serializedObject.FindProperty("_exitButton").objectReferenceValue = exitButton;
            Debug.Log("[LobbyPrefabFieldConnector] ExitButton 연결 완료");
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabFieldConnector] ExitButton을 찾을 수 없습니다.");
        }

        if (versionText != null)
        {
            serializedObject.FindProperty("_versionText").objectReferenceValue = versionText;
            Debug.Log("[LobbyPrefabFieldConnector] VersionText 연결 완료");
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabFieldConnector] VersionText를 찾을 수 없습니다.");
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(prefab);
    }

    private static void ConnectPlayPopupFields()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_PlayPopup.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        UI_PlayPopup popup = prefab.GetComponent<UI_PlayPopup>();
        if (popup == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] UI_PlayPopup 컴포넌트를 찾을 수 없습니다: {prefabPath}");
            return;
        }

        Button closeButton = FindComponentInChildren<Button>(prefab.transform, "CloseButton");
        TMP_Text contentText = FindComponentInChildren<TMP_Text>(prefab.transform, "ContentArea");

        SerializedObject serializedObject = new SerializedObject(popup);

        if (closeButton != null)
        {
            serializedObject.FindProperty("_closeButton").objectReferenceValue = closeButton;
            Debug.Log("[LobbyPrefabFieldConnector] UI_PlayPopup CloseButton 연결 완료");
        }

        if (contentText != null)
        {
            serializedObject.FindProperty("_contentText").objectReferenceValue = contentText;
            Debug.Log("[LobbyPrefabFieldConnector] UI_PlayPopup ContentText 연결 완료");
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(prefab);
    }

    private static void ConnectSettingPopupFields()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_SettingPopup.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        UI_SettingPopup popup = prefab.GetComponent<UI_SettingPopup>();
        if (popup == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] UI_SettingPopup 컴포넌트를 찾을 수 없습니다: {prefabPath}");
            return;
        }

        Button closeButton = FindComponentInChildren<Button>(prefab.transform, "CloseButton");
        TMP_Text contentText = FindComponentInChildren<TMP_Text>(prefab.transform, "ContentArea");

        SerializedObject serializedObject = new SerializedObject(popup);

        if (closeButton != null)
        {
            serializedObject.FindProperty("_closeButton").objectReferenceValue = closeButton;
            Debug.Log("[LobbyPrefabFieldConnector] UI_SettingPopup CloseButton 연결 완료");
        }

        if (contentText != null)
        {
            serializedObject.FindProperty("_contentText").objectReferenceValue = contentText;
            Debug.Log("[LobbyPrefabFieldConnector] UI_SettingPopup ContentText 연결 완료");
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(prefab);
    }

    private static void ConnectExitPopupFields()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_ExitPopup.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        UI_ExitPopup popup = prefab.GetComponent<UI_ExitPopup>();
        if (popup == null)
        {
            Debug.LogError($"[LobbyPrefabFieldConnector] UI_ExitPopup 컴포넌트를 찾을 수 없습니다: {prefabPath}");
            return;
        }

        Button yesButton = FindComponentInChildren<Button>(prefab.transform, "YesButton");
        Button noButton = FindComponentInChildren<Button>(prefab.transform, "NoButton");
        TMP_Text messageText = FindComponentInChildren<TMP_Text>(prefab.transform, "MessageText");

        SerializedObject serializedObject = new SerializedObject(popup);

        if (yesButton != null)
        {
            serializedObject.FindProperty("_yesButton").objectReferenceValue = yesButton;
            Debug.Log("[LobbyPrefabFieldConnector] UI_ExitPopup YesButton 연결 완료");
        }

        if (noButton != null)
        {
            serializedObject.FindProperty("_noButton").objectReferenceValue = noButton;
            Debug.Log("[LobbyPrefabFieldConnector] UI_ExitPopup NoButton 연결 완료");
        }

        if (messageText != null)
        {
            serializedObject.FindProperty("_messageText").objectReferenceValue = messageText;
            Debug.Log("[LobbyPrefabFieldConnector] UI_ExitPopup MessageText 연결 완료");
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(prefab);
    }

    /// <summary>
    /// 자식 오브젝트에서 특정 이름의 컴포넌트를 찾습니다.
    /// </summary>
    private static T FindComponentInChildren<T>(Transform parent, string name) where T : Component
    {
        if (parent == null) return null;

        // 직접 자식에서 찾기
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child.GetComponent<T>();
            }
        }

        // 재귀적으로 모든 자식에서 찾기
        foreach (Transform child in parent)
        {
            T component = FindComponentInChildren<T>(child, name);
            if (component != null)
                return component;
        }

        return null;
    }
}




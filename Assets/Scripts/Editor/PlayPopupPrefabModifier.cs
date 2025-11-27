using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// UI_PlayPopup 프리팹을 이미지 레이아웃에 맞게 수정하는 에디터 스크립트입니다.
/// Unity 메뉴에서 "Tools/Modify PlayPopup Prefab"을 선택하여 사용합니다.
/// </summary>
public class PlayPopupPrefabModifier
{
    [MenuItem("Tools/Modify PlayPopup Prefab")]
    public static void ModifyPlayPopupPrefab()
    {
        Debug.Log("[PlayPopupPrefabModifier] UI_PlayPopup 프리팹 수정을 시작합니다.");

        string prefabPath = "Assets/Prefabs/UI/Popup/UI_PlayPopup.prefab";
        
        // 프리팹 로드
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
        
        if (prefabInstance == null)
        {
            Debug.LogError($"[PlayPopupPrefabModifier] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        // UI_PlayPopup 컴포넌트 확인
        UI_PlayPopup playPopup = prefabInstance.GetComponent<UI_PlayPopup>();
        if (playPopup == null)
        {
            Debug.LogError($"[PlayPopupPrefabModifier] UI_PlayPopup 컴포넌트를 찾을 수 없습니다.");
            PrefabUtility.UnloadPrefabContents(prefabInstance);
            return;
        }

        // 1. 기존 ContentArea 제거 또는 숨김
        Transform contentArea = prefabInstance.transform.Find("ContentArea");
        if (contentArea != null)
        {
            Object.DestroyImmediate(contentArea.gameObject);
            Debug.Log("[PlayPopupPrefabModifier] 기존 ContentArea 제거 완료");
        }

        // 2. 기존 버튼들 확인 및 제거 (이미 존재하는 경우)
        Transform createGameButton = prefabInstance.transform.Find("CreateGameButton");
        Transform joinGameButton = prefabInstance.transform.Find("JoinGameButton");
        
        if (createGameButton != null)
        {
            Object.DestroyImmediate(createGameButton.gameObject);
        }
        if (joinGameButton != null)
        {
            Object.DestroyImmediate(joinGameButton.gameObject);
        }

        // 3. 버튼 컨테이너 생성 (중앙 배치)
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(prefabInstance.transform, false);
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(500, 200);

        // 4. 게임 생성 버튼 생성 (왼쪽)
        GameObject createButton = CreateButton("CreateGameButton", "게임 생성", buttonContainer.transform);
        RectTransform createRect = createButton.GetComponent<RectTransform>();
        createRect.anchorMin = new Vector2(0.5f, 0.5f);
        createRect.anchorMax = new Vector2(0.5f, 0.5f);
        createRect.pivot = new Vector2(0.5f, 0.5f);
        createRect.anchoredPosition = new Vector2(-150, 0); // 왼쪽으로 150px
        createRect.sizeDelta = new Vector2(200, 80);

        // 5. 게임 참가 버튼 생성 (오른쪽)
        GameObject joinButton = CreateButton("JoinGameButton", "게임 참가", buttonContainer.transform);
        RectTransform joinRect = joinButton.GetComponent<RectTransform>();
        joinRect.anchorMin = new Vector2(0.5f, 0.5f);
        joinRect.anchorMax = new Vector2(0.5f, 0.5f);
        joinRect.pivot = new Vector2(0.5f, 0.5f);
        joinRect.anchoredPosition = new Vector2(150, 0); // 오른쪽으로 150px
        joinRect.sizeDelta = new Vector2(200, 80);

        // 6. UI_PlayPopup 스크립트 필드 연결
        SerializedObject serializedObject = new SerializedObject(playPopup);
        
        Button createButtonComponent = createButton.GetComponent<Button>();
        Button joinButtonComponent = joinButton.GetComponent<Button>();
        Button closeButton = FindComponentInChildren<Button>(prefabInstance.transform, "CloseButton");

        if (createButtonComponent != null)
        {
            serializedObject.FindProperty("_createGameButton").objectReferenceValue = createButtonComponent;
            Debug.Log("[PlayPopupPrefabModifier] CreateGameButton 연결 완료");
        }

        if (joinButtonComponent != null)
        {
            serializedObject.FindProperty("_joinGameButton").objectReferenceValue = joinButtonComponent;
            Debug.Log("[PlayPopupPrefabModifier] JoinGameButton 연결 완료");
        }

        if (closeButton != null)
        {
            serializedObject.FindProperty("_closeButton").objectReferenceValue = closeButton;
            Debug.Log("[PlayPopupPrefabModifier] CloseButton 연결 완료");
        }

        serializedObject.ApplyModifiedProperties();

        // 7. 프리팹 저장
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabInstance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[PlayPopupPrefabModifier] UI_PlayPopup 프리팹 수정이 완료되었습니다.");
    }

    /// <summary>
    /// 기본 버튼을 생성합니다.
    /// </summary>
    private static GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);

        RectTransform rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 80);

        Image image = button.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = image;

        // 버튼 텍스트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TMP_Text tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = 24;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        return button;
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



using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

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

    [MenuItem("Tools/Create Game Start Confirm Popup")]
    public static void CreateGameStartConfirmPopupPrefab()
    {
        Debug.Log("[PlayPopupPrefabModifier] UI_GameStartConfirmPopup 프리팹 생성을 시작합니다.");

        string prefabPath = "Assets/Prefabs/UI/Popup/UI_GameStartConfirmPopup.prefab";
        string directory = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (File.Exists(prefabPath))
            AssetDatabase.DeleteAsset(prefabPath);

        GameObject popup = CreateBasePopup("UI_GameStartConfirmPopup");
        UI_GameStartConfirmPopup popupComponent = popup.AddComponent<UI_GameStartConfirmPopup>();

        // 메시지 텍스트
        GameObject messageGo = new GameObject("MessageText");
        messageGo.transform.SetParent(popup.transform, false);
        RectTransform messageRect = messageGo.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.7f);
        messageRect.anchorMax = new Vector2(0.5f, 0.7f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.sizeDelta = new Vector2(320, 80);

        TMP_Text messageText = messageGo.AddComponent<TextMeshProUGUI>();
        messageText.text = "게임을 시작할까요?";
        messageText.fontSize = 32;
        messageText.alignment = TextAlignmentOptions.Center;

        // 버튼 컨테이너
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(popup.transform, false);
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.35f);
        containerRect.anchorMax = new Vector2(0.5f, 0.35f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(360, 80);

        GameObject confirmButton = CreateButton("ConfirmButton", "확인", buttonContainer.transform);
        RectTransform confirmRect = confirmButton.GetComponent<RectTransform>();
        confirmRect.sizeDelta = new Vector2(160, 60);
        confirmRect.anchoredPosition = new Vector2(-100, 0);

        GameObject cancelButton = CreateButton("CancelButton", "취소", buttonContainer.transform);
        RectTransform cancelRect = cancelButton.GetComponent<RectTransform>();
        cancelRect.sizeDelta = new Vector2(160, 60);
        cancelRect.anchoredPosition = new Vector2(100, 0);

        SerializedObject serializedObject = new SerializedObject(popupComponent);
        serializedObject.FindProperty("_messageText").objectReferenceValue = messageText;
        serializedObject.FindProperty("_confirmButton").objectReferenceValue = confirmButton.GetComponent<Button>();
        serializedObject.FindProperty("_cancelButton").objectReferenceValue = cancelButton.GetComponent<Button>();
        serializedObject.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(popup, prefabPath);
        Object.DestroyImmediate(popup);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Addressable에 등록
        RegisterToAddressable(prefabPath, "UI/Popup/UI_GameStartConfirmPopup");

        Debug.Log("[PlayPopupPrefabModifier] UI_GameStartConfirmPopup 프리팹 생성이 완료되었습니다.");
    }

    [MenuItem("Tools/Register Game Start Confirm Popup to Addressable")]
    public static void RegisterGameStartConfirmPopupToAddressable()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_GameStartConfirmPopup.prefab";
        
        if (!File.Exists(prefabPath))
        {
            Debug.LogError($"[PlayPopupPrefabModifier] 프리팹을 찾을 수 없습니다: {prefabPath}");
            return;
        }

        RegisterToAddressable(prefabPath, "UI/Popup/UI_GameStartConfirmPopup");
        Debug.Log("[PlayPopupPrefabModifier] Addressable 등록이 완료되었습니다.");
    }

    [MenuItem("Tools/Create Game Join Popup")]
    public static void CreateGameJoinPopupPrefab()
    {
        Debug.Log("[PlayPopupPrefabModifier] UI_GameJoinPopup 프리팹 생성을 시작합니다.");

        string prefabPath = "Assets/Prefabs/UI/Popup/UI_GameJoinPopup.prefab";
        string directory = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (File.Exists(prefabPath))
            AssetDatabase.DeleteAsset(prefabPath);

        GameObject popup = CreateBasePopup("UI_GameJoinPopup");
        UI_GameJoinPopup popupComponent = popup.AddComponent<UI_GameJoinPopup>();

        // PopupPanel 찾기
        Transform popupPanel = popup.transform.Find("PopupPanel");
        if (popupPanel == null)
        {
            Debug.LogError("[PlayPopupPrefabModifier] PopupPanel을 찾을 수 없습니다.");
            Object.DestroyImmediate(popup);
            return;
        }

        // 메시지 텍스트
        GameObject messageGo = new GameObject("MessageText");
        messageGo.transform.SetParent(popupPanel, false);
        RectTransform messageRect = messageGo.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.7f);
        messageRect.anchorMax = new Vector2(0.5f, 0.7f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.sizeDelta = new Vector2(320, 80);

        TMP_Text messageText = messageGo.AddComponent<TextMeshProUGUI>();
        messageText.text = "게임 코드 입력";
        messageText.fontSize = 32;
        messageText.alignment = TextAlignmentOptions.Center;

        // 게임 코드 입력 필드
        GameObject inputFieldGo = CreateInputField("CodeInputField", popupPanel);
        RectTransform inputRect = inputFieldGo.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.pivot = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(360, 60);
        inputRect.anchoredPosition = Vector2.zero;

        TMP_InputField inputField = inputFieldGo.GetComponent<TMP_InputField>();
        inputField.placeholder.GetComponent<TMP_Text>().text = "게임 코드를 입력하세요";
        inputField.contentType = TMP_InputField.ContentType.Alphanumeric;

        // 입장하기 버튼
        GameObject enterButton = CreateButton("EnterButton", "입장하기", popupPanel);
        RectTransform enterRect = enterButton.GetComponent<RectTransform>();
        enterRect.anchorMin = new Vector2(0.5f, 0.3f);
        enterRect.anchorMax = new Vector2(0.5f, 0.3f);
        enterRect.pivot = new Vector2(0.5f, 0.5f);
        enterRect.sizeDelta = new Vector2(200, 60);
        enterRect.anchoredPosition = Vector2.zero;

        // Serialized 필드 연결
        SerializedObject serializedObject = new SerializedObject(popupComponent);
        serializedObject.FindProperty("_messageText").objectReferenceValue = messageText;
        serializedObject.FindProperty("_codeInputField").objectReferenceValue = inputField;
        serializedObject.FindProperty("_enterButton").objectReferenceValue = enterButton.GetComponent<Button>();
        serializedObject.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(popup, prefabPath);
        Object.DestroyImmediate(popup);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Addressable에 등록
        RegisterToAddressable(prefabPath, "UI/Popup/UI_GameJoinPopup");

        Debug.Log("[PlayPopupPrefabModifier] UI_GameJoinPopup 프리팹 생성이 완료되었습니다.");
    }

    /// <summary>
    /// TMP_InputField를 생성합니다.
    /// </summary>
    private static GameObject CreateInputField(string name, Transform parent)
    {
        GameObject inputFieldGo = new GameObject(name);
        inputFieldGo.transform.SetParent(parent, false);

        RectTransform rect = inputFieldGo.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(360, 60);

        Image image = inputFieldGo.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        TMP_InputField inputField = inputFieldGo.AddComponent<TMP_InputField>();

        // Text Area (Viewport)
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputFieldGo.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(textArea.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0);

        // Text
        GameObject text = new GameObject("Text");
        text.transform.SetParent(viewport.transform, false);
        RectTransform textRect = text.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TMP_Text textComponent = text.AddComponent<TextMeshProUGUI>();
        textComponent.text = string.Empty;
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Left;

        // Placeholder
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(viewport.transform, false);
        RectTransform placeholderRect = placeholder.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.anchoredPosition = Vector2.zero;

        TMP_Text placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "게임 코드를 입력하세요";
        placeholderText.fontSize = 24;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        placeholderText.fontStyle = FontStyles.Italic;

        // InputField 설정
        inputField.textViewport = viewportRect;
        inputField.textComponent = textComponent;
        inputField.placeholder = placeholderText;
        inputField.targetGraphic = image;

        return inputFieldGo;
    }

    /// <summary>
    /// 프리팹을 Addressable 시스템에 등록합니다.
    /// </summary>
    private static void RegisterToAddressable(string assetPath, string addressableAddress)
    {
#if UNITY_EDITOR
        try
        {
            // Addressable 설정 가져오기
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[PlayPopupPrefabModifier] Addressable 설정을 찾을 수 없습니다.");
                return;
            }

            // 에셋 GUID 가져오기
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"[PlayPopupPrefabModifier] 에셋 GUID를 찾을 수 없습니다: {assetPath}");
                return;
            }

            // 이미 등록되어 있는지 확인
            AddressableAssetEntry existingEntry = settings.FindAssetEntry(guid);
            if (existingEntry != null)
            {
                // 이미 등록되어 있으면 주소만 업데이트
                existingEntry.SetAddress(addressableAddress);
                Debug.Log($"[PlayPopupPrefabModifier] Addressable 주소 업데이트: {addressableAddress}");
            }
            else
            {
                // Default Group 가져오기
                AddressableAssetGroup defaultGroup = settings.DefaultGroup;
                if (defaultGroup == null)
                {
                    Debug.LogError("[PlayPopupPrefabModifier] Default Group을 찾을 수 없습니다.");
                    return;
                }

                // Addressable에 추가
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, defaultGroup, false, false);
                entry.SetAddress(addressableAddress);
                Debug.Log($"[PlayPopupPrefabModifier] Addressable에 등록 완료: {addressableAddress}");
            }

            // 변경사항 저장
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayPopupPrefabModifier] Addressable 등록 실패: {e.Message}");
        }
#endif
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
    private static GameObject CreateBasePopup(string name)
    {
        GameObject popup = new GameObject(name);

        Canvas canvas = popup.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = popup.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1.0f;

        popup.AddComponent<GraphicRaycaster>();

        SortingGroup sortingGroup = popup.AddComponent<SortingGroup>();
        sortingGroup.sortingOrder = 100;

        CanvasGroup canvasGroup = popup.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        GameObject backgroundPanel = new GameObject("BackgroundPanel");
        backgroundPanel.transform.SetParent(popup.transform, false);
        RectTransform bgRect = backgroundPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f);

        GameObject popupPanel = new GameObject("PopupPanel");
        popupPanel.transform.SetParent(popup.transform, false);
        RectTransform panelRect = popupPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(480, 320);

        Image panelImage = popupPanel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        return popup;
    }
}



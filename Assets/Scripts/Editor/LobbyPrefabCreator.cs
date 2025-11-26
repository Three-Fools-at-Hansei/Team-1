using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// 로비 UI 프리팹을 자동으로 생성하는 에디터 스크립트입니다.
/// Unity 메뉴에서 "Tools/Create Lobby Prefabs"를 선택하여 사용합니다.
/// </summary>
public class LobbyPrefabCreator : EditorWindow
{
    [MenuItem("Tools/Create Lobby Prefabs")]
    public static void CreateLobbyPrefabs()
    {
        Debug.Log("[LobbyPrefabCreator] 로비 프리팹 생성을 시작합니다.");

        // 1. UI_LobbyView 프리팹 생성
        CreateLobbyViewPrefab();

        // 2. UI_PlayPopup 프리팹 생성
        CreatePlayPopupPrefab();

        // 3. UI_SettingPopup 프리팹 생성
        CreateSettingPopupPrefab();

        // 4. UI_ExitPopup 프리팹 생성
        CreateExitPopupPrefab();

        Debug.Log("[LobbyPrefabCreator] 모든 로비 프리팹 생성이 완료되었습니다.");
        AssetDatabase.Refresh();
    }

    private static void CreateLobbyViewPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/View/UI_LobbyView.prefab";
        string directory = Path.GetDirectoryName(prefabPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 기존 프리팹이 있으면 삭제
        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        // GameObject 생성
        GameObject lobbyView = new GameObject("UI_LobbyView");

        // CanvasGroup 추가 (UI_View 요구사항)
        CanvasGroup canvasGroup = lobbyView.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // UI_LobbyView 스크립트 추가 (스크립트가 존재하는 경우에만)
        Type lobbyViewType = Type.GetType("UI_LobbyView");
        if (lobbyViewType != null)
        {
            lobbyView.AddComponent(lobbyViewType);
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabCreator] UI_LobbyView 스크립트를 찾을 수 없습니다. 프리팹 생성 후 수동으로 추가해주세요.");
        }

        // 버튼 컨테이너 생성
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(lobbyView.transform, false);
        RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0, 0.5f);
        buttonContainerRect.anchorMax = new Vector2(0, 0.5f);
        buttonContainerRect.pivot = new Vector2(0, 0.5f);
        buttonContainerRect.anchoredPosition = new Vector2(100, 0); // 좌측에서 100px 떨어진 위치
        buttonContainerRect.sizeDelta = new Vector2(200, 300);

        // Play 버튼 생성
        GameObject playButton = CreateButton("PlayButton", "Play", buttonContainer.transform);
        RectTransform playRect = playButton.GetComponent<RectTransform>();
        playRect.anchoredPosition = new Vector2(0, 100);

        // Setting 버튼 생성
        GameObject settingButton = CreateButton("SettingButton", "Setting", buttonContainer.transform);
        RectTransform settingRect = settingButton.GetComponent<RectTransform>();
        settingRect.anchoredPosition = new Vector2(0, 0);

        // Exit 버튼 생성
        GameObject exitButton = CreateButton("ExitButton", "Exit", buttonContainer.transform);
        RectTransform exitRect = exitButton.GetComponent<RectTransform>();
        exitRect.anchoredPosition = new Vector2(0, -100);

        // 버전 정보 텍스트 생성
        GameObject versionText = new GameObject("VersionText");
        versionText.transform.SetParent(lobbyView.transform, false);
        RectTransform versionRect = versionText.AddComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(0, 0);
        versionRect.anchorMax = new Vector2(0, 0);
        versionRect.pivot = new Vector2(0, 0);
        versionRect.anchoredPosition = new Vector2(50, 50);
        versionRect.sizeDelta = new Vector2(200, 30);

        TMP_Text versionTMP = versionText.AddComponent<TextMeshProUGUI>();
        versionTMP.text = "Ver. 0.0.0.1d";
        versionTMP.fontSize = 14;
        versionTMP.color = Color.gray;

        // 프리팹으로 저장
        PrefabUtility.SaveAsPrefabAsset(lobbyView, prefabPath);
        DestroyImmediate(lobbyView);

        Debug.Log($"[LobbyPrefabCreator] UI_LobbyView 프리팹 생성 완료: {prefabPath}");
    }

    private static void CreatePlayPopupPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_PlayPopup.prefab";
        string directory = Path.GetDirectoryName(prefabPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        GameObject popup = CreateBasePopup("UI_PlayPopup");
        // UI_PlayPopup 스크립트 추가 (스크립트가 존재하는 경우에만)
        Type playPopupType = Type.GetType("UI_PlayPopup");
        if (playPopupType != null)
        {
            popup.AddComponent(playPopupType);
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabCreator] UI_PlayPopup 스크립트를 찾을 수 없습니다. 프리팹 생성 후 수동으로 추가해주세요.");
        }

        // 팝업 내용 영역 (나중에 구현)
        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(popup.transform, false);
        RectTransform contentRect = contentArea.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(400, 300);

        TMP_Text contentText = contentArea.AddComponent<TextMeshProUGUI>();
        contentText.text = "Play Popup\n(구현 예정)";
        contentText.fontSize = 24;
        contentText.alignment = TextAlignmentOptions.Center;

        // 닫기 버튼
        GameObject closeButton = CreateButton("CloseButton", "닫기", popup.transform);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.1f);
        closeRect.anchorMax = new Vector2(0.5f, 0.1f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.anchoredPosition = Vector2.zero;
        closeRect.sizeDelta = new Vector2(150, 50);

        PrefabUtility.SaveAsPrefabAsset(popup, prefabPath);
        DestroyImmediate(popup);

        Debug.Log($"[LobbyPrefabCreator] UI_PlayPopup 프리팹 생성 완료: {prefabPath}");
    }

    private static void CreateSettingPopupPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_SettingPopup.prefab";
        string directory = Path.GetDirectoryName(prefabPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        GameObject popup = CreateBasePopup("UI_SettingPopup");
        // UI_SettingPopup 스크립트 추가 (스크립트가 존재하는 경우에만)
        Type settingPopupType = Type.GetType("UI_SettingPopup");
        if (settingPopupType != null)
        {
            popup.AddComponent(settingPopupType);
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabCreator] UI_SettingPopup 스크립트를 찾을 수 없습니다. 프리팹 생성 후 수동으로 추가해주세요.");
        }

        // 팝업 내용 영역 (나중에 구현)
        GameObject contentArea = new GameObject("ContentArea");
        contentArea.transform.SetParent(popup.transform, false);
        RectTransform contentRect = contentArea.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(400, 300);

        TMP_Text contentText = contentArea.AddComponent<TextMeshProUGUI>();
        contentText.text = "Setting Popup\n(구현 예정)";
        contentText.fontSize = 24;
        contentText.alignment = TextAlignmentOptions.Center;

        // 닫기 버튼
        GameObject closeButton = CreateButton("CloseButton", "닫기", popup.transform);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0.1f);
        closeRect.anchorMax = new Vector2(0.5f, 0.1f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.anchoredPosition = Vector2.zero;
        closeRect.sizeDelta = new Vector2(150, 50);

        PrefabUtility.SaveAsPrefabAsset(popup, prefabPath);
        DestroyImmediate(popup);

        Debug.Log($"[LobbyPrefabCreator] UI_SettingPopup 프리팹 생성 완료: {prefabPath}");
    }

    private static void CreateExitPopupPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/Popup/UI_ExitPopup.prefab";
        string directory = Path.GetDirectoryName(prefabPath);
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(prefabPath))
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        GameObject popup = CreateBasePopup("UI_ExitPopup");
        // UI_ExitPopup 스크립트 추가 (스크립트가 존재하는 경우에만)
        Type exitPopupType = Type.GetType("UI_ExitPopup");
        if (exitPopupType != null)
        {
            popup.AddComponent(exitPopupType);
        }
        else
        {
            Debug.LogWarning("[LobbyPrefabCreator] UI_ExitPopup 스크립트를 찾을 수 없습니다. 프리팹 생성 후 수동으로 추가해주세요.");
        }

        // 메시지 텍스트
        GameObject messageText = new GameObject("MessageText");
        messageText.transform.SetParent(popup.transform, false);
        RectTransform messageRect = messageText.AddComponent<RectTransform>();
        messageRect.anchorMin = new Vector2(0.5f, 0.6f);
        messageRect.anchorMax = new Vector2(0.5f, 0.6f);
        messageRect.pivot = new Vector2(0.5f, 0.5f);
        messageRect.anchoredPosition = Vector2.zero;
        messageRect.sizeDelta = new Vector2(400, 100);

        TMP_Text messageTMP = messageText.AddComponent<TextMeshProUGUI>();
        messageTMP.text = "정말 종료하시겠습니까?";
        messageTMP.fontSize = 24;
        messageTMP.alignment = TextAlignmentOptions.Center;

        // 예 버튼
        GameObject yesButton = CreateButton("YesButton", "예", popup.transform);
        RectTransform yesRect = yesButton.GetComponent<RectTransform>();
        yesRect.anchorMin = new Vector2(0.4f, 0.2f);
        yesRect.anchorMax = new Vector2(0.4f, 0.2f);
        yesRect.pivot = new Vector2(0.5f, 0.5f);
        yesRect.anchoredPosition = Vector2.zero;
        yesRect.sizeDelta = new Vector2(120, 50);

        // 아니오 버튼
        GameObject noButton = CreateButton("NoButton", "아니오", popup.transform);
        RectTransform noRect = noButton.GetComponent<RectTransform>();
        noRect.anchorMin = new Vector2(0.6f, 0.2f);
        noRect.anchorMax = new Vector2(0.6f, 0.2f);
        noRect.pivot = new Vector2(0.5f, 0.5f);
        noRect.anchoredPosition = Vector2.zero;
        noRect.sizeDelta = new Vector2(120, 50);

        PrefabUtility.SaveAsPrefabAsset(popup, prefabPath);
        DestroyImmediate(popup);

        Debug.Log($"[LobbyPrefabCreator] UI_ExitPopup 프리팹 생성 완료: {prefabPath}");
    }

    /// <summary>
    /// 기본 팝업 구조를 생성합니다 (Canvas, SortingGroup 포함)
    /// </summary>
    private static GameObject CreateBasePopup(string name)
    {
        GameObject popup = new GameObject(name);

        // Canvas 추가
        Canvas canvas = popup.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        // CanvasScaler 추가
        CanvasScaler scaler = popup.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1.0f;

        // GraphicRaycaster 추가
        popup.AddComponent<GraphicRaycaster>();

        // SortingGroup 추가 (UIManager에서 사용)
        SortingGroup sortingGroup = popup.AddComponent<SortingGroup>();
        sortingGroup.sortingOrder = 100;

        // CanvasGroup 추가 (UI_Popup은 UI_View를 상속하므로 필요)
        CanvasGroup canvasGroup = popup.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // 배경 패널 (클릭 시 닫기용)
        GameObject backgroundPanel = new GameObject("BackgroundPanel");
        backgroundPanel.transform.SetParent(popup.transform, false);
        RectTransform bgRect = backgroundPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f); // 반투명 검은색

        // 팝업 패널
        GameObject popupPanel = new GameObject("PopupPanel");
        popupPanel.transform.SetParent(popup.transform, false);
        RectTransform panelRect = popupPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 400);

        Image panelImage = popupPanel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // 어두운 회색 배경

        return popup;
    }

    /// <summary>
    /// 기본 버튼을 생성합니다.
    /// </summary>
    private static GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);

        RectTransform rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 50);

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
        tmpText.fontSize = 18;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;

        return button;
    }
}


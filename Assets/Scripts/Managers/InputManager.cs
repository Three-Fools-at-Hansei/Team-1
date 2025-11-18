using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Input;

    private GameInputActions _inputActions;

    private InputActionMap _currentActionMap;

    public void Init()
    {
        // input actions 생성
        _inputActions = new GameInputActions();

        // 기본값으로 "None" 액션맵 설정
        SwitchActionMap("None");

        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    /// <summary>
    /// 지정된 이름의 액션 맵으로 전환하고, 이전 맵은 비활성화합니다.
    /// </summary>
    /// <param name="mapName">활성화할 액션 맵의 이름입니다. (예: "None", "TestPopup")</param>
    public void SwitchActionMap(string mapName)
    {
        // 1. 현재 액션맵 비활성화
        _currentActionMap?.Disable();

        // 2. 매개변수 키 값으로 액션맵 로드, 활성화
        _currentActionMap = _inputActions.asset.FindActionMap(mapName);

        if (_currentActionMap == null )
        {
            Debug.LogError($"[InputManager] SwitchActionMap() - 액션 맵 로드에 실패했습니다. key: {mapName}");
            return;
        }

        _currentActionMap?.Enable();

        Debug.Log($"[InputManager] 액션 맵을 {mapName}(으)로 전환합니다.");
    }

    /// <summary>
    /// 특정 액션에 대한 콜백을 등록합니다.
    /// </summary>
    /// <param name="actionName">바인딩할 액션의 이름입니다. (예: "Setting", "Fire")</param>
    /// <param name="callback">입력이 발생했을 때 호출될 콜백 함수입니다.</param>
    /// <param name="phase">콜백을 호출할 입력 단계입니다. (기본값: Performed)</param>
    public void BindAction(string actionName, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
    {
        InputAction action = _inputActions.asset.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"[InputManager] 액션을 찾을 수 없습니다: {actionName}");
            return;
        }

        // started: 입력 시작
        // performed: Hold 완료
        // canceled: release
        switch (phase)
        {
            case InputActionPhase.Started:
                action.started += callback;
                break;
            case InputActionPhase.Performed:
                action.performed += callback;
                break;
            case InputActionPhase.Canceled:
                action.canceled += callback;
                break;
        }
    }

    /// <summary>
    /// 특정 액션에 등록된 콜백을 해제합니다.
    /// </summary>
    /// <param name="actionName">바인딩을 해제할 액션의 이름입니다.</param>
    /// <param name="callback">해제할 콜백 함수입니다.</param>
    /// <param name="phase">콜백이 등록된 입력 단계입니다.</param>
    public void UnbindAction(string actionName, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
    {
        InputAction action = _inputActions.asset.FindAction(actionName);
        if (action == null) return; // Unbind 시에는 에러 로그 없이 조용히 처리

        switch (phase)
        {
            case InputActionPhase.Started:
                action.started -= callback;
                break;
            case InputActionPhase.Performed:
                action.performed -= callback;
                break;
            case InputActionPhase.Canceled:
                action.canceled -= callback;
                break;
        }
    }

    /// <summary>
    /// 매니저의 모든 상태를 초기화하고, 모든 액션의 바인딩을 해제합니다.
    /// </summary>
    public void Clear()
    {
        // 기존 GameInputActions 를 버리고(GC) 새로 만들어서 액션 바인딩을 한 것과 같은 효과를 내요.
        _inputActions = new GameInputActions();

        // 기본 액션맵 세팅
        SwitchActionMap("None");

        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }
}
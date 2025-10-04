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
        // input actions ����
        _inputActions = new GameInputActions();

        // �⺻������ "None" �׼Ǹ� ����
        SwitchActionMap("None");

        Debug.Log($"{ManagerType} Manager Init �մϴ�.");
    }

    public void Update() { }

    /// <summary>
    /// ������ �̸��� �׼� ������ ��ȯ�ϰ�, ���� ���� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    /// <param name="mapName">Ȱ��ȭ�� �׼� ���� �̸��Դϴ�. (��: "None", "TestPopup")</param>
    public void SwitchActionMap(string mapName)
    {
        // 1. ���� �׼Ǹ� ��Ȱ��ȭ
        _currentActionMap?.Disable();

        // 2. �Ű����� Ű ������ �׼Ǹ� �ε�, Ȱ��ȭ
        _currentActionMap = _inputActions.asset.FindActionMap(mapName);

        if (_currentActionMap == null )
        {
            Debug.LogError($"[InputManager] SwitchActionMap() - �׼� �� �ε忡 �����߽��ϴ�. key: {mapName}");
            return;
        }

        _currentActionMap?.Enable();

        Debug.Log($"[InputManager] �׼� ���� {mapName}(��)�� ��ȯ�մϴ�.");
    }

    /// <summary>
    /// Ư�� �׼ǿ� ���� �ݹ��� ����մϴ�.
    /// </summary>
    /// <param name="actionName">���ε��� �׼��� �̸��Դϴ�. (��: "Setting", "Fire")</param>
    /// <param name="callback">�Է��� �߻����� �� ȣ��� �ݹ� �Լ��Դϴ�.</param>
    /// <param name="phase">�ݹ��� ȣ���� �Է� �ܰ��Դϴ�. (�⺻��: Performed)</param>
    public void BindAction(string actionName, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
    {
        InputAction action = _inputActions.asset.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"[InputManager] �׼��� ã�� �� �����ϴ�: {actionName}");
            return;
        }

        // started: �Է� ����
        // performed: Hold �Ϸ�
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
    /// Ư�� �׼ǿ� ��ϵ� �ݹ��� �����մϴ�.
    /// </summary>
    /// <param name="actionName">���ε��� ������ �׼��� �̸��Դϴ�.</param>
    /// <param name="callback">������ �ݹ� �Լ��Դϴ�.</param>
    /// <param name="phase">�ݹ��� ��ϵ� �Է� �ܰ��Դϴ�.</param>
    public void UnbindAction(string actionName, Action<InputAction.CallbackContext> callback, InputActionPhase phase = InputActionPhase.Performed)
    {
        InputAction action = _inputActions.asset.FindAction(actionName);
        if (action == null) return; // Unbind �ÿ��� ���� �α� ���� ������ ó��

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
    /// �Ŵ����� ��� ���¸� �ʱ�ȭ�ϰ�, ��� �׼��� ���ε��� �����մϴ�.
    /// </summary>
    public void Clear()
    {
        // ���� GameInputActions �� ������(GC) ���� ���� �׼� ���ε��� �� �Ͱ� ���� ȿ���� ����.
        _inputActions = new GameInputActions();

        // �⺻ �׼Ǹ� ����
        SwitchActionMap("None");

        Debug.Log($"{ManagerType} Manager Clear �մϴ�.");
    }
}
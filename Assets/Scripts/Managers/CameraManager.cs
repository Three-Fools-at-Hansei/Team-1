using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : IManagerBase
{
    public eManagerType ManagerType => eManagerType.Camera;

    private readonly Dictionary<string, CinemachineCamera> _registeredCameras = new();

    private Camera _mainCamera;
    private CinemachineBrain _brain;
    private CinemachineCamera _activeCamera;
    private CinemachineCamera _previousCamera;

    /// <summary>
    /// 메인 카메라에 대한 접근을 제공합니다. (캐싱됨)
    /// </summary>
    public Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            return _mainCamera;
        }
    }

    public void Init()
    {
        // MainCamera, CinemachineBrain 초기화
        if (MainCamera != null)
            _brain = _mainCamera.gameObject.GetOrAddComponent<CinemachineBrain>();

        Debug.Log($"{ManagerType} Manager Init 완료.");
    }

    public void Update() { }

    public void Clear()
    {
        _registeredCameras.Clear();
        _activeCamera = null;
        _previousCamera = null;
        _mainCamera = null;
        _brain = null;

        Debug.Log($"{ManagerType} Manager Clear 완료.");
    }

    // ========================================================================
    // Camera Registration & Activation
    // ========================================================================

    /// <summary>
    /// 씬에 배치된 CinemachineCamera를 매니저에 등록합니다.
    /// </summary>
    /// <param name="key">식별 키</param>
    /// <param name="cam">등록할 카메라</param>
    public void RegisterCamera(string key, CinemachineCamera cam)
    {
        if (string.IsNullOrEmpty(key) || cam == null) return;

        if (_registeredCameras.ContainsKey(key))
        {
            Debug.LogWarning($"[CameraManager] 이미 등록된 카메라 키입니다: {key}");
            return;
        }

        _registeredCameras[key] = cam;

        // 초기 상태는 비활성화 (필요 시 Activate 호출)
        cam.gameObject.SetActive(false);
    }

    /// <summary>
    /// 카메라 등록을 해제합니다. (오브젝트 파괴 시 호출 권장)
    /// </summary>
    public void UnregisterCamera(string key)
    {
        if (_registeredCameras.ContainsKey(key))
        {
            _registeredCameras.Remove(key);
        }
    }

    /// <summary>
    /// 지정된 키의 카메라를 활성화하고, 현재 카메라는 비활성화합니다.
    /// </summary>
    public void Activate(string key)
    {
        if (!_registeredCameras.TryGetValue(key, out var cam))
        {
            Debug.LogError($"[CameraManager] 카메라 키를 찾을 수 없습니다: {key}");
            return;
        }

        ChangeCamera(cam);
    }

    /// <summary>
    /// 이전 카메라로 되돌립니다.
    /// </summary>
    public void RestorePreviousCamera()
    {
        if (_previousCamera != null)
        {
            ChangeCamera(_previousCamera);
            _previousCamera = null;
        }
    }

    private void ChangeCamera(CinemachineCamera nextCam)
    {
        if (_activeCamera == nextCam) return;

        // 기존 카메라 비활성화 전 기록
        if (_activeCamera != null)
        {
            _previousCamera = _activeCamera;
            _activeCamera.gameObject.SetActive(false);
        }

        // 새 카메라 활성화
        _activeCamera = nextCam;
        if (_activeCamera != null)
        {
            _activeCamera.gameObject.SetActive(true);

            // 컷 전환을 원할 경우 Brain의 설정을 잠시 건드릴 수 있음 (옵션)
            Debug.Log($"[CameraManager] 카메라 전환: {nextCam.name}");
        }
    }

    // ========================================================================
    // Targeting & Utility
    // ========================================================================

    /// <summary>
    /// 특정 카메라의 Follow/LookAt 타겟을 설정합니다.
    /// </summary>
    public void SetTarget(string key, Transform target)
    {
        if (_registeredCameras.TryGetValue(key, out var cam))
        {
            cam.Follow = target;
            cam.LookAt = target; // 필요에 따라 null 처리 가능
        }
    }

    /// <summary>
    /// 현재 활성화된 카메라의 타겟을 설정합니다.
    /// </summary>
    public void SetCurrentCameraTarget(Transform target)
    {
        if (_activeCamera != null)
        {
            _activeCamera.Follow = target;
            // 2D 게임의 경우 LookAt은 보통 사용하지 않거나, 특정 로직이 필요할 수 있음
            _activeCamera.LookAt = target;
        }
    }

    /// <summary>
    /// 스크린 좌표(마우스 위치 등)를 월드 좌표로 변환합니다.
    /// (Player 및 Weapon 클래스에서 Camera.main 대신 사용 권장)
    /// </summary>
    public Vector2 ScreenToWorldPoint(Vector2 screenPos)
    {
        if (MainCamera == null) return Vector2.zero;
        return MainCamera.ScreenToWorldPoint(screenPos);
    }
}
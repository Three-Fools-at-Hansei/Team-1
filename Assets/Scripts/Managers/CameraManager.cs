using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        Debug.Log($"{ManagerType} Manager Init 완료.");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Main Camera & Brain 갱신
        if (MainCamera != null)
            _brain = _mainCamera.gameObject.GetOrAddComponent<CinemachineBrain>();

        // 2. 씬에 배치된 CinemachineCamera 자동 검색 및 등록
        var cams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            // 오브젝트 이름을 키로 사용하여 등록
            RegisterCamera(cam.name, cam);

            // 활성 카메라가 없다면 발견된 첫 번째 카메라를 활성화
            if (_activeCamera == null)
            {
                Activate(cam.name);
                Debug.Log($"[CameraManager] {cam.name} 자동 활성화");
            }
        }

        Debug.Log($"[CameraManager] 씬 카메라 스캔 완료. 발견된 카메라: {cams.Length}개");
    }

    public void Update() { }

    public void Clear()
    {
        // 씬 전환 시 데이터 정리
        _registeredCameras.Clear();
        _activeCamera = null;
        _previousCamera = null;
        _mainCamera = null;
        _brain = null;

        Debug.Log($"{ManagerType} Manager Clear 완료.");
    }

    // ========================================================================
    // Scene Setup & Registration
    // ========================================================================

    /// <summary>
    /// 특정 카메라를 매니저에 수동으로 등록합니다.
    /// </summary>
    public void RegisterCamera(string key, CinemachineCamera cam)
    {
        if (string.IsNullOrEmpty(key) || cam == null) return;

        if (!_registeredCameras.ContainsKey(key))
        {
            _registeredCameras[key] = cam;

            // 씬 초기 상태에서 이미 켜져 있는 카메라를 우선 활성 카메라로 간주
            if (cam.gameObject.activeSelf && _activeCamera == null)
            {
                _activeCamera = cam;
            }
            else
            {
                // 나머지는 비활성화하여 Brain 제어권 정리
                cam.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 카메라 등록을 해제합니다.
    /// </summary>
    public void UnregisterCamera(string key)
    {
        if (_registeredCameras.ContainsKey(key))
        {
            _registeredCameras.Remove(key);
        }
    }

    // ========================================================================
    // Activation & Control
    // ========================================================================

    /// <summary>
    /// 지정된 키의 카메라를 활성화합니다.
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
    /// 이전 카메라로 복귀합니다.
    /// </summary>
    public void RestorePreviousCamera()
    {
        if (_previousCamera != null)
        {
            ChangeCamera(_previousCamera);
            _previousCamera = null;
        }
    }

    /// <summary>
    /// 현재 활성화된 카메라를 반환합니다.
    /// </summary>
    public CinemachineCamera GetActiveCamera() => _activeCamera;

    private void ChangeCamera(CinemachineCamera nextCam)
    {
        if (_activeCamera == nextCam) return;

        // 이전 카메라 기록 및 비활성화
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
        }
    }

    // ========================================================================
    // Utility (Targeting & Coordinates)
    // ========================================================================

    /// <summary>
    /// 현재 활성 카메라의 추적 타겟(Follow)을 설정합니다.
    /// </summary>
    public void SetCurrentCameraTarget(Transform target)
    {
        if (_activeCamera != null)
        {
            _activeCamera.Follow = target;
            // 2D 게임에서는 LookAt을 보통 사용하지 않으므로 주석 처리하거나 필요 시 사용
            // _activeCamera.LookAt = target; 
        }
    }

    /// <summary>
    /// 특정 카메라의 타겟을 설정합니다.
    /// </summary>
    public void SetTarget(string key, Transform target)
    {
        if (_registeredCameras.TryGetValue(key, out var cam))
        {
            cam.Follow = target;
        }
    }

    /// <summary>
    /// 스크린 좌표를 월드 좌표로 변환합니다. (Player 등에서 사용)
    /// </summary>
    public Vector2 ScreenToWorldPoint(Vector2 screenPos)
    {
        if (MainCamera == null) return Vector2.zero;
        return MainCamera.ScreenToWorldPoint(screenPos);
    }
}
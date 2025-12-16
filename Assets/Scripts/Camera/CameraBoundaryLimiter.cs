using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;

/// <summary>
/// 카메라가 타일맵 경계를 벗어나지 않도록 제한하는 컴포넌트입니다.
/// Cinemachine 카메라와 함께 사용됩니다.
/// </summary>
public class CameraBoundaryLimiter : MonoBehaviour
{
    [Header("타일맵 참조")]
    [Tooltip("타일맵을 직접 지정하지 않으면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private Tilemap _targetTilemap;

    [Header("카메라 참조")]
    [Tooltip("제한할 카메라를 지정합니다. 비워두면 Main Camera를 사용합니다.")]
    [SerializeField] private Camera _targetCamera;

    [Header("경계 설정")]
    [Tooltip("카메라가 타일맵 경계에서 얼마나 떨어져야 하는지 (카메라 뷰포트 크기 고려). 값이 작을수록 카메라가 더 바깥까지 이동합니다.")]
    [SerializeField] private float _boundaryMargin = 0.0f;
    
    [Tooltip("카메라가 타일맵 경계를 넘어서 보일 수 있는 여유 공간 (월드 유닛). 0.5로 설정하면 타일맵 경계에서 0.5 유닛 더 바깥까지 보입니다.")]
    [SerializeField] private float _outerMargin = 0.5f;
    
    [Tooltip("카메라 뷰포트 크기에 비례한 마진 사용. false로 설정하면 Boundary Margin 값만 사용합니다.")]
    [SerializeField] private bool _useProportionalMargin = false;

    private CinemachineCamera _cinemachineCamera;
    private float _cameraOrthographicSize;
    private float _cameraAspect;

    // 타일맵 경계 (월드 좌표)
    private float _tilemapMinX, _tilemapMaxX, _tilemapMinY, _tilemapMaxY;
    private bool _boundariesCalculated = false;

    private void Awake()
    {
        // 카메라 참조 초기화
        if (_targetCamera == null)
        {
            _targetCamera = Camera.main;
        }

        // CinemachineCamera 찾기
        _cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        
        // CinemachineCamera가 없으면 다시 찾기 시도
        if (_cinemachineCamera == null)
        {
            Invoke(nameof(FindCinemachineCamera), 0.1f);
        }
    }
    
    private void FindCinemachineCamera()
    {
        if (_cinemachineCamera == null)
        {
            _cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        }
    }

    private void Start()
    {
        InitializeTilemap();
    }

    private void LateUpdate()
    {
        // 경계가 계산되지 않았다면 다시 시도
        if (!_boundariesCalculated)
        {
            InitializeTilemap();
        }

        // 카메라 위치 제한 (LateUpdate에서 실행하여 Cinemachine 업데이트 후에 적용)
        if (_boundariesCalculated && _targetCamera != null)
        {
            ClampCameraPosition();
        }
    }

    private void InitializeTilemap()
    {
        // 타일맵이 지정되지 않았다면 씬에서 찾기
        if (_targetTilemap == null)
        {
            // 모든 타일맵 찾기 (비활성화된 것도 포함)
            Tilemap[] allTilemaps = Resources.FindObjectsOfTypeAll<Tilemap>();
            
            if (allTilemaps == null || allTilemaps.Length == 0)
            {
                Debug.LogWarning("[CameraBoundaryLimiter] 씬에서 Tilemap을 찾을 수 없습니다. Inspector에서 수동으로 지정해주세요.");
                Debug.LogWarning("[CameraBoundaryLimiter] 타일맵을 추가하려면: Hierarchy에서 우클릭 > 2D Object > Tilemap > Rectangular");
                return;
            }

            // 활성화된 타일맵 우선 선택
            _targetTilemap = System.Array.Find(allTilemaps, t => t.gameObject.activeInHierarchy);
            
            // 활성화된 것이 없으면 첫 번째 것 사용
            if (_targetTilemap == null)
            {
                _targetTilemap = allTilemaps[0];
                Debug.LogWarning($"[CameraBoundaryLimiter] 활성화된 타일맵이 없어 비활성화된 타일맵을 사용합니다: {_targetTilemap.name}");
            }
            else
            {
                Debug.Log($"[CameraBoundaryLimiter] 타일맵을 자동으로 찾았습니다: {_targetTilemap.name} (GameObject: {_targetTilemap.gameObject.name})");
            }
        }
        else
        {
            Debug.Log($"[CameraBoundaryLimiter] 지정된 타일맵 사용: {_targetTilemap.name}");
        }

        CalculateTilemapBoundaries();
        CalculateCameraViewportSize();
    }

    /// <summary>
    /// 타일맵의 실제 경계를 계산합니다.
    /// 실제로 타일이 있는 셀만 고려하여 경계를 계산합니다.
    /// </summary>
    private void CalculateTilemapBoundaries()
    {
        if (_targetTilemap == null)
        {
            _boundariesCalculated = false;
            return;
        }

        // 실제로 타일이 있는 셀만 찾기
        BoundsInt cellBounds = _targetTilemap.cellBounds;
        Vector3 cellSize = _targetTilemap.cellSize;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        bool hasTiles = false;

        // 타일이 있는 셀만 찾아서 경계 계산
        foreach (Vector3Int pos in cellBounds.allPositionsWithin)
        {
            if (_targetTilemap.HasTile(pos))
            {
                hasTiles = true;
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }
        }

        // 타일이 하나도 없으면 cellBounds 사용
        if (!hasTiles)
        {
            Debug.LogWarning("[CameraBoundaryLimiter] 타일이 없는 타일맵입니다. cellBounds를 사용합니다.");
            minX = cellBounds.xMin;
            maxX = cellBounds.xMax - 1; // xMax는 exclusive이므로 -1
            minY = cellBounds.yMin;
            maxY = cellBounds.yMax - 1; // yMax는 exclusive이므로 -1
        }

        // 셀 좌표를 월드 좌표로 변환
        // 왼쪽 아래 모서리
        Vector3 worldMin = _targetTilemap.CellToWorld(new Vector3Int(minX, minY, 0));
        // 오른쪽 위 모서리 (셀의 오른쪽 위 모서리)
        Vector3 worldMax = _targetTilemap.CellToWorld(new Vector3Int(maxX + 1, maxY + 1, 0));

        _tilemapMinX = worldMin.x;
        _tilemapMaxX = worldMax.x;
        _tilemapMinY = worldMin.y;
        _tilemapMaxY = worldMax.y;

        Debug.Log($"[CameraBoundaryLimiter] 타일맵 경계 계산 완료 - X: [{_tilemapMinX:F2}, {_tilemapMaxX:F2}], Y: [{_tilemapMinY:F2}, {_tilemapMaxY:F2}]");
        Debug.Log($"[CameraBoundaryLimiter] 타일이 있는 셀 범위: X[{minX}, {maxX}], Y[{minY}, {maxY}]");
    }

    /// <summary>
    /// 카메라 뷰포트 크기를 계산합니다.
    /// </summary>
    private void CalculateCameraViewportSize()
    {
        if (_targetCamera == null)
        {
            _boundariesCalculated = false;
            return;
        }

        if (_targetCamera.orthographic)
        {
            _cameraOrthographicSize = _targetCamera.orthographicSize;
            _cameraAspect = _targetCamera.aspect;
            _boundariesCalculated = true;
        }
        else
        {
            Debug.LogWarning("[CameraBoundaryLimiter] Orthographic 카메라만 지원합니다.");
            _boundariesCalculated = false;
        }
    }

    /// <summary>
    /// 카메라 위치를 타일맵 경계 내로 제한합니다.
    /// Cinemachine을 사용하는 경우 Follow 타겟의 위치를 제한합니다.
    /// </summary>
    private void ClampCameraPosition()
    {
        if (!_boundariesCalculated || _targetCamera == null)
            return;

        // 카메라 뷰포트 크기 계산
        float cameraHeight = _cameraOrthographicSize * 2f;
        float cameraWidth = cameraHeight * _cameraAspect;

        // 마진 계산 (비례 마진 사용 시 카메라 크기에 비례)
        float marginX = _useProportionalMargin ? (cameraWidth * 0.01f) : _boundaryMargin;
        float marginY = _useProportionalMargin ? (cameraHeight * 0.01f) : _boundaryMargin;

        // 카메라가 이동할 수 있는 최소/최대 위치 계산
        // 카메라 중심이 이 범위 내에 있어야 함
        // _outerMargin을 빼서 카메라가 타일맵 경계를 약간 넘어서 이동할 수 있도록 함
        float minCameraX = _tilemapMinX + (cameraWidth / 2f) + marginX - _outerMargin;
        float maxCameraX = _tilemapMaxX - (cameraWidth / 2f) - marginX + _outerMargin;
        float minCameraY = _tilemapMinY + (cameraHeight / 2f) + marginY - _outerMargin;
        float maxCameraY = _tilemapMaxY - (cameraHeight / 2f) - marginY + _outerMargin;

        // 타일맵이 카메라보다 작은 경우 처리
        if (minCameraX > maxCameraX)
        {
            minCameraX = maxCameraX = (_tilemapMinX + _tilemapMaxX) / 2f;
        }
        if (minCameraY > maxCameraY)
        {
            minCameraY = maxCameraY = (_tilemapMinY + _tilemapMaxY) / 2f;
        }

        // Cinemachine을 사용하는 경우 Follow 타겟의 위치를 제한
        // 이렇게 하면 카메라가 타일맵 경계를 벗어나지 않음
        if (_cinemachineCamera != null && _cinemachineCamera.Follow != null)
        {
            Transform followTarget = _cinemachineCamera.Follow;
            Vector3 currentTargetPos = followTarget.position;
            
            Vector3 clampedTargetPos = new Vector3(
                Mathf.Clamp(currentTargetPos.x, minCameraX, maxCameraX),
                Mathf.Clamp(currentTargetPos.y, minCameraY, maxCameraY),
                currentTargetPos.z
            );

            // Follow 타겟 위치 제한
            if (currentTargetPos != clampedTargetPos)
            {
                followTarget.position = clampedTargetPos;
            }
        }
        else
        {
            // CinemachineCamera를 다시 찾기 시도
            if (_cinemachineCamera == null)
            {
                _cinemachineCamera = FindObjectOfType<CinemachineCamera>();
            }
            
            // Cinemachine을 사용하지 않는 경우 카메라 위치 직접 제한
            Vector3 currentPosition = _targetCamera.transform.position;
            Vector3 clampedPosition = new Vector3(
                Mathf.Clamp(currentPosition.x, minCameraX, maxCameraX),
                Mathf.Clamp(currentPosition.y, minCameraY, maxCameraY),
                currentPosition.z
            );

            if (currentPosition != clampedPosition)
            {
                _targetCamera.transform.position = clampedPosition;
            }
        }
    }

    /// <summary>
    /// 타일맵을 수동으로 설정합니다.
    /// </summary>
    public void SetTilemap(Tilemap tilemap)
    {
        _targetTilemap = tilemap;
        CalculateTilemapBoundaries();
    }

    /// <summary>
    /// 현재 사용 중인 타일맵을 반환합니다.
    /// </summary>
    public Tilemap GetTilemap()
    {
        return _targetTilemap;
    }

    /// <summary>
    /// 카메라를 수동으로 설정합니다.
    /// </summary>
    public void SetCamera(Camera camera)
    {
        _targetCamera = camera;
        CalculateCameraViewportSize();
    }
}


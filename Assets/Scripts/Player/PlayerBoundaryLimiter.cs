using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 타일맵 경계를 이용하여 플레이어가 맵 밖으로 나가지 못하게 제한하는 컴포넌트입니다.
/// 네트워크 멀티플레이어 환경을 고려하여 Owner만 위치를 제한합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBoundaryLimiter : NetworkBehaviour
{
    [Header("경계 설정")]
    [Tooltip("타일맵 경계로부터의 여유 공간 (플레이어가 경계에서 얼마나 떨어져야 하는지). 플레이어는 타일맵 끝까지 이동할 수 있으므로 작은 값(0.1~0.5)을 권장합니다.")]
    [SerializeField] private float _boundaryMargin = 0.1f;

    [Header("타일맵 참조 (자동으로 찾을 수도 있음)")]
    [Tooltip("타일맵을 직접 지정하지 않으면 씬에서 자동으로 찾습니다.")]
    [SerializeField] private Tilemap _targetTilemap;

    private Rigidbody2D _rigidbody;

    // 경계 값 (캐싱)
    private float _minX, _maxX, _minY, _maxY;
    private bool _boundariesCalculated = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[PlayerBoundaryLimiter] OnNetworkSpawn 호출됨. IsOwner: {IsOwner}");
        if (IsOwner)
        {
            InitializeTilemap();
        }
        else
        {
            Debug.Log("[PlayerBoundaryLimiter] Owner가 아니므로 경계 제한을 비활성화합니다.");
        }
    }

    private void InitializeTilemap()
    {
        // 타일맵이 지정되지 않았다면 씬에서 찾기
        if (_targetTilemap == null)
        {
            // CameraBoundaryLimiter가 사용하는 타일맵과 동일한 것을 찾기
            CameraBoundaryLimiter cameraLimiter = FindObjectOfType<CameraBoundaryLimiter>();
            if (cameraLimiter != null)
            {
                _targetTilemap = cameraLimiter.GetTilemap();
                if (_targetTilemap != null)
                {
                    Debug.Log($"[PlayerBoundaryLimiter] CameraBoundaryLimiter와 동일한 타일맵 사용: {_targetTilemap.name}");
                    CalculateBoundaries();
                    return;
                }
                else
                {
                    Debug.LogWarning("[PlayerBoundaryLimiter] CameraBoundaryLimiter가 타일맵을 아직 찾지 못했습니다. 잠시 후 다시 시도합니다.");
                }
            }
            
            // CameraBoundaryLimiter를 찾지 못했거나 타일맵이 없으면 일반 방법으로 찾기
            Tilemap[] allTilemaps = Resources.FindObjectsOfTypeAll<Tilemap>();
            
            if (allTilemaps == null || allTilemaps.Length == 0)
            {
                Debug.LogWarning("[PlayerBoundaryLimiter] 씬에서 Tilemap을 찾을 수 없습니다. Inspector에서 수동으로 지정해주세요.");
                Debug.LogWarning("[PlayerBoundaryLimiter] 타일맵을 추가하려면: Hierarchy에서 우클릭 > 2D Object > Tilemap > Rectangular");
                return;
            }

            // 활성화된 타일맵 중에서 타일이 가장 많이 있는 것을 선택
            Tilemap bestTilemap = null;
            int maxTileCount = -1;
            
            foreach (var tilemap in allTilemaps)
            {
                if (tilemap.gameObject.activeInHierarchy)
                {
                    // cellBounds를 강제로 업데이트
                    tilemap.CompressBounds();
                    
                    int tileCount = 0;
                    BoundsInt bounds = tilemap.cellBounds;
                    foreach (Vector3Int pos in bounds.allPositionsWithin)
                    {
                        if (tilemap.HasTile(pos)) tileCount++;
                    }
                    
                    Debug.Log($"[PlayerBoundaryLimiter] 타일맵 후보: {tilemap.name}, 타일 수: {tileCount}, 셀 범위: {bounds}");
                    
                    if (tileCount > maxTileCount)
                    {
                        maxTileCount = tileCount;
                        bestTilemap = tilemap;
                    }
                }
            }
            
            if (bestTilemap != null && maxTileCount > 0)
            {
                _targetTilemap = bestTilemap;
                Debug.Log($"[PlayerBoundaryLimiter] 타일맵을 자동으로 찾았습니다: {_targetTilemap.name} (GameObject: {_targetTilemap.gameObject.name}), 타일 수: {maxTileCount}");
            }
            else if (allTilemaps.Length > 0)
            {
                // 타일이 없는 타일맵이라도 사용 (나중에 다시 시도)
                _targetTilemap = allTilemaps[0];
                Debug.LogWarning($"[PlayerBoundaryLimiter] 타일이 있는 타일맵을 찾지 못했습니다. 임시로 {_targetTilemap.name}을 사용합니다. Inspector에서 수동으로 지정해주세요.");
            }
        }
        else
        {
            Debug.Log($"[PlayerBoundaryLimiter] 지정된 타일맵 사용: {_targetTilemap.name}");
        }

        CalculateBoundaries();
    }

    private void Update()
    {
        // 타일맵이 아직 초기화되지 않았거나, CameraBoundaryLimiter의 타일맵을 사용해야 하는 경우
        if (IsOwner)
        {
            if (!_boundariesCalculated)
            {
                InitializeTilemap();
            }
            else if (_targetTilemap != null)
            {
                // CameraBoundaryLimiter가 타일맵을 찾았는지 확인하고 동일한 것을 사용
                CameraBoundaryLimiter cameraLimiter = FindObjectOfType<CameraBoundaryLimiter>();
                if (cameraLimiter != null)
                {
                    Tilemap cameraTilemap = cameraLimiter.GetTilemap();
                    if (cameraTilemap != null && cameraTilemap != _targetTilemap)
                    {
                        // 다른 타일맵을 사용 중이면 변경
                        Debug.Log($"[PlayerBoundaryLimiter] CameraBoundaryLimiter의 타일맵으로 변경: {cameraTilemap.name}");
                        _targetTilemap = cameraTilemap;
                        CalculateBoundaries();
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        // Owner만 위치를 제한 (다른 클라이언트는 네트워크 동기화를 통해 올바른 위치를 받음)
        if (IsOwner && _boundariesCalculated)
        {
            ClampPositionToBoundaries();
        }
    }

    /// <summary>
    /// 타일맵의 실제 경계를 계산합니다.
    /// CameraBoundaryLimiter와 동일한 방식으로 계산합니다.
    /// </summary>
    private void CalculateBoundaries()
    {
        if (_targetTilemap == null)
        {
            _boundariesCalculated = false;
            return;
        }

        // CameraBoundaryLimiter와 동일한 방식으로 계산
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
            Debug.LogWarning("[PlayerBoundaryLimiter] 타일이 없는 타일맵입니다. cellBounds를 사용합니다.");
            minX = cellBounds.xMin;
            maxX = cellBounds.xMax - 1; // xMax는 exclusive이므로 -1
            minY = cellBounds.yMin;
            maxY = cellBounds.yMax - 1; // yMax는 exclusive이므로 -1
        }

        // 셀 경계를 월드 좌표로 변환
        // 타일맵의 transform을 고려하여 변환
        Vector3 worldMin = _targetTilemap.CellToWorld(new Vector3Int(minX, minY, 0));
        Vector3 worldMax = _targetTilemap.CellToWorld(new Vector3Int(maxX + 1, maxY + 1, 0));
        
        // 디버깅: 변환 전 셀 좌표와 타일맵 정보
        Debug.Log($"[PlayerBoundaryLimiter] 타일맵 Transform: position={_targetTilemap.transform.position}, scale={_targetTilemap.transform.lossyScale}");
        Debug.Log($"[PlayerBoundaryLimiter] 셀 좌표 변환 전: minX={minX}, maxX={maxX}, minY={minY}, maxY={maxY}");
        Debug.Log($"[PlayerBoundaryLimiter] CellToWorld 변환 결과: worldMin={worldMin}, worldMax={worldMax}");

        // worldMin과 worldMax가 같거나 잘못된 경우 cellBounds를 직접 사용
        if (worldMin == worldMax || (worldMin.x == 0 && worldMin.y == 0 && worldMax.x == 0 && worldMax.y == 0))
        {
            Debug.LogWarning("[PlayerBoundaryLimiter] CellToWorld 변환이 잘못되었습니다. cellBounds를 직접 사용합니다.");
            // 이미 선언된 cellBounds와 cellSize를 재사용
            Vector3 tilemapPos = _targetTilemap.transform.position;
            
            // 셀 좌표를 월드 좌표로 직접 변환
            worldMin = new Vector3(
                tilemapPos.x + cellBounds.xMin * cellSize.x,
                tilemapPos.y + cellBounds.yMin * cellSize.y,
                0
            );
            worldMax = new Vector3(
                tilemapPos.x + (cellBounds.xMax) * cellSize.x,
                tilemapPos.y + (cellBounds.yMax) * cellSize.y,
                0
            );
            
            Debug.Log($"[PlayerBoundaryLimiter] 직접 계산한 월드 좌표: worldMin={worldMin}, worldMax={worldMax}");
        }

        // 경계에 마진 추가
        _minX = worldMin.x + _boundaryMargin;
        _maxX = worldMax.x - _boundaryMargin;
        _minY = worldMin.y + _boundaryMargin;
        _maxY = worldMax.y - _boundaryMargin;
        
        // min > max 오류 방지
        if (_minX > _maxX)
        {
            Debug.LogError($"[PlayerBoundaryLimiter] ⚠️ 경계 계산 오류: minX ({_minX:F2}) > maxX ({_maxX:F2})");
            float temp = _minX;
            _minX = _maxX;
            _maxX = temp;
        }
        
        if (_minY > _maxY)
        {
            Debug.LogError($"[PlayerBoundaryLimiter] ⚠️ 경계 계산 오류: minY ({_minY:F2}) > maxY ({_maxY:F2})");
            float temp = _minY;
            _minY = _maxY;
            _maxY = temp;
        }

        _boundariesCalculated = true;

        Debug.Log($"[PlayerBoundaryLimiter] 타일맵 경계 계산 완료 - X: [{_minX:F2}, {_maxX:F2}], Y: [{_minY:F2}, {_maxY:F2}]");
        Debug.Log($"[PlayerBoundaryLimiter] 타일이 있는 셀 범위: X[{minX}, {maxX}], Y[{minY}, {maxY}], 셀 크기: {cellSize}");
        Debug.Log($"[PlayerBoundaryLimiter] 월드 좌표: worldMin({worldMin.x:F2}, {worldMin.y:F2}), worldMax({worldMax.x:F2}, {worldMax.y:F2})");
        
        // 플레이어가 경계 밖에 있는지 확인
        if (_rigidbody != null)
        {
            Vector2 playerPos = _rigidbody.position;
            Debug.Log($"[PlayerBoundaryLimiter] 현재 플레이어 위치: ({playerPos.x:F2}, {playerPos.y:F2})");
            
            if (playerPos.x < _minX || playerPos.x > _maxX || playerPos.y < _minY || playerPos.y > _maxY)
            {
                Debug.LogWarning($"[PlayerBoundaryLimiter] ⚠️ 플레이어가 경계 밖에 있습니다!");
                Debug.LogWarning($"[PlayerBoundaryLimiter] 플레이어: ({playerPos.x:F2}, {playerPos.y:F2})");
                Debug.LogWarning($"[PlayerBoundaryLimiter] 경계: X[{_minX:F2}, {_maxX:F2}], Y[{_minY:F2}, {_maxY:F2}]");
                
                // 플레이어를 경계 내로 즉시 이동
                Vector2 clampedPos = new Vector2(
                    Mathf.Clamp(playerPos.x, _minX, _maxX),
                    Mathf.Clamp(playerPos.y, _minY, _maxY)
                );
                _rigidbody.position = clampedPos;
                Debug.LogWarning($"[PlayerBoundaryLimiter] 플레이어 위치 조정됨: ({clampedPos.x:F2}, {clampedPos.y:F2})");
            }
            else
            {
                Debug.Log($"[PlayerBoundaryLimiter] ✓ 플레이어가 경계 내에 있습니다.");
            }
        }
    }

    /// <summary>
    /// 플레이어 위치를 경계 내로 제한합니다.
    /// </summary>
    private void ClampPositionToBoundaries()
    {
        if (!_boundariesCalculated || _rigidbody == null)
            return;

        Vector2 currentPosition = _rigidbody.position;
        Vector2 velocity = _rigidbody.linearVelocity;
        bool wasClamped = false;

        // X축 경계 체크 및 제한
        if (currentPosition.x < _minX)
        {
            currentPosition.x = _minX;
            velocity.x = Mathf.Min(0, velocity.x); // 경계 밖으로 나가는 속도만 제한
            wasClamped = true;
        }
        else if (currentPosition.x > _maxX)
        {
            currentPosition.x = _maxX;
            velocity.x = Mathf.Max(0, velocity.x); // 경계 밖으로 나가는 속도만 제한
            wasClamped = true;
        }

        // Y축 경계 체크 및 제한
        if (currentPosition.y < _minY)
        {
            currentPosition.y = _minY;
            velocity.y = Mathf.Min(0, velocity.y); // 경계 밖으로 나가는 속도만 제한
            wasClamped = true;
        }
        else if (currentPosition.y > _maxY)
        {
            currentPosition.y = _maxY;
            velocity.y = Mathf.Max(0, velocity.y); // 경계 밖으로 나가는 속도만 제한
            wasClamped = true;
        }

        // 위치가 경계를 벗어났다면 강제로 제한
        if (wasClamped)
        {
            _rigidbody.position = currentPosition;
            _rigidbody.linearVelocity = velocity;
            
            // 디버깅: 경계 제한이 발생했을 때만 로그 출력 (너무 많이 출력되지 않도록)
            if (Time.frameCount % 60 == 0) // 1초에 한 번만
            {
                Debug.Log($"[PlayerBoundaryLimiter] 플레이어 위치 제한됨: ({currentPosition.x:F2}, {currentPosition.y:F2}), 경계: X[{_minX:F2}, {_maxX:F2}], Y[{_minY:F2}, {_maxY:F2}]");
            }
        }
    }

    /// <summary>
    /// 현재 계산된 경계 값을 반환합니다. (디버깅용)
    /// </summary>
    public void GetBoundaries(out float minX, out float maxX, out float minY, out float maxY)
    {
        minX = _minX;
        maxX = _maxX;
        minY = _minY;
        maxY = _maxY;
    }

    /// <summary>
    /// 타일맵을 수동으로 설정합니다. (런타임에 변경 가능)
    /// </summary>
    public void SetTilemap(Tilemap tilemap)
    {
        _targetTilemap = tilemap;
        if (IsOwner)
        {
            CalculateBoundaries();
        }
    }
}

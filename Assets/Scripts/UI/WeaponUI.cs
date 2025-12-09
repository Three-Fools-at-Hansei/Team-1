using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
/// 플레이어가 총을 들고 있는 것처럼 보이는 월드 좌표 UI
/// </summary>
public class WeaponUI : MonoBehaviour
{
    [Header("스프라이트 설정")]
    [SerializeField] private Sprite _weaponSprite;
    
    [Header("위치 및 크기 설정")]
    [SerializeField] private Vector3 _baseOffset = new Vector3(0.3f, -0.3f, 0f);
    [SerializeField] private float _scale = 0.5f;
    [SerializeField] private int _sortingOrderOffset = 5;

    [Header("조준선 설정")]
    [SerializeField] private float _aimLineLength = 3f;
    [SerializeField] private float _aimLineWidth = 0.05f;
    [SerializeField] private Color _aimLineStartColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color _aimLineEndColor = new Color(1f, 1f, 1f, 0f);

    private Transform _owner;
    private Transform _weaponRoot;
    private SpriteRenderer _weaponRenderer;
    private SpriteRenderer _ownerRenderer;
    private Camera _mainCamera;
    private Player _player;
    private LineRenderer _aimLineRenderer;

    private void Awake()
    {
        // Inspector에서 할당되지 않았다면 자동 로드 시도
        if (_weaponSprite == null)
        {
            LoadWeaponSprite();
        }
        EnsureRenderer();
        EnsureAimLine();
    }

    private void LateUpdate()
    {
        // 카메라 참조 업데이트
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        UpdatePosition();
        UpdateRotation();
        UpdateFlip();
        UpdateAimLine();
    }

    /// <summary>
    /// 총 UI 초기화
    /// </summary>
    public void Initialize(Transform owner)
    {
        _owner = owner;
        _ownerRenderer = _owner.GetComponent<SpriteRenderer>();
        _player = _owner.GetComponent<Player>();
        _mainCamera = Camera.main;
        EnsureRenderer();
        EnsureAimLine();
        UpdatePosition();
    }

    private void LoadWeaponSprite()
    {
        // 방법 1: Resources 폴더에서 로드 시도
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/UI");
        if (sprites != null && sprites.Length > 0)
        {
            _weaponSprite = System.Array.Find(sprites, sprite => sprite.name == "Select 3");
            if (_weaponSprite != null)
            {
                Debug.Log("[WeaponUI] Select 3 스프라이트를 Resources에서 로드했습니다.");
                return;
            }
        }

        // 방법 2: Addressables에서 로드 시도
        if (Managers.Inst != null && Managers.Resource != null)
        {
            _ = LoadSpriteFromAddressables();
        }
        else
        {
            Debug.LogWarning("[WeaponUI] 스프라이트를 로드할 수 없습니다. Inspector에서 직접 할당해주세요.");
        }
    }

    private async System.Threading.Tasks.Task LoadSpriteFromAddressables()
    {
        try
        {
            Texture2D texture = await Managers.Resource.LoadAsync<Texture2D>("Sprites/UI");
            if (texture != null)
            {
                // 스프라이트 시트에서 개별 스프라이트를 추출할 수 없으므로
                // Inspector에서 직접 할당하도록 안내
                Debug.LogWarning("[WeaponUI] Addressables로는 스프라이트 시트의 개별 스프라이트를 로드할 수 없습니다. Inspector에서 직접 할당해주세요.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[WeaponUI] Addressables 로드 실패: {ex.Message}");
        }
    }

    private void EnsureRenderer()
    {
        if (_weaponRoot == null)
        {
            GameObject root = new GameObject("WeaponUIRoot");
            root.transform.SetParent(transform, false);
            _weaponRoot = root.transform;
        }

        // 스프라이트가 나중에 할당될 수 있으므로 항상 렌더러 확인
        if (_weaponRenderer == null)
        {
            Transform child = _weaponRoot.Find("Weapon");
            if (child == null)
            {
                GameObject go = new GameObject("Weapon");
                go.transform.SetParent(_weaponRoot, false);
                child = go.transform;
            }

            _weaponRenderer = child.GetComponent<SpriteRenderer>();
            if (_weaponRenderer == null)
            {
                _weaponRenderer = child.gameObject.AddComponent<SpriteRenderer>();
            }

            // 스프라이트 할당
            if (_weaponSprite != null)
            {
                _weaponRenderer.sprite = _weaponSprite;
                _weaponRenderer.enabled = true;
            }
            else
            {
                _weaponRenderer.enabled = false;
                Debug.LogWarning("[WeaponUI] 스프라이트가 할당되지 않았습니다. Inspector에서 Weapon Sprite 필드에 Select 3 스프라이트를 할당해주세요.");
            }

            // Sorting 설정
            SpriteRenderer ownerRenderer = GetComponent<SpriteRenderer>();
            if (ownerRenderer != null)
            {
                _weaponRenderer.sortingLayerID = ownerRenderer.sortingLayerID;
                _weaponRenderer.sortingOrder = ownerRenderer.sortingOrder + _sortingOrderOffset;
            }
            else
            {
                _weaponRenderer.sortingLayerID = 0;
                _weaponRenderer.sortingOrder = _sortingOrderOffset;
            }

            child.localScale = Vector3.one * _scale;
            child.localPosition = Vector3.zero;
        }
        else if (_weaponRenderer != null)
        {
            // 렌더러가 이미 있지만 스프라이트가 업데이트되었을 수 있음
            if (_weaponSprite != null && _weaponRenderer.sprite != _weaponSprite)
            {
                _weaponRenderer.sprite = _weaponSprite;
                _weaponRenderer.enabled = true;
            }
        }
    }

    private void UpdatePosition()
    {
        if (_weaponRoot == null || _owner == null)
            return;

        // x 좌표는 항상 고정, 좌우 반전과 무관하게 동일한 위치 유지
        _weaponRoot.position = _owner.position + _baseOffset;
    }

    /// <summary>
    /// 마우스 포인터 위치를 기반으로 좌우 반전 여부 결정
    /// </summary>
    private bool ShouldFlipBasedOnMouse()
    {
        Vector2 aimDirection = Vector2.right;
        
        if (_player != null && _player.IsOwner)
        {
            // Owner인 경우 마우스 위치 직접 읽기
            if (_mainCamera != null && Mouse.current != null)
            {
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
                aimDirection = (mouseWorldPos - (Vector2)_owner.position).normalized;
            }
            else
            {
                // 마우스 입력이 없으면 플레이어의 flipX 상태 사용
                return _ownerRenderer != null && _ownerRenderer.flipX;
            }
        }
        else
        {
            // Owner가 아닌 경우 동기화된 마우스 조준 방향 사용
            PlayerMove playerMove = _owner != null ? _owner.GetComponent<PlayerMove>() : null;
            if (playerMove != null)
            {
                aimDirection = playerMove.CurrentMouseAimDirection;
                if (aimDirection.magnitude < 0.01f)
                {
                    // 동기화된 방향이 없으면 플레이어의 flipX 상태 사용
                    return _ownerRenderer != null && _ownerRenderer.flipX;
                }
            }
            else
            {
                // PlayerMove가 없으면 플레이어의 flipX 상태 사용
                return _ownerRenderer != null && _ownerRenderer.flipX;
            }
        }
        
        // 조준 방향의 x가 음수이면 반전 (왼쪽을 향함)
        return aimDirection.x < 0;
    }

    /// <summary>
    /// 마우스 커서 방향으로 총구 회전
    /// </summary>
    private void UpdateRotation()
    {
        if (_weaponRoot == null || _owner == null)
            return;

        Vector2 aimDirection = Vector2.right;
        
        if (_player != null && _player.IsOwner)
        {
            // Owner인 경우 마우스 위치를 읽어서 조준
            // 카메라가 없으면 다시 찾기
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            // 마우스 입력 확인
            if (Mouse.current != null && _mainCamera != null)
            {
                Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
                Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
                
                // 총구 위치에서 마우스 방향 계산
                Vector2 weaponPos = _weaponRoot.position;
                aimDirection = (mouseWorldPos - weaponPos).normalized;
            }
            else
            {
                // 마우스나 카메라가 없으면 기본 방향 설정
                if (_ownerRenderer != null)
                {
                    aimDirection = _ownerRenderer.flipX ? Vector2.left : Vector2.right;
                }
            }
        }
        else
        {
            // 다른 클라이언트는 동기화된 마우스 조준 방향 사용
            PlayerMove playerMove = _owner != null ? _owner.GetComponent<PlayerMove>() : null;
            if (playerMove != null)
            {
                aimDirection = playerMove.CurrentMouseAimDirection;
                if (aimDirection.magnitude < 0.01f)
                {
                    // 동기화된 방향이 없으면 기본 방향 사용
                    aimDirection = _ownerRenderer != null && _ownerRenderer.flipX ? Vector2.left : Vector2.right;
                }
            }
            else
            {
                // PlayerMove가 없으면 기본 방향 설정
                if (_ownerRenderer != null)
                {
                    aimDirection = _ownerRenderer.flipX ? Vector2.left : Vector2.right;
                }
            }
        }

        // 방향이 유효한 경우에만 회전 적용
        if (aimDirection.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            _weaponRoot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// 마우스 포인터 위치에 따라 총 좌우 반전
    /// </summary>
    private void UpdateFlip()
    {
        if (_weaponRenderer == null)
            return;

        // 마우스 포인터 위치를 기반으로 반전 결정
        bool shouldFlip = ShouldFlipBasedOnMouse();
        _weaponRenderer.flipY = shouldFlip;
    }

    /// <summary>
    /// 총구 위치를 반환합니다 (FirePoint로 사용)
    /// </summary>
    public Vector2 GetMuzzlePosition()
    {
        if (_weaponRoot == null || _weaponRenderer == null)
            return _owner != null ? _owner.position : Vector2.zero;

        // 총의 회전 방향 계산
        float angle = _weaponRoot.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        // 총 스프라이트의 크기를 고려하여 총구 위치 계산
        // 스프라이트의 bounds를 사용하여 총의 길이 계산
        float weaponLength = 0f;
        if (_weaponRenderer.sprite != null)
        {
            Bounds bounds = _weaponRenderer.sprite.bounds;
            weaponLength = bounds.size.x * _scale * 0.5f; // 스프라이트의 절반 길이 (중심에서 끝까지)
        }
        else
        {
            // 기본값 (스프라이트가 없을 경우)
            weaponLength = 0.2f;
        }

        // 총구 위치 = 총의 위치 + 회전 방향 * 총의 길이
        Vector2 muzzlePos = (Vector2)_weaponRoot.position + direction * weaponLength;
        return muzzlePos;
    }

    /// <summary>
    /// 조준선 렌더러 생성 및 초기화
    /// </summary>
    private void EnsureAimLine()
    {
        if (_aimLineRenderer != null)
            return;

        // 조준선용 GameObject 생성
        GameObject aimLineObj = new GameObject("AimLine");
        aimLineObj.transform.SetParent(_weaponRoot != null ? _weaponRoot : transform, false);

        _aimLineRenderer = aimLineObj.AddComponent<LineRenderer>();
        
        // LineRenderer 설정 (2D 게임용)
        _aimLineRenderer.useWorldSpace = true;
        _aimLineRenderer.positionCount = 2;
        _aimLineRenderer.startWidth = _aimLineWidth;
        _aimLineRenderer.endWidth = _aimLineWidth; // 일정한 두께 유지
        _aimLineRenderer.alignment = LineAlignment.TransformZ; // 2D에 적합
        
        // Material 설정 (반투명)
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.SetFloat("_Mode", 3); // Transparent 모드
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.DisableKeyword("_ALPHATEST_ON");
        lineMaterial.EnableKeyword("_ALPHABLEND_ON");
        lineMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        lineMaterial.renderQueue = 3000;
        _aimLineRenderer.material = lineMaterial;
        
        // 그라데이션 효과: 시작은 불투명, 끝은 완전히 투명
        _aimLineRenderer.startColor = _aimLineStartColor;
        _aimLineRenderer.endColor = _aimLineEndColor;
        
        // 색상 그라데이션 활성화
        _aimLineRenderer.colorGradient = new Gradient
        {
            mode = GradientMode.Blend,
            alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(_aimLineStartColor.a, 0f), // 시작점: 시작 색상의 알파
                new GradientAlphaKey(_aimLineEndColor.a, 1f)   // 끝점: 끝 색상의 알파 (투명)
            },
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(_aimLineStartColor, 0f),   // 시작점: 시작 색상
                new GradientColorKey(_aimLineEndColor, 1f)      // 끝점: 끝 색상
            }
        };
        
        _aimLineRenderer.enabled = false; // 초기에는 비활성화
    }

    /// <summary>
    /// 조준선 업데이트
    /// </summary>
    private void UpdateAimLine()
    {
        // Owner인 경우에만 조준선 표시
        if (_player == null || !_player.IsOwner)
        {
            if (_aimLineRenderer != null)
            {
                _aimLineRenderer.enabled = false;
            }
            return;
        }

        if (_aimLineRenderer == null)
        {
            EnsureAimLine();
        }

        if (_aimLineRenderer == null || _weaponRoot == null)
        {
            return;
        }

        // 총구 위치
        Vector2 muzzlePos = GetMuzzlePosition();
        
        // 총의 현재 회전 방향 사용 (총이 마우스를 향하도록 회전하므로)
        float angle = _weaponRoot.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        
        // 조준선 끝 위치 계산 (총의 회전 방향으로)
        Vector2 endPos = muzzlePos + direction * _aimLineLength;
        
        // LineRenderer에 위치 설정 (z 좌표는 카메라와 동일하게)
        float zPos = _mainCamera != null ? _mainCamera.transform.position.z + 1f : 0f;
        _aimLineRenderer.SetPosition(0, new Vector3(muzzlePos.x, muzzlePos.y, zPos));
        _aimLineRenderer.SetPosition(1, new Vector3(endPos.x, endPos.y, zPos));
        _aimLineRenderer.enabled = true;
    }
}


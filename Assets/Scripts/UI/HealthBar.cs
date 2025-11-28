using UnityEngine;

/// <summary>
/// 월드 좌표에 표시되는 간단한 체력바
/// </summary>
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 _offset = new Vector3(0f, -0.6f, 0f);
    [SerializeField] private float _width = 0.9f;
    [SerializeField] private float _height = 0.12f;
    [SerializeField] private Color _fillColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private int _sortingOrderOffset = 10;

    private Transform _owner;
    private Transform _barRoot;
    private SpriteRenderer _backgroundRenderer;
    private SpriteRenderer _fillRenderer;
    private int _maxValue;

    private static Sprite _cachedSprite;

    private void Awake()
    {
        EnsureRenderers();
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    /// <summary>
    /// 체력바 초기화
    /// </summary>
    public void Initialize(Transform owner, int maxValue)
    {
        _owner = owner;
        _maxValue = Mathf.Max(1, maxValue);
        EnsureRenderers();
        SetValue(_maxValue);
        UpdatePosition();
    }

    /// <summary>
    /// 현재 체력 값 갱신
    /// </summary>
    /// <param name="currentValue">현재 체력</param>
    public void SetValue(int currentValue)
    {
        if (_fillRenderer == null || _maxValue <= 0)
            return;

        float ratio = Mathf.Clamp01((float)currentValue / _maxValue);
        Vector3 scale = _fillRenderer.transform.localScale;
        scale.x = _width * ratio;
        _fillRenderer.transform.localScale = scale;

        Vector3 position = _fillRenderer.transform.localPosition;
        position.x = -(_width - scale.x) * 0.5f; // 왼쪽 기준을 고정해 오른쪽만 줄어들도록 이동
        _fillRenderer.transform.localPosition = position;
    }

    private void EnsureRenderers()
    {
        if (_barRoot == null)
        {
            GameObject root = new GameObject("HealthBarRoot");
            root.transform.SetParent(transform, false);
            _barRoot = root.transform;
        }

        if (_backgroundRenderer == null)
        {
            _backgroundRenderer = CreateRenderer("Background", _backgroundColor, _width + 0.05f);
        }

        if (_fillRenderer == null)
        {
            _fillRenderer = CreateRenderer("Fill", _fillColor, _width);
        }
    }

    private SpriteRenderer CreateRenderer(string name, Color color, float width)
    {
        Transform child = _barRoot.Find(name);
        if (child == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_barRoot, false);
            child = go.transform;
        }

        SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = child.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = GetBarSprite();
        renderer.color = color;

        SpriteRenderer ownerRenderer = GetComponent<SpriteRenderer>();
        if (ownerRenderer != null)
        {
            renderer.sortingLayerID = ownerRenderer.sortingLayerID;
            renderer.sortingOrder = ownerRenderer.sortingOrder + _sortingOrderOffset;
        }
        else
        {
            renderer.sortingLayerID = 0;
            renderer.sortingOrder = _sortingOrderOffset;
        }

        child.localScale = new Vector3(width, _height, 1f);
        child.localPosition = Vector3.zero;

        return renderer;
    }

    private void UpdatePosition()
    {
        if (_barRoot == null || _owner == null)
            return;

        _barRoot.position = _owner.position + _offset;
    }

    private static Sprite GetBarSprite()
    {
        if (_cachedSprite != null)
        {
            return _cachedSprite;
        }

        Texture2D texture = new Texture2D(1, 1)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        _cachedSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        return _cachedSprite;
    }
}



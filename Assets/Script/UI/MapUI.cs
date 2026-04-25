using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class MapUI : MonoBehaviour
{
    [Header("Ограничения карты")]
    public Transform tilemapParent; // Перетащить сюда TileMape

    private GameObject _mapPanel;
    private RawImage _mapImage;
    private Camera _mapCamera;
    private bool _isOpen;
    private Bounds? _worldBounds;

    private const float ZoomSpeed = 80f;
    private const float MinZoom = 8f;
    private const float MaxZoom = 100f;

    private Vector2 _lastMousePos;
    private bool _isDragging;

    void Start()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            var panelT = canvas.transform.Find("MapPanel");
            if (panelT != null)
            {
                _mapPanel = panelT.gameObject;
                var rawGO = _mapPanel.transform.Find("MapFrame/MapRawImage");
                if (rawGO != null) _mapImage = rawGO.GetComponent<RawImage>();
            }
        }

        _mapCamera = GameObject.Find("MapCamera")?.GetComponent<Camera>();

        if (tilemapParent != null)
            _worldBounds = CalculateBounds();

        if (_mapPanel != null)
            _mapPanel.SetActive(false);
    }

    Bounds CalculateBounds()
    {
        var renderers = tilemapParent.GetComponentsInChildren<TilemapRenderer>();
        if (renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one * 200f);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);
        return b;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            Toggle();

        if (_isOpen)
        {
            HandleZoom();
            HandlePan();
        }
    }

    void HandleZoom()
    {
        if (_mapCamera == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        float newSize = _mapCamera.orthographicSize - scroll * ZoomSpeed;

        // Максимальный зум — чтобы вид не вылезал за пределы карты
        float maxAllowed = MaxZoom;
        if (_worldBounds.HasValue)
        {
            float maxH = _worldBounds.Value.size.y * 0.5f;
            float maxW = _worldBounds.Value.size.x * 0.5f / _mapCamera.aspect;
            maxAllowed = Mathf.Min(MaxZoom, Mathf.Min(maxH, maxW));
        }

        _mapCamera.orthographicSize = Mathf.Clamp(newSize, MinZoom, maxAllowed);
        _mapCamera.transform.position = ClampPosition(_mapCamera.transform.position);
    }

    void HandlePan()
    {
        if (_mapCamera == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            _lastMousePos = Input.mousePosition;
            _isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
            _isDragging = false;

        if (_isDragging && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePos;
            _lastMousePos = Input.mousePosition;

            float worldPerPixel = (_mapCamera.orthographicSize * 2f) / Screen.height;
            Vector3 newPos = _mapCamera.transform.position
                - new Vector3(delta.x * worldPerPixel, delta.y * worldPerPixel, 0f);

            _mapCamera.transform.position = ClampPosition(newPos);
        }
    }

    Vector3 ClampPosition(Vector3 pos)
    {
        if (!_worldBounds.HasValue || _mapCamera == null) return pos;

        Bounds b = _worldBounds.Value;
        float halfH = _mapCamera.orthographicSize;
        float halfW = halfH * _mapCamera.aspect;

        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;
        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        // Если зум слишком большой — фиксируем по центру
        if (minX > maxX) { minX = maxX = b.center.x; }
        if (minY > maxY) { minY = maxY = b.center.y; }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }

    void Toggle()
    {
        _isOpen = !_isOpen;
        _mapPanel?.SetActive(_isOpen);

        if (_isOpen)
        {
            if (_mapCamera != null)
                _mapCamera.transform.localPosition = new Vector3(0f, 0f, -10f);

            if (tilemapParent != null)
                _worldBounds = CalculateBounds();

            Player.Instance?.DisableInput();
            FindObjectOfType<Atack>()?.DisableInput();
        }
        else
        {
            if (_mapCamera != null)
                _mapCamera.transform.localPosition = new Vector3(0f, 0f, -10f);

            Player.Instance?.EnableInput();
            FindObjectOfType<Atack>()?.EnableInput();
        }
    }

    public bool IsOpen => _isOpen;
}

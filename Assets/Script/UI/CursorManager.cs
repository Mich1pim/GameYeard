using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    [Header("Текстуры курсора")]
    public Texture2D defaultCursor;   // Catpaw Mouse icon       — обычное состояние
    public Texture2D holdingCursor;   // Catpaw holding Mouse icon — зажата ЛКМ / перетаскивание
    public Texture2D pointingCursor;  // Catpaw pointing Mouse icon — курсор над UI-элементом

    [Header("Точка нажатия (hotspot)")]
    public Vector2 defaultHotspot  = Vector2.zero;
    public Vector2 holdingHotspot  = Vector2.zero;
    public Vector2 pointingHotspot = Vector2.zero;

    private enum CursorState { Default, Holding, Pointing }
    private CursorState _current = CursorState.Default;

    public const string PrefKey = "use_custom_cursor";

    void Start()
    {
        if (PlayerPrefs.GetInt(PrefKey, 1) == 0)
        {
            enabled = false;
            return;
        }
        Cursor.visible = true;
        ApplyCursor(CursorState.Default);
    }

    void Update()
    {
        CursorState next = GetDesiredState();
        if (next != _current)
        {
            _current = next;
            ApplyCursor(_current);
        }
    }

    void OnDisable()
    {
        // Вернуть системный курсор при отключении объекта
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    CursorState GetDesiredState()
    {
        // Приоритет 1: перетаскивание предмета или зажатая ЛКМ
        if (InventoryItem.AnyDragging || Input.GetMouseButton(0))
            return CursorState.Holding;

        // Приоритет 2: курсор над UI-элементом
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return CursorState.Pointing;

        return CursorState.Default;
    }

    void ApplyCursor(CursorState state)
    {
        switch (state)
        {
            case CursorState.Holding:
                Cursor.SetCursor(holdingCursor, holdingHotspot, CursorMode.Auto);
                break;
            case CursorState.Pointing:
                Cursor.SetCursor(pointingCursor, pointingHotspot, CursorMode.Auto);
                break;
            default:
                Cursor.SetCursor(defaultCursor, defaultHotspot, CursorMode.Auto);
                break;
        }
    }
}

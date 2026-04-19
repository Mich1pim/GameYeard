using UnityEngine;

/// <summary>
/// Предмет на земле (лут). Можно подобрать и сохраняется при загрузке.
/// Навешивается на префабы лута (монеты, яблоки, клубника и т.д.)
/// </summary>
public class PickupItem : MonoBehaviour, ISaveable
{
    [Header("Настройки")]
    [SerializeField] private string itemName;       // Имя предмета (для ItemRegistry)
    public int count = 1;                           // Количество (публичный для сохранения)
    public string objectID;                         // Уникальный ID для сохранения (публичный)

    private bool _isCollected = false;

    void Awake()
    {
        // Автогенерация ID
        if (string.IsNullOrEmpty(objectID))
        {
            // Для динамически созданных объектов добавляем уникальный идентификатор
            string hierarchyPath = GetHierarchyPath();
            
            // Если в имени есть "(Clone)", значит объект создан динамически
            if (gameObject.name.Contains("(Clone)"))
            {
                // Добавляем позицию и время для уникальности
                objectID = $"{hierarchyPath}_{transform.position.x:F2}_{transform.position.y:F2}_{Time.time:F3}";
            }
            else
            {
                objectID = hierarchyPath;
            }
        }

        // Если имя предмета не задано — берём из имени объекта
        if (string.IsNullOrEmpty(itemName))
            itemName = CleanItemName(gameObject.name);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCollected) return;

        // Проверяем столкновение с игроком
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if (_isCollected) return;

        // Проверяем наличие ItemRegistry в сцене
        if (ItemRegistry.Instance == null)
        {
            Debug.LogError($"[PickupItem] ItemRegistry not found in scene! Create GameObject with ItemRegistry component.");
            return;
        }

        Item item = ItemRegistry.FindItem(itemName);
        if (item == null)
        {
            Debug.LogWarning($"[PickupItem] Item '{itemName}' not found in registry!");
            return;
        }

        // Добавляем в инвентарь
        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < count; i++)
            {
                bool added = InventoryManager.Instance.AddItem(item);
                if (!added)
                {
                    Debug.LogWarning($"[PickupItem] Inventory full, could not add {itemName}");
                    break;
                }
            }

            _isCollected = true;
            Destroy(gameObject);
            Debug.Log($"[PickupItem] Picked up {count}x {itemName}");
        }
    }

    private string CleanItemName(string name)
    {
        // Убираем "(Clone)" и другие суффиксы
        int cloneIndex = name.IndexOf(" (");
        if (cloneIndex > 0)
            return name.Substring(0, cloneIndex);
        return name;
    }

    #region ISaveable

    private string GetHierarchyPath()
    {
        Transform current = transform;
        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return $"PickupItem_{path}";
    }

    public string GetObjectId()
    {
        return objectID;
    }

    public WorldObjectData GetSaveData()
    {
        // Убираем "(Clone)" перед сохранением
        string cleanName = itemName;
        int cloneIdx = cleanName.IndexOf(" (");
        if (cloneIdx > 0) cleanName = cleanName.Substring(0, cloneIdx);

        return new WorldObjectData
        {
            objectId = GetObjectId(),
            objectType = "PickupItem",
            health = count,
            isDead = _isCollected,
            posX = transform.position.x,
            posY = transform.position.y,
            itemName = cleanName
        };
    }

    public void LoadData(WorldObjectData data)
    {
        count = (int)data.health;
        _isCollected = data.isDead;

        if (_isCollected)
        {
            Destroy(gameObject);
        }
    }

    #endregion
}

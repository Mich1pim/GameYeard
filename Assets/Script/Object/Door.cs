using UnityEngine;

public class Door : UsingAllObject, ISaveable
{
    [Header("Сохранение")]
    public string objectID;
    
    public bool isOpen = false;
    
    protected override void Start()
    {
        base.Start();
        interactionDistance = 0.8f;
        
        // Автогенерация ID если не задан
        if (string.IsNullOrEmpty(objectID))
            objectID = GetHierarchyPath();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Use()
    {
        base.Use();
        isOpen = true;
        animator.SetTrigger("Open");
    }

    protected override void UnUse()
    {
        base.UnUse();
        isOpen = false;
        animator.SetTrigger("Close");
    }
    
    private string GetHierarchyPath()
    {
        Transform current = transform;
        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return $"Door_{path}";
    }
    
    #region ISaveable
    
    public string GetObjectId()
    {
        return objectID;
    }
    
    public WorldObjectData GetSaveData()
    {
        return new WorldObjectData
        {
            objectId = GetObjectId(),
            objectType = "Door",
            isDead = isOpen, // Используем isDead для хранения состояния открыта/закрыта
            posX = transform.position.x,
            posY = transform.position.y
        };
    }
    
    public void LoadData(WorldObjectData data)
    {
        isOpen = data.isDead;
        isOpened = data.isDead;
        
        // Устанавливаем правильное состояние анимации только если дверь открыта
        if (isOpen)
        {
            // Для открытой двери устанавливаем параметр напрямую
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }
        }
        // Если дверь закрыта, не трогаем аниматор - он уже в правильном состоянии по умолчанию
        
        Debug.Log($"[Door] Loaded state: {objectID}, isOpen={isOpen}");
    }
    
    #endregion
}

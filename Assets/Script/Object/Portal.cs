using UnityEngine;

/// <summary>
/// Портал для телепортации игрока
/// </summary>
public class Portal : MonoBehaviour, ISaveable
{
    [Header("Сохранение")]
    [Tooltip("Уникальный ID для системы сохранений")]
    public string objectID;
    [Header("Настройки портала")]
    [Tooltip("Целевой портал, куда телепортируется игрок")]
    public Transform targetPortal;
    
    [Tooltip("Смещение от центра портала при телепортации")]
    public Vector2 spawnOffset = Vector2.zero;
    
    [Tooltip("Задержка перед повторной телепортацией (чтобы не зациклиться)")]
    public float cooldown = 1f;
    
    [Tooltip("Тег игрока")]
    public string playerTag = "Player";
    
    [Header("Анимация")]
    [Tooltip("Аниматор портала (если не указан, берется с этого объекта)")]
    public Animator portalAnimator;
    
    private float _lastTeleportTime = -999f;
    private bool _isDestroyed = false;
    private bool _hasLoadedData = false;
    
    void Awake()
    {
        // Автогенерация ID если не задан
        if (string.IsNullOrEmpty(objectID))
            objectID = GetHierarchyPath();
        
        // Если аниматор не указан, пытаемся найти на этом объекте
        if (portalAnimator == null)
        {
            portalAnimator = GetComponent<Animator>();
        }
    }
    
    void Start()
    {
        // Даем время системе сохранений загрузить данные
        // Если через 0.1 секунды данные не загружены и портал удален - скрываем
        Invoke(nameof(CheckDestroyedState), 0.1f);
    }
    
    private void CheckDestroyedState()
    {
        // Если портал был удален, скрываем его
        if (_isDestroyed)
        {
            Debug.Log($"[Portal] Portal {objectID} is destroyed, hiding");
            HidePortal();
        }
    }
    
    private void HidePortal()
    {
        // Отключаем коллайдер и рендерер
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        
        // Отключаем все дочерние объекты
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        
        // Отключаем аниматор
        if (portalAnimator != null)
        {
            portalAnimator.enabled = false;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что это игрок и прошло достаточно времени
        if (other.CompareTag(playerTag) && Time.time - _lastTeleportTime > cooldown)
        {
            TeleportPlayer(other.transform);
        }
    }
    
    private void TeleportPlayer(Transform player)
    {
        if (targetPortal == null)
        {
            Debug.LogWarning("[Portal] Target portal is not set!");
            return;
        }
        
        // Запускаем анимацию закрытия на этом портале
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger("Close");
        }
        
        // Вычисляем позицию телепортации
        Vector3 targetPosition = (Vector3)((Vector2)targetPortal.position + spawnOffset);
        
        // Телепортируем игрока
        player.position = targetPosition;
        
        // Обновляем время последней телепортации
        _lastTeleportTime = Time.time;
        
        // Если у целевого портала тоже есть компонент Portal, обновляем его cooldown и запускаем анимацию
        Portal targetPortalScript = targetPortal.GetComponent<Portal>();
        if (targetPortalScript != null)
        {
            targetPortalScript._lastTeleportTime = Time.time;
            
            // Запускаем анимацию закрытия на целевом портале
            if (targetPortalScript.portalAnimator != null)
            {
                targetPortalScript.portalAnimator.SetTrigger("Close");
            }
        }
        
        Debug.Log($"[Portal] Player teleported to {targetPortal.name}");
    }
    
    /// <summary>
    /// Удаляет портал. Вызывается в конце анимации через Animation Event
    /// </summary>
    public void DestroyPortal()
    {
        _isDestroyed = true;
        Debug.Log($"[Portal] Destroying portal: {gameObject.name}, objectID: {objectID}");
        HidePortal();
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
        return $"Portal_{path}";
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
            objectType = "Portal",
            isDead = _isDestroyed,
            posX = transform.position.x,
            posY = transform.position.y
        };
    }
    
    public void LoadData(WorldObjectData data)
    {
        _isDestroyed = data.isDead;
        _hasLoadedData = true;
        
        // Если портал был удален, скрываем его немедленно
        if (_isDestroyed)
        {
            Debug.Log($"[Portal] Loading destroyed portal: {objectID}, hiding immediately");
            // Отменяем отложенную проверку, если она еще не выполнилась
            CancelInvoke(nameof(CheckDestroyedState));
            HidePortal();
        }
    }
    
    #endregion
    
    // Визуализация в редакторе
    void OnDrawGizmos()
    {
        if (targetPortal != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPortal.position);
            
            Gizmos.color = Color.green;
            Vector3 targetPos = (Vector3)((Vector2)targetPortal.position + spawnOffset);
            Gizmos.DrawWireSphere(targetPos, 0.3f);
        }
    }
}

using UnityEngine;

public class Extraction : MonoBehaviour, ISaveable
{
    #region Serialized Fields

    [Header("Object ID (для сохранений)")]
    [SerializeField] private string objectID;

    [Header("Combat Settings")]
    [SerializeField] private int damage = 2;
    [SerializeField] private int health = 6;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject spawnPrefabCoin;
    [SerializeField] private GameObject spawnPrefabLoot;
    [SerializeField] private float spawnRadius = 1.3f;
    [SerializeField] private int maxSpawnCount = 4;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] textures; // [0] - мёртвое состояние, [1] - живое

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 10f;

    #endregion

    #region Private Fields

    private InventoryManager _inventoryManager;
    private bool _isDying = false;
    private bool _isRespawning = false;
    private float _respawnTimer = 0f;
    private int _currentHealth;

    #endregion

    #region Constants

    private const int LootSpawnCount = 3;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Автогенерация ID если не назначен
        if (string.IsNullOrEmpty(objectID))
            objectID = GetHierarchyPath();

        // Поиск InventoryManager, если не назначен в инспекторе
        if (_inventoryManager == null)
            _inventoryManager = FindObjectOfType<InventoryManager>();

        // Получение SpriteRenderer, если не назначен
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Проверка наличия текстур для избежания IndexOutOfRangeException
        if (textures == null || textures.Length < 2)
        {
            Debug.LogError($"Extraction на объекте {gameObject.name}: массив textures должен содержать минимум 2 спрайта (мёртвый и живой)!");
            enabled = false;
            return;
        }

        _currentHealth = health;
    }

    private void FixedUpdate()
    {
        if (_isRespawning)
        {
            _respawnTimer += Time.fixedDeltaTime;
            if (_respawnTimer >= respawnDelay)
            {
                Respawn();
            }
        }
    }

    #endregion

    #region Public Methods

    public void TakeDamage()
    {
        // Нельзя получить урон во время смерти или респавна
        if (_isDying || _isRespawning) return;

        if (_inventoryManager == null) return;

        Item receivedItem = _inventoryManager.GetSelectedItem(false);
        if (receivedItem != null && CompareTag(receivedItem.name))
        {
            _currentHealth -= damage;
            if (_currentHealth <= 0 && !_isDying)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        _isDying = true;

        // Спавн лута
        SpawnLoot();

        // Смена спрайта на мёртвый
        spriteRenderer.sprite = textures[0];

        // Запуск респавна
        _isRespawning = true;
        _respawnTimer = 0f;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Возрождение объекта после смерти.
    /// </summary>
    private void Respawn()
    {
        _isRespawning = false;
        _respawnTimer = 0f;
        _isDying = false;
        _currentHealth = health;

        // Восстановление живого спрайта
        if (textures.Length > 1)
            spriteRenderer.sprite = textures[1];
    }

    /// <summary>
    /// Спавн монет и обычного лута.
    /// </summary>
    private void SpawnLoot()
    {
        int coinCount = Random.Range(1, maxSpawnCount + 1);
        SpawnPrefabs(spawnPrefabCoin, coinCount, spawnRadius);
        SpawnPrefabs(spawnPrefabLoot, LootSpawnCount, spawnRadius);
    }

    /// <summary>
    /// Спавн указанного префаба в случайных позициях вокруг объекта.
    /// </summary>
    private void SpawnPrefabs(GameObject prefab, int count, float radius)
    {
        if (prefab == null) return;

        Vector2 spawnOrigin = transform.position;
        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-radius, radius),
                Random.Range(-radius, radius)
            );
            Vector2 spawnPosition = spawnOrigin + randomOffset;
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }

    #endregion

    #region Trigger Handling

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tool"))
        {
            TakeDamage();
        }
    }

    #endregion

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
        return $"{GetType().Name}_{path}";
    }

    public string GetObjectId()
    {
        return objectID;
    }

    public WorldObjectData GetSaveData()
    {
        return new WorldObjectData
        {
            objectId = GetObjectId(),
            objectType = "Extraction",
            health = _currentHealth,
            isDead = _isDying,
            isRespawning = _isRespawning,
            respawnTimer = _respawnTimer
        };
    }

    public void LoadData(WorldObjectData data)
    {
        _currentHealth = (int)data.health;
        _isDying = data.isDead;
        _isRespawning = data.isRespawning;
        _respawnTimer = data.respawnTimer;

        if (_isDying)
        {
            spriteRenderer.sprite = textures[0];
        }
        else if (textures.Length > 1)
        {
            spriteRenderer.sprite = textures[1];
        }
    }

    #endregion
}
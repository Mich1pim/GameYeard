using UnityEngine;

public class AppleTree : UsingAllObject, ISaveable
{
    #region Serialized Fields

    [Header("Object ID (для сохранений)")]
    [SerializeField] private string objectID;

    [Header("AppleTree Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Sprite[] textures;          // массив спрайтов для стадий роста (0 - мёртвое, 1+ - живые)
    [SerializeField] private GameObject spawnPrefabCoin;
    [SerializeField] private GameObject spawnPrefabLoot;
    [SerializeField] private GameObject spawnPrefabApple;

    [Header("Growth & Spawn")]
    [SerializeField] private float growthInterval = 2f;   // время между стадиями роста (было speedStage)
    [SerializeField] private float spawnRadius = 1.3f;
    [SerializeField] private int maxSpawnCount = 4;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0, 0.5f);

    [Header("Combat & Interaction")]
    [SerializeField] private int damage = 2;
    [SerializeField] private int health = 6;
    [SerializeField] private float respawnDelay = 10f;
    [SerializeField] private float interactionDistance = 1.2f;

    #endregion

    #region Private Fields

    private int _currentStage = 0;          // текущая стадия роста (0 = мёртвое, 1+ = живые)
    private float _growthTimer = 0f;         // таймер для перехода к следующей стадии
    private float _respawnTimer = 0f;
    private bool _isDying = false;
    private bool _isRespawning = false;
    private bool _isAppleReady = false;      // готовы ли яблоки (обычно stage == 2)

    #endregion

    #region Constants

    private const int AppleCountMin = 1;
    private const int AppleCountMax = 4;
    private const float AppleSpawnRadius = 1.5f;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Автогенерация ID если не назначен
        if (string.IsNullOrEmpty(objectID))
            objectID = GetHierarchyPath();

        // Автоматическое получение компонентов, если не назначены в инспекторе
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        // Защита от пустого массива текстур
        if (textures == null || textures.Length == 0)
        {
            Debug.LogError($"AppleTree на объекте {gameObject.name}: массив textures не назначен или пуст!");
            enabled = false;
            return;
        }

        // Начальное состояние: живое дерево, стадия 1 (первый живой спрайт)
        _currentStage = 1;
        _growthTimer = 0f;
        UpdateSpriteByStage();
    }

    private void FixedUpdate()
    {
        // Обновление роста, если дерево живо и не умирает/респавнится
        if (!_isDying && !_isRespawning)
        {
            UpdateGrowth();
        }

        // Логика респавна после смерти
        if (_isRespawning)
        {
            _respawnTimer += Time.fixedDeltaTime;
            if (_respawnTimer >= respawnDelay)
            {
                RespawnTree();
            }
        }
    }

    protected override void Update()
    {
        base.Update();  // обновляет distance из UsingAllObject

        // Сбор урожая, если дерево готово и игрок рядом
        if (distance < interactionDistance && Input.GetKeyDown(interactionKey) && IsTreeReadyForHarvest())
        {
            HarvestApple();
        }
    }

    #endregion

    #region Public Methods (Combat)

    public void TakeDamage()
    {
        if (_isDying || _isRespawning) return;
        if (inventoryManager == null) return;

        Item receivedItem = inventoryManager.GetSelectedItem(false);
        if (receivedItem != null && CompareTag(receivedItem.name))
        {
            health -= damage;
            if (health <= 0 && !_isDying)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        _isDying = true;

        // Спавн лута в зависимости от типа дерева
        SpawnLoot();

        // Переход в мёртвое состояние (спрайт textures[0])
        _currentStage = 0;
        UpdateSpriteByStage();
        _isAppleReady = false;

        // Запуск респавна
        _isRespawning = true;
        _respawnTimer = 0f;
    }

    #endregion

    #region Private Methods (Growth & Harvest)

    /// <summary>
    /// Обновление стадий роста дерева.
    /// </summary>
    private void UpdateGrowth()
    {
        int maxStageIndex = textures.Length - 1; // последний индекс массива спрайтов

        // Если достигли максимальной стадии — не растем дальше
        if (_currentStage >= maxStageIndex)
            return;

        _growthTimer += Time.deltaTime;
        if (_growthTimer >= growthInterval)
        {
            _currentStage++;
            _growthTimer = 0f;
            UpdateSpriteByStage();

            // Обновляем флаг готовности яблок (например, на стадии 2)
            _isAppleReady = (_currentStage == 2);
        }
    }

    /// <summary>
    /// Сбор яблок с дерева.
    /// </summary>
    private void HarvestApple()
    {
        int appleCount = Random.Range(AppleCountMin, AppleCountMax);
        for (int i = 0; i < appleCount; i++)
        {
            SpawnApple();
        }

        // Сбрасываем дерево на начальную стадию (без яблок)
        _currentStage = 1;
        _growthTimer = 0f;
        UpdateSpriteByStage();
        _isAppleReady = false;
    }

    /// <summary>
    /// Проверка, готово ли дерево к сбору урожая.
    /// </summary>
    private bool IsTreeReadyForHarvest()
    {
        return !_isDying && !_isRespawning && _isAppleReady;
    }

    /// <summary>
    /// Возрождение дерева после смерти.
    /// </summary>
    private void RespawnTree()
    {
        _isRespawning = false;
        _respawnTimer = 0f;
        _isDying = false;
        health = 6;

        // Восстанавливаем начальное живое состояние
        _currentStage = 1;
        _growthTimer = 0f;
        UpdateSpriteByStage();
        _isAppleReady = false;
    }

    /// <summary>
    /// Обновляет спрайт в зависимости от текущей стадии.
    /// </summary>
    private void UpdateSpriteByStage()
    {
        if (textures != null && _currentStage >= 0 && _currentStage < textures.Length)
        {
            spriteRenderer.sprite = textures[_currentStage];
        }
        else
        {
            Debug.LogWarning($"AppleTree: неверный индекс стадии {_currentStage} (длина текстур: {textures?.Length})");
        }
    }

    #endregion

    #region Spawn Helpers

    /// <summary>
    /// Спавн монет, обычного лута и (если isApple == true) яблок.
    /// </summary>
    private void SpawnLoot()
    {
        // Всегда спавним монеты и обычный лут
        SpawnPrefabs(spawnPrefabCoin, Random.Range(1, maxSpawnCount + 1), spawnRadius);
        SpawnPrefabs(spawnPrefabLoot, 3, spawnRadius);

        // Если дерево было плодоносящим (с яблоками), добавляем яблоки
        if (_isAppleReady)
        {
            SpawnPrefabs(spawnPrefabApple, 3, spawnRadius);
        }
    }

    /// <summary>
    /// Спавн указанного префаба в случайных позициях вокруг дерева.
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

    /// <summary>
    /// Спавн одного яблока с небольшим случайным смещением.
    /// </summary>
    private void SpawnApple()
    {
        if (spawnPrefabApple == null) return;

        Vector2 randomOffset = new Vector2(
            Random.Range(-AppleSpawnRadius, AppleSpawnRadius),
            Random.Range(-AppleSpawnRadius, AppleSpawnRadius)
        );
        Vector2 spawnPosition = (Vector2)transform.position + spawnOffset + randomOffset;
        Instantiate(spawnPrefabApple, spawnPosition, Quaternion.identity);
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
            objectType = "AppleTree",
            health = health,
            stage = _currentStage,
            isDead = _isDying,
            isRespawning = _isRespawning,
            respawnTimer = _respawnTimer,
            growthTimer = _growthTimer,
            isAppleReady = _isAppleReady
        };
    }

    public void LoadData(WorldObjectData data)
    {
        health = (int)data.health;
        _currentStage = data.stage;
        _isDying = data.isDead;
        _isRespawning = data.isRespawning;
        _respawnTimer = data.respawnTimer;
        _growthTimer = data.growthTimer;
        _isAppleReady = data.isAppleReady;

        UpdateSpriteByStage();
    }

    #endregion
}
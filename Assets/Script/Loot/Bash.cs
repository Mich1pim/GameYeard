using UnityEngine;

public class Bush : UsingAllObject
{
    #region Serialized Fields

    [Header("Bush Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] textures;          // [0] - пустой куст, [1+] - стадии роста с ягодами
    [SerializeField] private GameObject strawberryPrefab;
    
    [Header("Growth Settings")]
    [SerializeField] private float growthIntervalMinutes = 5f; // время между стадиями роста (в минутах)
    
    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 1.2f;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0, 0.5f);

    #endregion

    #region Private Fields

    private int _currentStage = 0;          // текущая стадия роста
    private float _lastGrowthTimeMinutes = 0f; // время последнего роста в минутах (игровых)

    #endregion

    #region Constants

    private const int MinStrawberryCount = 1;
    private const int MaxStrawberryCount = 4; // верхняя граница не включительно в Random.Range, поэтому 4
    private const float StrawberrySpawnRadius = 1.5f;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Автоматическое получение компонентов, если не назначены
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Проверка наличия текстур
        if (textures == null || textures.Length == 0)
        {
            Debug.LogError($"Bush на объекте {gameObject.name}: массив textures не назначен или пуст!");
            enabled = false;
            return;
        }
        
        _currentStage = 0;
        _lastGrowthTimeMinutes = GetCurrentMinutes();
        UpdateSprite();
    }

    private void FixedUpdate()
    {
        // Рост куста только если не достиг максимальной стадии
        if (_currentStage < GetMaxStageIndex())
        {
            float currentMinutes = GetCurrentMinutes();
            if (currentMinutes >= _lastGrowthTimeMinutes + growthIntervalMinutes)
            {
                GrowToNextStage();
                _lastGrowthTimeMinutes = currentMinutes;
            }
        }
    }

    protected override void Update()
    {
        base.Update(); // обновляет distance из UsingAllObject
        
        // Сбор урожая, если куст готов и игрок рядом
        if (distance < interactionDistance && Input.GetKeyDown(interactionKey) && IsReadyForHarvest())
        {
            HarvestStrawberries();
        }
    }

    #endregion

    #region Private Methods (Growth & Harvest)

    /// <summary>
    /// Возвращает текущее игровое время в минутах.
    /// </summary>
    private float GetCurrentMinutes()
    {
        if (GlobalTime.Instance == null)
        {
            Debug.LogWarning("GlobalTime.Instance не найден! Используем 0.");
            return 0f;
        }
        return GlobalTime.Instance.minutes;
    }

    /// <summary>
    /// Возвращает индекс максимальной стадии (последний спрайт).
    /// </summary>
    private int GetMaxStageIndex()
    {
        return textures.Length - 1;
    }

    /// <summary>
    /// Переход к следующей стадии роста.
    /// </summary>
    private void GrowToNextStage()
    {
        if (_currentStage < GetMaxStageIndex())
        {
            _currentStage++;
            UpdateSprite();
        }
    }

    /// <summary>
    /// Проверка, готов ли куст к сбору ягод (достиг ли максимальной стадии).
    /// </summary>
    private bool IsReadyForHarvest()
    {
        return _currentStage >= GetMaxStageIndex();
    }

    /// <summary>
    /// Сбор ягод: спавн ягод и сброс куста в начальное состояние.
    /// </summary>
    private void HarvestStrawberries()
    {
        int strawberryCount = Random.Range(MinStrawberryCount, MaxStrawberryCount);
        for (int i = 0; i < strawberryCount; i++)
        {
            SpawnStrawberry();
        }
        
        // Сброс куста
        _currentStage = 0;
        UpdateSprite();
        _lastGrowthTimeMinutes = GetCurrentMinutes();
    }

    /// <summary>
    /// Обновляет спрайт в зависимости от текущей стадии.
    /// </summary>
    private void UpdateSprite()
    {
        if (textures != null && _currentStage >= 0 && _currentStage < textures.Length)
        {
            spriteRenderer.sprite = textures[_currentStage];
        }
        else
        {
            Debug.LogWarning($"Bush: неверный индекс стадии {_currentStage} (длина текстур: {textures?.Length})");
        }
    }

    /// <summary>
    /// Спавн одной ягоды с случайным смещением.
    /// </summary>
    private void SpawnStrawberry()
    {
        if (strawberryPrefab == null) return;
        
        Vector2 randomOffset = new Vector2(
            Random.Range(-StrawberrySpawnRadius, StrawberrySpawnRadius),
            Random.Range(-StrawberrySpawnRadius, StrawberrySpawnRadius)
        );
        
        Vector2 spawnPosition = (Vector2)transform.position + spawnOffset + randomOffset;
        Instantiate(strawberryPrefab, spawnPosition, Quaternion.identity);
    }

    #endregion
}
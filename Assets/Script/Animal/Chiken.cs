using UnityEngine;

public class Chiken : MonoBehaviour
{
    #region Serialized Fields

    [Header("Chicken Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] textures; // [0] - обычный вид, [1] - вид во время кладки
    
    [Header("Egg Spawn Settings")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int eggsPerSpawn = 1;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0, 0.5f);
    [SerializeField] private float spawnRadius = 1.5f;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f; // как долго держать спрайт кладки

    #endregion

    #region Private Fields

    private float _spawnTimer;
    private Sprite _originalSprite;   // запоминаем исходный спрайт
    private bool _isAnimating;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (eggPrefab == null)
            Debug.LogError($"Chiken на объекте {gameObject.name}: eggPrefab не назначен!");
        
        // Запоминаем исходный спрайт (если есть текстуры и спрайт-рендерер)
        if (spriteRenderer != null && spriteRenderer.sprite != null)
            _originalSprite = spriteRenderer.sprite;
        
        _spawnTimer = spawnInterval;
    }

    private void Update()
    {
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f && !_isAnimating)
        {
            LayEggs();
            _spawnTimer = spawnInterval;
        }
    }

    #endregion

    #region Egg Laying

    private void LayEggs()
    {
        if (eggPrefab == null) return;
        
        // Если есть второй спрайт (textures[1]) — проигрываем анимацию кладки
        if (textures != null && textures.Length > 1 && textures[1] != null)
        {
            StartCoroutine(PlayLayAnimation());
        }
        
        // Спавним яйца
        for (int i = 0; i < eggsPerSpawn; i++)
        {
            SpawnEgg();
        }
    }

    private System.Collections.IEnumerator PlayLayAnimation()
    {
        _isAnimating = true;
        
        // Меняем на спрайт "кладки"
        Sprite oldSprite = spriteRenderer.sprite;
        spriteRenderer.sprite = textures[1];
        
        // Ждём
        yield return new WaitForSeconds(animationDuration);
        
        // Возвращаем исходный спрайт (или первый, если исходный был не задан)
        spriteRenderer.sprite = _originalSprite ?? (textures.Length > 0 ? textures[0] : oldSprite);
        
        _isAnimating = false;
    }

    private void SpawnEgg()
    {
        Vector2 randomOffset = new Vector2(
            Random.Range(-spawnRadius, spawnRadius),
            Random.Range(-spawnRadius, spawnRadius)
        );
        
        Vector2 spawnPosition = (Vector2)transform.position + spawnOffset + randomOffset;
        Instantiate(eggPrefab, spawnPosition, Quaternion.identity);
    }

    #endregion
}
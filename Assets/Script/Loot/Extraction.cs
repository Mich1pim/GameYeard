using UnityEngine;


public class Extraction : MonoBehaviour
{
    public int damage = 2;
    public int health = 6;
    public InventoryManager inventoryManager;
    public GameObject spawnPrefabCoin;
    public GameObject spawnPrefabLoot;
    public float spawnRadius = 1.3f;
    public int maxSpawnCount = 4;
    public SpriteRenderer sprite;
    public Sprite[] textures;
    public bool isDying = false;
    public bool isRespawning = false;
    public float respawnTimer = 0f;
    public float respawnDelay = 10f;

    public void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void TakeDamage()
    {
        Item receivedItem = inventoryManager.GetSelectedItem(false);
        if (this.CompareTag(receivedItem.name))
        {
            health -= damage;
            if (health <= 0 && !isDying)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        isDying = true;

        SpawnPrefabs(spawnPrefabCoin, Random.Range(1, maxSpawnCount), spawnRadius);
        SpawnPrefabs(spawnPrefabLoot, 3, spawnRadius);

        sprite.sprite = textures[0];

        isRespawning = true;
        respawnTimer = 0f;
    }

    public void FixedUpdate()
    {
        if (isRespawning)
        {
            respawnTimer += Time.fixedDeltaTime;
            if (respawnTimer >= respawnDelay)
            {
                sprite.sprite = textures[1];

                isRespawning = false;
                respawnTimer = 0f;

                health = 6;
                isDying = false;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tool"))
        {
            TakeDamage();
        }
    }

    private void SpawnPrefabs(GameObject prefab, int count, float radius)
    {
        Vector2 spawnPosition = transform.position;
        for (int i = 0; i < count; i++)
        {
            Vector2 randomPosition = spawnPosition + new Vector2(
                Random.Range(-radius, radius),
                Random.Range(-radius, radius)
            );
            Instantiate(prefab, randomPosition, Quaternion.identity);
        }
    }
}


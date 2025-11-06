using UnityEngine;

public class AppleTree : UsingAllObject
{
    [Header("AppleTree Settings")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] textures;
    public int maxStage;
    public GameObject ApplePreFab;
    public int stage = 0;
    private int lastMinuteCheck;
    public float interactionDistance = 1.2f;
    
    public Vector2 spawnOffset = new Vector2(0, 0.5f);

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxStage = textures.Length;
        lastMinuteCheck = 0;
        stage = 1;
    }

    public void FixedUpdate()
    {
        int currentMinutes = GlobalTime.Instance.minutes;

        if (currentMinutes >= lastMinuteCheck + 5 && stage < maxStage - 1)
        {
            stage += 1;
            lastMinuteCheck = currentMinutes;
            spriteRenderer.sprite = textures[stage];
        }
    }

    protected override void Update()
    {
        base.Update();

        if (distance < 2f && Input.GetKeyDown(interactionKey) && stage >= maxStage - 1)
        {
            HarvestApple();
        }
    }


    private void HarvestApple()
    {
        int AppleCount = Random.Range(1, 4);

        for (int i = 0; i < AppleCount; i++)
        {
            SpawnApple(i);
        }
        stage = 1;
        spriteRenderer.sprite = textures[stage];
        lastMinuteCheck = GlobalTime.Instance.minutes;
    }

    private void SpawnApple(int index)
    {
        if (ApplePreFab != null)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-1.5f, 1.5f),
                Random.Range(-1.5f, 1.5f)
            );

            Vector2 spawnPosition = (Vector2)transform.position + spawnOffset + randomOffset;

            Instantiate(ApplePreFab, spawnPosition, Quaternion.identity);
        }
    }
}
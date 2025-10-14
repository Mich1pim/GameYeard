using UnityEngine;

public class Bash : MonoBehaviour
{
    [Header("Settings")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] textures;
    public float speedStage = 2f;
    public int maxStage;
    public int stage = 0;
    private int lastMinuteCheck; 

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxStage = textures.Length;
        lastMinuteCheck = 0; 
    }

    public void FixedUpdate()
    {
        int currentMinutes = GlobalTime.Instance.minutes;
        
        if (currentMinutes >= lastMinuteCheck + 5)
        {
            stage += 1;
            lastMinuteCheck = currentMinutes;
            
            if (stage >= maxStage)
            {
                stage = maxStage - 1;
            }
            
            spriteRenderer.sprite = textures[stage];
        }
    }
}
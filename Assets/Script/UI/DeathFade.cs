using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathFade : MonoBehaviour
{
    public static DeathFade Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private float fadeInDuration  = 1.2f;
    [SerializeField] private float holdDuration    = 0.8f;
    [SerializeField] private float fadeOutDuration = 2.0f;

    private void Awake()
    {
        Instance = this;
        SetAlpha(0f);
    }

    // Start вызывается после всех Awake — PlayerHealth.Instance уже гарантированно задан
    private void Start()
    {
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnDied += HandleDeath;
    }

    private void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (Player.Instance != null)
            Player.Instance.DisableInput();

        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(holdDuration);

        PlayerHealth.Instance.Respawn();

        yield return Fade(1f, 0f, fadeOutDuration);

        if (Player.Instance != null)
            Player.Instance.EnableInput();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (blackOverlay == null) return;
        Color c = blackOverlay.color;
        c.a = alpha;
        blackOverlay.color = c;
    }
}

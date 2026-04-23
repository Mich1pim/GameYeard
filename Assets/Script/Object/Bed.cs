using System.Collections;
using UnityEngine;

/// <summary>
/// Скрипт кровати. При нажатии E вечером/ночью устанавливает 11:00 следующего дня
/// и показывает эффект затемнения экрана.
/// </summary>
public class Bed : UsingAllObject
{
    [Header("Настройки сна")]
    [Tooltip("Минимальный час, когда можно лечь спать (18 = вечер)")]
    [SerializeField] private int minSleepHour = 18;
    [Tooltip("Время пробуждения (часы)")]
    [SerializeField] private int wakeUpHour = 11;

    [Header("UI затемнения")]
    [Tooltip("Объект-заполнитель для затемнения экрана (чёрный Image с raycastTarget)")]
    [SerializeField] private GameObject fadeOverlay;
    [Tooltip("Длительность затемнения при засыпании (секунды)")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [Tooltip("Длительность осветления при пробуждении (секунды)")]
    [SerializeField] private float fadeOutDuration = 1.0f;

    private bool _canSleep = false;
    private UnityEngine.UI.Image _fadeImage;

    protected override void Start()
    {
        base.Start();
        interactionDistance = 1.5f;

        // Находим или создаём объект затемнения
        if (fadeOverlay == null)
        {
            CreateFadeOverlay();
        }

        if (fadeOverlay != null)
        {
            _fadeImage = fadeOverlay.GetComponent<UnityEngine.UI.Image>();
            if (_fadeImage != null)
            {
                _fadeImage.color = new Color(0, 0, 0, 0);
            }
        }
    }

    /// <summary>
    /// Создаёт чёрный Image на весь экран для эффекта затемнения как дочерний объект корневого Canvas
    /// </summary>
    private void CreateFadeOverlay()
    {
        // Ищем корневой Canvas (у которого нет родителя-Canvas)
        Canvas rootCanvas = null;
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            // Проверяем что у Canvas нет родительского Canvas
            Transform parent = canvas.transform.parent;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                rootCanvas = canvas;
                break;
            }
        }

        if (rootCanvas == null)
        {
            Debug.LogWarning("Bed: Canvas не найден. Эффект затемнения не будет работать.");
            return;
        }

        GameObject overlay = new GameObject("SleepFadeOverlay");
        overlay.transform.SetParent(rootCanvas.transform);
        overlay.transform.SetAsLastSibling();

        var rectTransform = overlay.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var image = overlay.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, 0);
        image.raycastTarget = false;

        // Делаем overlay активным, но прозрачным
        overlay.SetActive(true);

        fadeOverlay = overlay;
    }

    protected override void Update()
    {
        // Проверяем, можно ли спать (вечер или ночь)
        _canSleep = GlobalTime.Instance.IsEvening() || GlobalTime.Instance.IsNight();

        base.Update();
    }

    protected override void Use()
    {
        if (!_canSleep)
        {
            Debug.Log("Bed: Спать можно только вечером или ночью!");
            return;
        }

        // Не вызываем base.Use() чтобы не блокировать повторное использование
        StartCoroutine(SleepSequence());
    }

    protected override void UnUse()
    {
        // Кровать не имеет состояния UnUse, но сбрасываем флаг
        isOpened = false;
    }

    /// <summary>
    /// Последовательность сна: затемнение → установка времени → осветление
    /// </summary>
    private IEnumerator SleepSequence()
    {
        // Убеждаемся что overlay на самом верху иерархии Canvas
        if (fadeOverlay != null)
        {
            fadeOverlay.transform.SetAsLastSibling();
            fadeOverlay.SetActive(true);
        }

        // 1. Затемнение экрана
        if (_fadeImage != null)
        {
            yield return FadeIn();
        }

        // 2. Устанавливаем 11:00 следующего дня
        GlobalTime.Instance.hours = wakeUpHour;
        GlobalTime.Instance.minutes = 0;
        GlobalTime.Instance.days++;
        GlobalTime.Instance._Time();

        // Восстанавливаем здоровье до максимума
        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.SetHealth(PlayerHealth.Instance.MaxHealth);

        // Меняем погоду на новый день
        if (WeatherManager.Instance != null)
            WeatherManager.Instance.RollNewDay();

        Debug.Log($"Bed: Герой уснул и проснулся в {wakeUpHour:00}:00, день {GlobalTime.Instance.days}");

        // Небольшая пауза
        yield return new WaitForSeconds(0.3f);

        // 3. Осветление экрана
        if (_fadeImage != null)
        {
            yield return FadeOut();
        }
    }

    /// <summary>
    /// Плавное затемнение экрана
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = _fadeImage.color;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            _fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        _fadeImage.color = new Color(0, 0, 0, 1f);
    }

    /// <summary>
    /// Плавное осветление экрана
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            _fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        _fadeImage.color = new Color(0, 0, 0, 0f);
    }
}

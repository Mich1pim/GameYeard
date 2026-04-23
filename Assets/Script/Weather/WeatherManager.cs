using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum WeatherType { Sunny, Cloudy, Rainy, Stormy }

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("Визуальные объекты")]
    [SerializeField] private GameObject rainEffect;      // GameObject с ParticleSystem дождя
    [SerializeField] private Image weatherOverlay;       // UI Image на Canvas (весь экран, прозрачная)
    [SerializeField] private TextMeshProUGUI weatherText; // UI текст — текущая погода в HUD

    [Header("Цвета оверлея")]
    [SerializeField] private Color sunnyColor  = new Color(1f,    0.95f, 0.8f,  0f);
    [SerializeField] private Color cloudyColor = new Color(0.7f,  0.7f,  0.8f,  0.12f);
    [SerializeField] private Color rainyColor  = new Color(0.45f, 0.55f, 0.75f, 0.22f);
    [SerializeField] private Color stormyColor = new Color(0.25f, 0.25f, 0.35f, 0.38f);

    [Header("Вероятности погоды (веса)")]
    [SerializeField] private float sunnyWeight  = 50f;
    [SerializeField] private float cloudyWeight = 25f;
    [SerializeField] private float rainyWeight  = 20f;
    [SerializeField] private float stormyWeight = 5f;

    // Текущий тип погоды — доступен из любого скрипта
    public static WeatherType Current { get; private set; } = WeatherType.Sunny;

    // Множитель роста растений: дождь ускоряет, облака замедляют
    public static float GrowthMultiplier
    {
        get
        {
            switch (Current)
            {
                case WeatherType.Rainy:  return 2f;
                case WeatherType.Stormy: return 1.5f;
                case WeatherType.Cloudy: return 0.8f;
                default:                 return 1f;
            }
        }
    }

    private int _lastWeatherDay = -1;
    private Color _overlayColor;
    private Color _targetOverlayColor;
    private const float OverlayLerpSpeed = 0.8f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        RollWeather();
        _lastWeatherDay = GlobalTime.Instance != null ? GlobalTime.Instance.days : 0;
        _overlayColor = GetTargetColor();
        _targetOverlayColor = _overlayColor;
        ApplyVisuals(instant: true);
    }

    void Update()
    {
        if (GlobalTime.Instance == null) return;

        // Смена погоды раз в игровой день, на рассвете (6:00)
        int currentDay = GlobalTime.Instance.days;
        if (currentDay != _lastWeatherDay && GlobalTime.Instance.hours == 6)
        {
            RollWeather();
            _lastWeatherDay = currentDay;
        }

        // Плавный переход цвета оверлея
        _overlayColor = Color.Lerp(_overlayColor, _targetOverlayColor, Time.deltaTime * OverlayLerpSpeed);
        ApplyVisuals(instant: false);
    }

    private void RollWeather()
    {
        float total = sunnyWeight + cloudyWeight + rainyWeight + stormyWeight;
        float roll  = Random.Range(0f, total);

        if      (roll < sunnyWeight)                              Current = WeatherType.Sunny;
        else if (roll < sunnyWeight + cloudyWeight)               Current = WeatherType.Cloudy;
        else if (roll < sunnyWeight + cloudyWeight + rainyWeight) Current = WeatherType.Rainy;
        else                                                      Current = WeatherType.Stormy;

        _targetOverlayColor = GetTargetColor();
        Debug.Log($"[WeatherManager] День {GlobalTime.Instance?.days}: погода — {Current} (множитель роста x{GrowthMultiplier})");
    }

    private Color GetTargetColor()
    {
        switch (Current)
        {
            case WeatherType.Cloudy: return cloudyColor;
            case WeatherType.Rainy:  return rainyColor;
            case WeatherType.Stormy: return stormyColor;
            default:                 return sunnyColor;
        }
    }

    private void ApplyVisuals(bool instant)
    {
        bool isRaining = Current == WeatherType.Rainy || Current == WeatherType.Stormy;

        if (rainEffect != null)
            rainEffect.SetActive(isRaining);

        if (weatherOverlay != null)
            weatherOverlay.color = instant ? _targetOverlayColor : _overlayColor;

        if (weatherText != null)
            weatherText.text = GetWeatherLabel();
    }

    private string GetWeatherLabel()
    {
        switch (Current)
        {
            case WeatherType.Sunny:  return "Солнечно";
            case WeatherType.Cloudy: return "Облачно";
            case WeatherType.Rainy:  return "Дождь";
            case WeatherType.Stormy: return "Гроза";
            default:                 return "";
        }
    }

    public WeatherData GetSaveData()
    {
        return new WeatherData
        {
            weatherType = (int)Current,
            lastWeatherDay = _lastWeatherDay
        };
    }

    public void LoadData(WeatherData data)
    {
        Current = (WeatherType)data.weatherType;
        _lastWeatherDay = data.lastWeatherDay;
        _targetOverlayColor = GetTargetColor();
        _overlayColor = _targetOverlayColor;
        ApplyVisuals(instant: true);
        Debug.Log($"[WeatherManager] Погода загружена: {Current}");
    }

    // Контекстное меню для тестирования погоды прямо в редакторе
    [ContextMenu("Установить: Солнечно")]
    void ForceSunny()  => ForceWeather(WeatherType.Sunny);

    [ContextMenu("Установить: Облачно")]
    void ForceCloudy() => ForceWeather(WeatherType.Cloudy);

    [ContextMenu("Установить: Дождь")]
    void ForceRainy()  => ForceWeather(WeatherType.Rainy);

    [ContextMenu("Установить: Гроза")]
    void ForceStormy() => ForceWeather(WeatherType.Stormy);

    private void ForceWeather(WeatherType weather)
    {
        Current = weather;
        _targetOverlayColor = GetTargetColor();
        _lastWeatherDay = GlobalTime.Instance != null ? GlobalTime.Instance.days : 0;
        ApplyVisuals(instant: false);
    }

    public void RollNewDay()
    {
        _lastWeatherDay = GlobalTime.Instance != null ? GlobalTime.Instance.days : 0;
        RollWeather();
    }
}

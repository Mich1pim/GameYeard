using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;  // Добавляем Toggle для fullscreen
    private Resolution[] resolutions;
    private List<ResolutionOption> uniqueResolutions;
    private bool isInitialized = false;

    // Структура для хранения уникальных разрешений
    private struct ResolutionOption
    {
        public int width;
        public int height;
        public List<int> refreshRates;

        public ResolutionOption(int w, int h, int rr)
        {
            width = w;
            height = h;
            refreshRates = new List<int> { rr };
        }

        public override string ToString()
        {
            return $"{width}x{height} ({string.Join(", ", refreshRates.OrderByDescending(r => r).Select(r => r + "Hz"))})";
        }
    }

    void Start()
    {
        InitializeResolutionDropdown();
        InitializeFullscreenToggle();
        LoadSettings();
        isInitialized = true;
    }

    void InitializeFullscreenToggle()
    {
        if (fullscreenToggle != null)
        {
            // Устанавливаем начальное состояние Toggle
            fullscreenToggle.isOn = Screen.fullScreen;

            // Подписываемся на изменения
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);

            Debug.Log($"Fullscreen Toggle инициализирован: {fullscreenToggle.isOn}");
        }
        else
        {
            Debug.LogWarning("Fullscreen Toggle не назначен!");
        }
    }

    void InitializeResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("Resolution Dropdown не назначен!");
            return;
        }

        // Получаем все доступные разрешения
        resolutions = Screen.resolutions;

        // Группируем по разрешению (убираем дубликаты с разной частотой)
        uniqueResolutions = new List<ResolutionOption>();

        foreach (var res in resolutions)
        {
            // Фильтруем слишком маленькие разрешения (меньше 800x600)
            if (res.width < 800 || res.height < 600)
                continue;

            // Фильтруем разрешения с низкой частотой (меньше 59Hz)
            int refreshRate = res.refreshRate;
            if (refreshRate < 59)
                continue;

            // Ищем существующее разрешение
            int existingIndex = uniqueResolutions.FindIndex(r =>
                r.width == res.width && r.height == res.height);

            if (existingIndex >= 0)
            {
                // Добавляем частоту если её ещё нет
                var option = uniqueResolutions[existingIndex];
                if (!option.refreshRates.Contains(refreshRate))
                {
                    option.refreshRates.Add(refreshRate);
                    uniqueResolutions[existingIndex] = option;
                }
            }
            else
            {
                // Добавляем новое разрешение
                uniqueResolutions.Add(new ResolutionOption(res.width, res.height, refreshRate));
            }
        }

        // Сортируем по качеству (сначала большие разрешения)
        uniqueResolutions.Sort((a, b) =>
        {
            if (a.width != b.width) return b.width - a.width;
            if (a.height != b.height) return b.height - a.height;
            return b.refreshRates.Max() - a.refreshRates.Max();
        });

        // Создаем опции для dropdown
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // Текущее разрешение экрана
        int currentWidth = Screen.currentResolution.width;
        int currentHeight = Screen.currentResolution.height;

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            options.Add(uniqueResolutions[i].ToString());

            // Ищем ближайшее к текущему разрешению
            if (uniqueResolutions[i].width == currentWidth &&
                uniqueResolutions[i].height == currentHeight)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        Debug.Log($"Найдено {uniqueResolutions.Count} уникальных разрешений (из {resolutions.Length} общих)");
        Debug.Log($"Текущее: {currentWidth}x{currentHeight}, индекс: {currentResolutionIndex}");
    }

    public void OnFullscreenToggleChanged(bool isFullscreen)
    {
        // Применяем только после инициализации
        if (!isInitialized) return;

        Screen.fullScreen = isFullscreen;
        Debug.Log($"Fullscreen изменен на: {isFullscreen}");

        // Сохраняем без применения разрешения
        PlayerPrefs.SetInt("FullscreenPreference", System.Convert.ToInt32(isFullscreen));
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        // Метод для вызова из UI (если не используется Toggle)
        Screen.fullScreen = isFullscreen;

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
        }

        PlayerPrefs.SetInt("FullscreenPreference", System.Convert.ToInt32(isFullscreen));
        PlayerPrefs.Save();

        Debug.Log($"Fullscreen установлен на: {isFullscreen}");
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= uniqueResolutions.Count)
        {
            Debug.LogError("Неверный индекс разрешения!");
            return;
        }

        var resOption = uniqueResolutions[resolutionIndex];

        // Берем максимальную доступную частоту
        int maxRefreshRate = resOption.refreshRates.OrderByDescending(r => r).First();

        // Устанавливаем разрешение
        Screen.SetResolution(resOption.width, resOption.height, Screen.fullScreen, maxRefreshRate);

        Debug.Log($"Установлено: {resOption.width}x{resOption.height} @ {maxRefreshRate}Hz");

        SaveSettings();
    }

    public void SaveSettings()
    {
        // Применяем выбранное разрешение
        ApplyResolution();

        // Сохраняем настройки
        PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", System.Convert.ToInt32(Screen.fullScreen));
        PlayerPrefs.Save();

        Debug.Log("Настройки сохранены и применены");
    }

    public void ApplyResolution()
    {
        int selectedIndex = resolutionDropdown.value;

        if (selectedIndex < 0 || selectedIndex >= uniqueResolutions.Count)
        {
            Debug.LogError("Неверный индекс разрешения!");
            return;
        }

        var resOption = uniqueResolutions[selectedIndex];

        // Берем максимальную доступную частоту
        int maxRefreshRate = resOption.refreshRates.OrderByDescending(r => r).First();

        // Устанавливаем разрешение
        Screen.SetResolution(resOption.width, resOption.height, Screen.fullScreen, maxRefreshRate);

        Debug.Log($"Применено разрешение: {resOption.width}x{resOption.height} @ {maxRefreshRate}Hz");
    }

    public void LoadSettings()
    {
        if (resolutionDropdown == null) return;

        int savedIndex = 0;

        if (PlayerPrefs.HasKey("ResolutionPreference"))
        {
            savedIndex = PlayerPrefs.GetInt("ResolutionPreference");

            // Проверяем что индекс валиден
            if (savedIndex >= uniqueResolutions.Count)
            {
                savedIndex = 0;
                Debug.LogWarning("Сохраненный индекс невалиден, сброс на 0");
            }
        }
        else
        {
            // Ищем текущее разрешение
            int currentWidth = Screen.currentResolution.width;
            int currentHeight = Screen.currentResolution.height;

            for (int i = 0; i < uniqueResolutions.Count; i++)
            {
                if (uniqueResolutions[i].width == currentWidth &&
                    uniqueResolutions[i].height == currentHeight)
                {
                    savedIndex = i;
                    break;
                }
            }
        }

        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        // Загружаем fullscreen
        if (PlayerPrefs.HasKey("FullscreenPreference"))
        {
            bool savedFullscreen = System.Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));

            // Применяем к экрану
            Screen.fullScreen = savedFullscreen;

            // Обновляем Toggle если он есть
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = savedFullscreen;
            }

            Debug.Log($"Загружен fullscreen: {savedFullscreen}");
        }

        Debug.Log($"Настройки загружены, индекс: {savedIndex}");
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;

public class SetupSettingsUI : MonoBehaviour
{
    [MenuItem("Tools/Setup Settings UI in Menu")]
    public static void SetupSettingsUIInScene()
    {
        // Находим Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas не найден в сцене!");
            return;
        }

        // Ищем существующую панель настроек или создаём новую
        GameObject settingsPanel = GameObject.Find("SettingsPanel");
        if (settingsPanel == null)
        {
            settingsPanel = GameObject.Find("Settings");
        }
        if (settingsPanel == null)
        {
            // Ищем любой объект с Image который может быть панелью
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject.name.Contains("Setting") || img.gameObject.name.Contains("Panel"))
                {
                    settingsPanel = img.gameObject;
                    break;
                }
            }
        }

        if (settingsPanel == null)
        {
            Debug.LogError("Панель настроек не найдена! Создаём новую.");
            settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
            settingsPanel.transform.SetParent(canvas.transform, false);
        }

        // Удаляем все старые дочерние элементы панели
        while (settingsPanel.transform.childCount > 0)
        {
            Object.DestroyImmediate(settingsPanel.transform.GetChild(0).gameObject);
        }

        // Настраиваем панель
        RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            panelRect = settingsPanel.AddComponent<RectTransform>();
        }
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 300);
        panelRect.localScale = Vector3.one;

        Image panelImage = settingsPanel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = settingsPanel.AddComponent<Image>();
        }
        panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // Заголовок "Настройки"
        GameObject titleObj = CreateTextObject("TitleText", "Настройки", settingsPanel,
            new Vector2(0, 110), new Vector2(300, 40), 24, Color.white);

        // Label для разрешения
        GameObject resLabel = CreateTextObject("ResolutionLabel", "Разрешение экрана:", settingsPanel,
            new Vector2(-80, 50), new Vector2(180, 30), 16, Color.white);

        // Dropdown для разрешения
        GameObject resDropdown = CreateDropdownObject("ResolutionDropdown", settingsPanel,
            new Vector2(60, 50), new Vector2(220, 35));

        // Кнопка "Применить настройки"
        GameObject applyButton = CreateButtonObject("ApplyButton", "Применить", settingsPanel,
            new Vector2(0, -30), new Vector2(250, 45), "Sprite/UI/buttons/Square Buttons 26x26");

        // Кнопка "Закрыть"
        GameObject closeButton = CreateButtonObject("CloseButton", "Закрыть", settingsPanel,
            new Vector2(0, -90), new Vector2(200, 40), "Sprite/UI/buttons/Square Buttons 19x26");

        // Добавляем компонент SettingsMenu
        SettingsMenu settingsMenu = settingsPanel.GetComponent<SettingsMenu>();
        if (settingsMenu == null)
        {
            settingsMenu = settingsPanel.AddComponent<SettingsMenu>();
        }

        // Привязываем UI элементы через反射
        var settingsPanelField = typeof(SettingsMenu).GetField("settingsPanel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (settingsPanelField != null) settingsPanelField.SetValue(settingsMenu, settingsPanel);

        var dropdownField = typeof(SettingsMenu).GetField("resolutionDropdown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (dropdownField != null)
        {
            TMP_Dropdown dropdown = resDropdown.GetComponent<TMP_Dropdown>();
            dropdownField.SetValue(settingsMenu, dropdown);
        }

        // Привязываем кнопки
        Button applyBtn = applyButton.GetComponent<Button>();
        applyBtn.onClick.RemoveAllListeners();
        applyBtn.onClick.AddListener(() =>
        {
            settingsMenu.ApplySettings();
        });

        Button closeBtn = closeButton.GetComponent<Button>();
        closeBtn.onClick.RemoveAllListeners();
        closeBtn.onClick.AddListener(() =>
        {
            settingsMenu.CloseSettings();
        });

        // Изначально скрываем панель
        settingsPanel.SetActive(false);

        // Находим MainMenu и привязываем SettingsMenu
        MainMenu mainMenu = Object.FindFirstObjectByType<MainMenu>();
        if (mainMenu != null)
        {
            var settingsMenuField = typeof(MainMenu).GetField("settingsMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (settingsMenuField != null)
            {
                settingsMenuField.SetValue(mainMenu, settingsMenu);
            }
            EditorUtility.SetDirty(mainMenu);
            Debug.Log("MainMenu привязан к SettingsMenu");
        }

        EditorUtility.SetDirty(settingsPanel);
        Debug.Log("Settings UI создан успешно! Панель: " + settingsPanel.name);
    }

    private static GameObject CreateTextObject(string name, string text, GameObject parent,
        Vector2 position, Vector2 size, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = color;

        return textObj;
    }

    private static GameObject CreateDropdownObject(string name, GameObject parent,
        Vector2 position, Vector2 size)
    {
        GameObject dropdownObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        dropdownObj.transform.SetParent(parent.transform, false);

        RectTransform rect = dropdownObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        Image image = dropdownObj.GetComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Создаём TMP_Dropdown
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = image;

        // Заполняем разрешениями
        Resolution[] resolutions = Screen.resolutions;
        dropdown.ClearOptions();

        int defaultIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                defaultIndex = i;
            }
        }

        dropdown.value = defaultIndex;
        dropdown.RefreshShownValue();

        return dropdownObj;
    }

    private static GameObject CreateButtonObject(string name, string text, GameObject parent,
        Vector2 position, Vector2 size, string spritePath)
    {
        GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(Image));
        buttonObj.transform.SetParent(parent.transform, false);

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;

        Image image = buttonObj.GetComponent<Image>();
        image.color = Color.white;

        // Пробуем загрузить спрайт
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        else
        {
            image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        }

        Button button = buttonObj.GetComponent<Button>();
        button.targetGraphic = image;

        // Текст кнопки
        GameObject btnText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnText.transform.SetParent(buttonObj.transform, false);
        btnText.GetComponent<RectTransform>().sizeDelta = size;
        btnText.GetComponent<RectTransform>().localScale = Vector3.one;

        TextMeshProUGUI textComponent = btnText.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 18;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;

        return buttonObj;
    }
}
#endif

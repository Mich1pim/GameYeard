# Инструкция по настройке меню настроек

## Способ 1: Автоматическая настройка (Рекомендуется) ✨

### Шаг 1: Открыть сцену Menu
1. В Unity Editor откройте `Assets/Scenes/Menu.unity`

### Шаг 2: Запустить автосетуп
1. В меню Unity выберите: **Tools → Setup Resolution Settings UI**
2. Проверьте консоль (Ctrl+Shift+C) на наличие сообщений
3. Должны появиться зеленые галочки ✅

### Шаг 3: Сохранить сцену
1. Нажмите **Ctrl+S** для сохранения сцены

### Шаг 4: Проверить работу
1. Нажмите **Play** (Ctrl+P)
2. Нажмите на кнопку **Settings** в главном меню
3. Должно открыться меню настроек с Dropdown и кнопкой "Применить"
4. Выберите разрешение и нажмите "Применить"

---

## Способ 2: Ручная настройка 🔧

Если автоматическая настройка не сработала, выполните следующие шаги:

### Шаг 1: Открыть SettingsMenu для редактирования
1. Откройте сцену `Menu.unity`
2. В иерархии найдите `Canvas/SettingsMenu`
3. Временно активируйте его (поставьте галочку в Inspector)

### Шаг 2: Создать ResolutionDropdown
1. В Unity: **GameObject → UI → TMP Dropdown**
2. Переименуйте в `ResolutionDropdown`
3. Переместите внутрь `SettingsMenu`
4. Настройте RectTransform:
   ```
   Anchor: Middle Center
   Position: X=0, Y=200, Z=0
   Size: Width=400, Height=50
   ```

### Шаг 3: Создать ApplyButton
1. В Unity: **GameObject → UI → Button - TextMeshPro**
2. Переименуйте в `ApplyButton`
3. Переместите внутрь `SettingsMenu`
4. Настройте RectTransform:
   ```
   Anchor: Middle Center
   Position: X=-120, Y=-200, Z=0
   Size: Width=200, Height=60
   ```
5. Выберите дочерний `Text (TMP)` и измените текст на **"Применить"**
6. Настройте Image на кнопке (зеленый цвет)

### Шаг 4: Добавить скрипт SettingsMenu
1. Выберите объект `SettingsMenu`
2. В Inspector: **Add Component → SettingsMenu**

### Шаг 5: Настроить ссылки в Inspector
На компоненте **Settings Menu**:
- **Resolution Dropdown**: перетащите `ResolutionDropdown`
- **Apply Button**: перетащите `ApplyButton`
- **Close Button**: перетащите `CloseSettingsMenuButton`

### Шаг 6: Настроить SettingsButton
1. Выберите `SettingsButton` (в `Canvas/Buttons/SettingsButton`)
2. В компоненте **Button** найдите **On Click()**
3. Нажмите `+` и настройте:
   - Объект: `Canvas`
   - Функция: `MainMenu → OpenSettings()`

### Шаг 7: Настроить MainMenu
1. Выберите `Canvas`
2. В компоненте **Main Menu**:
   - **Settings Menu**: перетащите `SettingsMenu`

### Шаг 8: Настроить CloseSettingsMenuButton
1. Выберите `CloseSettingsMenuButton`
2. В компоненте **Button** → **On Click()**:
   - Нажмите `+`
   - Объект: `SettingsMenu`
   - Функция: `SettingsMenu → CloseSettings()`

### Шаг 9: Сохранить и проверить
1. Выйдите из Play Mode если активен
2. Деактивируйте `SettingsMenu` (снимите галочку)
3. Сохраните сцену (**Ctrl+S**)
4. Нажмите **Play** и проверьте работу

---

## Функционал

### Что реализовано:
✅ Автоматическое получение всех доступных разрешений системы
✅ Фильтрация по частоте (60Hz и выше)
✅ Удаление дубликатов
✅ Dropdown с красивым отображением (1920x1080 @60Hz)
✅ Кнопка применения настроек
✅ Сохранение в PlayerPrefs
✅ Автоматическая загрузка сохраненных настроек
✅ Кнопка закрытия меню

### Сохраненные данные:
- `ResolutionIndex` - индекс разрешения
- `ScreenWidth` - ширина
- `ScreenHeight` - высота
- `ScreenRefreshRate` - частота обновления

---

## Troubleshooting

### ❌ Dropdown не показывает разрешения
- Убедитесь что `resolutionDropdown` назначен в Inspector
- Проверьте консоль на ошибки
- Убедитесь что в системе есть разрешения 60Hz+

### ❌ Кнопки не работают
- Проверьте что все ссылки назначены в Inspector
- Убедитесь что On Click() события настроены
- Проверьте что кнопки имеют компонент Button

### ❌ Настройки не сохраняются
- Проверьте консоль на ошибки
- Убедитесь что вызывается `PlayerPrefs.Save()`
- Проверьте что нет ошибок в скрипте

### ❌ SettingsMenu не открывается
- Проверьте что SettingsButton имеет событие On Click
- Убедитесь что MainMenu.settingsMenu назначен
- Проверьте что SettingsMenu деактивирован по умолчанию

---

## Файлы проекта

### Созданные файлы:
- `Assets/Script/UI/SettingsMenu.cs` - основной скрипт настроек
- `Assets/Script/UI/MainMenu.cs` - обновленный (добавлены методы Open/Close)
- `Assets/Script/Editor/ResolutionSettingsSetup.cs` - автоматическая настройка
- `SETTINGS_SETUP.md` - эта инструкция

---

## Дополнительная информация

### Загрузка настроек при старте игры
Чтобы автоматически применять сохраненные настройки при запуске, добавьте в `MainMenu.cs`:

```csharp
private void Awake()
{
    LoadSavedResolution();
}

private void LoadSavedResolution()
{
    int width = PlayerPrefs.GetInt("ScreenWidth", Screen.currentResolution.width);
    int height = PlayerPrefs.GetInt("ScreenHeight", Screen.currentResolution.height);
    
    if (width != Screen.currentResolution.width || height != Screen.currentResolution.height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        Debug.Log($"Загружено разрешение: {width}x{height}");
    }
}
```

---

## Готово! 🎉

Теперь у вас есть полнофункциональное меню настроек с выбором разрешения экрана!

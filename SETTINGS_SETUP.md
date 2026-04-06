# Настройка панели настроек в меню Menu

## ✅ Что уже сделано:

### 1. Обновлены скрипты:
- ✅ **MainMenu.cs** - добавлено поле `settingsMenu` и метод `OpenSettings()`
- ✅ **SetupSettingsUI.cs** - обновлён скрипт для автоматического создания UI
- ✅ **Settings.cs** - уже реализована работа с разрешениями
- ✅ **SettingsMenu.cs** - уже реализована логика управления настройками

### 2. Структура сцены Menu:
- ✅ **Canvas** - основной UI контейнер
- ✅ **Buttons** - контейнер с кнопками
  - `PlayButton` - начать игру
  - `ExitButton` - выйти
  - `SettingButton` - открыть настройки (уже есть!)
- ✅ **Setting** - панель настроек (ID: 1024014750), сейчас пустая и неактивная

---

## 🔧 Как настроить UI панели настроек:

### Способ 1: Автоматическая настройка через меню Unity

1. **Откройте Unity Editor** с проектом GameYear
2. **Откройте сцену Menu**: `Assets/Scenes/Menu.unity`
3. **Запустите автоматическую настройку**:
   - В верхнем меню Unity выберите: `Tools > Setup Settings UI in Menu`
4. **Проверьте результат**:
   - Панель "Setting" должна заполниться UI элементами
   - В консоли появится сообщение: "Settings UI создан успешно!"

### Способ 2: Ручная настройка (если автоматическая не сработала)

1. **Откройте сцену Menu** в Unity Editor
2. **Найдите объект "Setting"** в иерархии
3. **Добавьте компонент SettingsMenu**:
   - Выделите объект "Setting"
   - Add Component → `SettingsMenu`
4. **Создайте дочерние элементы**:
   - **TitleText** (TextMeshProUGUI) - текст "НАСТРОЙКИ"
   - **ResolutionLabel** (TextMeshProUGUI) - текст "Разрешение экрана:"
   - **ResolutionDropdown** (TMP_Dropdown) - выпадающий список разрешений
   - **FullscreenLabel** (TextMeshProUGUI) - текст "Полноэкранный режим:"
   - **FullscreenToggle** (Toggle) - переключатель полноэкранного режима
   - **ApplyButton** (Button) - текст "ПРИМЕНИТЬ"
   - **CloseButton** (Button) - текст "ЗАКРЫТЬ"
5. **Привяжите элементы в SettingsMenu**:
   - Settings Panel → объект "Setting"
   - Resolution Dropdown → объект "ResolutionDropdown"
   - Fullscreen Toggle → объект "FullscreenToggle"
6. **Настройте кнопки**:
   - **SettingButton.onClick** → MainMenu.OpenSettings()
   - **ApplyButton.onClick** → SettingsMenu.ApplySettings()
   - **CloseButton.onClick** → SettingsMenu.CloseSettings()

---

## 🎮 Как это работает:

### Поток пользователя:
1. Игрок запускает игру
2. В главном меню нажимает **"Настройки"** (SettingButton)
3. Открывается панель "Setting" с:
   - Выпадающим списком доступных разрешений
   - Переключателем полноэкранного режима
   - Кнопкой "Применить"
   - Кнопкой "Закрыть"
4. Игрок выбирает нужное разрешение
5. Нажимает **"Применить"** - разрешение меняется и сохраняется
6. Нажимает **"Закрыть"** - панель закрывается

### Технические детали:
- **Settings.cs** сохраняет настройки в PlayerPrefs
- При запуске игры автоматически применяется последнее разрешение
- Доступные разрешения получаются из `Screen.resolutions`

---

## 🐛 Возможные проблемы и решения:

### Проблема: "Canvas не найден в сцене!"
**Решение:** Убедитесь, что в сцене есть объект с компонентом Canvas

### Проблема: "Панель 'Setting' не найдена в сцене!"
**Решение:** Убедитесь, что в сцене есть объект с именем "Setting" (ID: 1024014750)

### Проблема: Dropdown пустой
**Решение:** Проверьте, что в системе есть доступные разрешения (`Screen.resolutions`)

### Проблема: Кнопка "Настройки" не работает
**Решение:** 
1. Проверьте, что на MainMenu есть компонент SettingsMenu
2. Проверьте, что SettingButton.onClick привязан к MainMenu.OpenSettings()

---

## 📝 После настройки:

1. **Сохраните сцену** (Ctrl+S)
2. **Запустите игру** (Play Mode)
3. **Проверьте работу**:
   - Нажмите "Настройки"
   - Выберите другое разрешение
   - Нажмите "Применить"
   - Проверьте, что разрешение изменилось
   - Нажмите "Закрыть"
   - Перезапустите игру - настройки должны сохраниться

---

## 📂 Расположение файлов:

- **Скрипты:**
  - `c:\UnityGames\GameYear\Assets\Script\UI\MainMenu.cs`
  - `c:\UnityGames\GameYear\Assets\Script\UI\Settings.cs`
  - `c:\UnityGames\GameYear\Assets\Script\UI\SettingsMenu.cs`
  - `c:\UnityGames\GameYear\Assets\Script\UI\SetupSettingsUI.cs`

- **Сцена:**
  - `c:\UnityGames\GameYear\Assets\Scenes\Menu.unity`

---

**Дата создания:** 6 апреля 2026 г.
**Статус:** ✅ Готово к тестированию

# Настройка MCP серверов для Unity

## Текущие серверы

### 1. Ваш текущий сервер (mcp-for-unity-server)
- **Порт:** 8080
- **URL:** `http://127.0.0.1:8080/mcp`
- **Статус:** ✅ Работает

### 2. Ivan Murzak Unity MCP (~147 инструментов)
- **Порт:** 24997
- **URL:** `http://localhost:24997`
- **Статус:** ✅ Работает
- **Возможности:**
  - Управление ассетами, сценами, GameObject, компонентами
  - Работа со скриптами, пакетами и тестами
  - Рефлексия и доступ к коду (включая приватные методы)
  - Генерация навыков (Skills)
  - Редактор и Runtime поддержка

### 3. Unity Prefab Parser MCP (анализ префабов)
- **Тип:** stdio (локальный)
- **Путь:** `c:\UnityGames\Unity-Prefab-Parser-MCP\dist\index.js`
- **Статус:** ✅ Готов к использованию
- **Возможности:**
  - Анализ префабов и .unity файлов
  - Вывод только видимых в Inspector свойств (экономия 70-90% токенов)
  - Поддержка Prefab Variants
  - Разрешение ссылок (GUID → имена ассетов)
  - Умная фильтрация и маппинг слоёв

---

## Итоговая конфигурация (settings.json)

```json
{
  "mcpServers": {
    "unityMCP": {
      "url": "http://127.0.0.1:8080/mcp",
      "type": "http"
    },
    "ivanMurzakMCP": {
      "url": "http://localhost:24997",
      "type": "http"
    },
    "prefabParserMCP": {
      "command": "node",
      "args": ["c:\\UnityGames\\Unity-Prefab-Parser-MCP\\dist\\index.js"],
      "type": "stdio"
    }
  }
}
```

---

## Порты серверов
- **8080** - Ваш текущий сервер (mcp-for-unity-server)
- **24997** - Ivan Murzak Unity MCP
- **stdio** - Prefab Parser (не использует порт, работает через stdin/stdout)

---

## Расположение репозиториев
- **Ivan Murzak Unity MCP:** `c:\UnityGames\Unity-MCP-IvanMurzak`
- **Unity Prefab Parser MCP:** `c:\UnityGames\Unity-Prefab-Parser-MCP`
- **Плагин в проекте:** `c:\UnityGames\GameYear\Packages` (через OpenUPM)

---

## Проверка работы

```bash
# Проверить порт 8080 (текущий сервер)
netstat -ano | findstr 8080

# Проверить порт 24997 (Ivan Murzak)
netstat -ano | findstr 24997

# Проверить статус через CLI
unity-mcp-cli status
```

---

## Управление серверами

### Ivan Murzak MCP
- **Запуск:** Откройте Unity Editor → `Window > AI Game Developer`
- **Остановка:** Закройте Unity Editor или используйте `Auto-generate Skills`
- **Обновление:** `unity-mcp-cli update`

### Prefab Parser MCP
- Запускается автоматически при вызове инструмента AI
- Не требует отдельного запуска

---

## Доступные инструменты

### Ivan Murzak Unity MCP (~147 инструментов)
- `manage_editor` - управление редактором (play/stop/pause)
- `manage_scene` - управление сценами
- `manage_gameobject` - работа с GameObjects
- `manage_asset` - управление ассетами
- `manage_script` - работа со скриптами
- `manage_package` - управление пакетами
- `run_tests` - запуск тестов
- И многие другие...

### Prefab Parser MCP
- `unity-parser_read_unity_file` - анализ префабов и .unity файлов

---

## Последнее обновление
**6 апреля 2026 г.** - Все серверы настроены и готовы к работе

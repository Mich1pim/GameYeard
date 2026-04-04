# Unity MCP Server - Быстрый доступ

## MCP Сервер
- **URL:** `http://127.0.0.1:8080/mcp`
- **Название:** `mcp-for-unity-server`
- **Версия:** `3.2.0`
- **Тип:** HTTP (Server-Sent Events)

## Инициализация
```json
POST http://127.0.0.1:8080/mcp
Content-Type: application/json
Accept: application/json, text/event-stream

{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"qwen","version":"1.0.0"}},"id":1}
```

## Заголовки для всех запросов
```
Content-Type: application/json
Accept: application/json, text/event-stream
```

## Основные инструменты (~40 доступно)

### Editor
- `manage_editor` - управление редактором
  - `action: play` - войти в play mode
  - `action: stop` - выйти из play mode
  - `action: pause` - пауза

### Сцены
- `manage_scene` - управление сценами
  - `action: get_active_scene`
  - `action: load_scene`
  - `action: get_scene_objects`

### GameObjects
- `manage_gameobject` - работа с объектами
  - `action: create`
  - `action: delete`
  - `action: modify`
  - `action: get_component`

### Ассеты
- `manage_asset` - управление ассетами
  - `action: import`
  - `action: find`
  - `action: get_info`

### Консоль и тесты
- `read_console` - чтение логов Unity
- `run_tests` - запуск тестов

## Быстрые команды

### Запустить игру
```json
{"jsonrpc":"2.0","method":"tools/call","params":{"name":"manage_editor","arguments":{"action":"play"}},"id":N}
```

### Остановить игру
```json
{"jsonrpc":"2.0","method":"tools/call","params":{"name":"manage_editor","arguments":{"action":"stop"}},"id":N}
```

### Получить список инструментов
```json
{"jsonrpc":"2.0","method":"tools/list","id":N}
```

## Примеры использования
1. **Запуск игры:** initialize → tools/call (manage_editor, play)
2. **Чтение логов:** initialize → tools/call (read_console)
3. **Работа со сценой:** initialize → tools/call (manage_scene)

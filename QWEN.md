# Unity MCP Server - Быстрый доступ

## Основной MCP Сервер (Project Unity)
- **URL:** `http://localhost:24997`
- **Название:** `unity-mcp-server`
- **Версия:** `0.63.3.0`
- **Тип:** HTTP (Server-Sent Events)
- **Протокол:** `2024-11-05`
- **Сессия:** Требует `Mcp-Session-Id` заголовок после инициализации
- **Возможности:** logging, prompts, resources, tools

### Инициализация основного сервера
```
POST http://localhost:24997
Content-Type: application/json
Accept: application/json, text/event-stream

{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"qwen","version":"1.0.0"}},"id":1}
```

После инициализации: сохранить `Mcp-Session-Id` из заголовков ответа и использовать в последующих запросах.

---

## Альтернативный MCP Сервер (Legacy)
- **URL:** `http://127.0.0.1:8080/mcp`
- **Название:** `mcp-for-unity-server`
- **Версия:** `3.2.0`
- **Тип:** HTTP (Server-Sent Events)
- **Сессия:** Не требует

### Инициализация альтернативного сервера
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

---

## Word Document MCP Server
- **Путь:** `c:\UnityGames\Office-Word-MCP-Server`
- **URL:** `http://127.0.0.1:24998/mcp`
- **Название:** `Word Document Server`
- **Версия:** `3.2.3`
- **Протокол:** MCP `2024-11-05`
- **Транспорт:** Streamable HTTP
- **Сессия:** Требуется (Mcp-Session-Id)
- **Инструменты:** 54 (создание, редактирование, форматирование .docx)

### Запуск сервера
```powershell
cd c:\UnityGames\Office-Word-MCP-Server
$env:MCP_TRANSPORT='streamable-http'
$env:MCP_HOST='127.0.0.1'
$env:MCP_PORT='24998'
$env:MCP_PATH='/mcp'
python word_mcp_server.py
```

### Основные инструменты
- `create_document` - создать документ
- `add_paragraph`, `add_heading` - добавить текст/заголовок
- `add_table` - добавить таблицу
- `format_text`, `format_table` - форматирование
- `search_and_replace` - поиск и замена
- `convert_to_pdf` - конвертация в PDF
- `protect_document` - защита документа
- `get_document_info` - информация о документе

### Инициализация
```json
POST http://127.0.0.1:24998/mcp
Content-Type: application/json
Accept: application/json, text/event-stream

{"jsonrpc":"2.0","method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"qwen","version":"1.0.0"}},"id":1}
```

---

## Примеры использования
1. **Запуск игры:** initialize → tools/call (manage_editor, play)
2. **Чтение логов:** initialize → tools/call (read_console)
3. **Работа со сценой:** initialize → tools/call (manage_scene)
4. **Работа с Word:** initialize → tools/call (create_document/add_paragraph/...)

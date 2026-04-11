import urllib.request
import json

BASE_URL = "http://127.0.0.1:8080/mcp"
session_id = None

def mcp(method, params=None, id=1):
    global session_id
    payload = {"jsonrpc": "2.0", "method": method, "params": params or {}, "id": id}
    data = json.dumps(payload).encode('utf-8')
    req = urllib.request.Request(BASE_URL, data=data, headers={"Content-Type": "application/json", "Accept": "application/json, text/event-stream"}, method="POST")
    if session_id:
        req.add_header("Mcp-Session-Id", session_id)
    with urllib.request.urlopen(req, timeout=15) as response:
        session_id = response.getheader("Mcp-Session-Id") or session_id
        content = response.read().decode('utf-8')
        if "data:" in content:
            for line in content.split('\n'):
                if line.startswith('data:'):
                    return json.loads(line[5:].strip())
        return json.loads(content)

def show(title, resp):
    err = resp.get('isError')
    content = resp.get('result', {}).get('content', [{}])[0].get('text', '')
    print(f"\n{'='*60}")
    print(f"{title} {'❌' if err else '✅'}")
    print('='*60)
    print(content[:600])

# Инициализация
mcp("initialize", {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "qwen", "version": "1.0.0"}}, id=1)
print("MCP сервер инициализирован ✅")

# 1. Создать пустой GameObject для Dropdown
show("Создание ResolutionDropdown", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "create",
        "name": "ResolutionDropdown",
        "parent": "Canvas/SettingsMenu"
    }
}, id=2))

# 2. Добавить TMP_Dropdown компонент
show("Добавление TMP_Dropdown", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "modify",
        "target": "Canvas/SettingsMenu/ResolutionDropdown",
        "addComponent": "TMPro.TMP_Dropdown"
    }
}, id=3))

# 3. Создать ApplyButton
show("Создание ApplyButton", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "create",
        "name": "ApplyButton",
        "parent": "Canvas/SettingsMenu"
    }
}, id=4))

# 4. Добавить Button компонент
show("Добавление Button", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "modify",
        "target": "Canvas/SettingsMenu/ApplyButton",
        "addComponent": "UnityEngine.UI.Button"
    }
}, id=5))

# 5. Создать Image для кнопки
show("Добавление Image на кнопку", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "modify",
        "target": "Canvas/SettingsMenu/ApplyButton",
        "addComponent": "UnityEngine.UI.Image"
    }
}, id=6))

# 6. Создать текст для ApplyButton
show("Создание Text для ApplyButton", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "create",
        "name": "Text (TMP)",
        "parent": "Canvas/SettingsMenu/ApplyButton"
    }
}, id=7))

# 7. Добавить TextMeshProUGUI
show("Добавление TextMeshProUGUI", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "modify",
        "target": "Canvas/SettingsMenu/ApplyButton/Text (TMP)",
        "addComponent": "TMPro.TextMeshProUGUI"
    }
}, id=8))

print("\n" + "="*60)
print("UI элементы созданы!")
print("="*60)
print("Далее: настройте позиции и связи в Unity Editor")

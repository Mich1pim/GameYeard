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
    print(f"\n{title} {'❌' if err else '✅'}")
    print(content[:1000])

# Инициализация
mcp("initialize", {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "qwen", "version": "1.0.0"}}, id=1)

# 1. Добавить TMP_Dropdown компонент на ResolutionDropdown
print("="*60)
print("Добавление компонентов на ResolutionDropdown")
print("="*60)
show("Dropdown", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "modify",
        "target": "Canvas/SettingsMenu/ResolutionDropdown",
        "addComponent": "Dropdown"
    }
}, id=2))

# 2. Добавить Button на ApplyButton
show("Button", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "modify",
        "target": "Canvas/SettingsMenu/ApplyButton",
        "addComponent": "Button"
    }
}, id=3))

# 3. Добавить TextMeshProUGUI на Text
show("TextMeshProUGUI", mcp("tools/call", {
    "name": "manage_gameobject",
    "arguments": {
        "action": "modify",
        "target": "Canvas/SettingsMenu/ApplyButton/Text (TMP)",
        "addComponent": "TextMeshProUGUI"
    }
}, id=4))

print("\nГотово!")

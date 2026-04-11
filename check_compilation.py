import urllib.request
import json
import time

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

# Инициализация
print("🔌 Инициализация MCP сервера...")
mcp("initialize", {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "qwen", "version": "1.0.0"}}, id=1)
print("✅ Сервер подключен")

# Подождать компиляцию
print("\n⏳ Ожидание компиляции скриптов...")
time.sleep(3)

# Проверить консоль
print("\n📋 Проверка консоли Unity на ошибки:")
console = mcp("tools/call", {
    "name": "read_console",
    "arguments": {
        "log_type": "Error",
        "count": 10
    }
}, id=2)

content = console.get('result', {}).get('content', [{}])[0].get('text', '')
if "no logs" in content.lower() or content.strip() == "":
    print("✅ Ошибок компиляции нет!")
else:
    print("❌ Найдены ошибки:")
    print(content[:1000])

# Проверить warnings
print("\n⚠️ Проверка предупреждений:")
warnings = mcp("tools/call", {
    "name": "read_console",
    "arguments": {
        "log_type": "Warning",
        "count": 5
    }
}, id=3)

warn_content = warnings.get('result', {}).get('content', [{}])[0].get('text', '')
if "no logs" in warn_content.lower() or warn_content.strip() == "":
    print("✅ Предупреждений нет!")
else:
    print("⚠️ Предупреждения:")
    print(warn_content[:800])

print("\n" + "="*60)
print("✨ Готово! Можете открыть Unity и использовать автосетуп:")
print("   Tools → Setup Resolution Settings UI")
print("="*60)

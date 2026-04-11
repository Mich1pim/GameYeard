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

# Инициализация
mcp("initialize", {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "qwen", "version": "1.0.0"}}, id=1)

# Получить детей Canvas
result = mcp("tools/call", {
    "name": "manage_scene",
    "arguments": {
        "action": "get_hierarchy",
        "cursor": "0",
        "page_size": 20,
        "parent_path": "Canvas"
    }
}, id=2)

content = result.get('result', {}).get('content', [{}])[0].get('text', '')
print("Raw content:")
print(content[:500])
print("="*80)

# Пробуем распарсить
try:
    data = json.loads(content).get('data', {})
    items = data.get('items', [])
    for item in items:
        print(f"📁 {item['name']}")
        print(f"   Path: {item['path']}")
        print(f"   Active: {item.get('activeInHierarchy', False)}")
        print(f"   Children: {item.get('childCount', 0)}")
        print(f"   Components: {', '.join(item.get('componentTypes', []))}")
        print()
except Exception as e:
    print(f"Ошибка парсинга: {e}")

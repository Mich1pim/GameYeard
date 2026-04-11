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

# Получить всю иерархию
result = mcp("tools/call", {
    "name": "manage_scene",
    "arguments": {"action": "get_hierarchy", "page_size": 100}
}, id=2)

content = result.get('result', {}).get('content', [{}])[0].get('text', '')
print("Иерархия сцены Menu:")
print("="*80)
print(content[:2000])

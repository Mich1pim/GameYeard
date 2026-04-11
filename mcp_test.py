import urllib.request
import json
import re

BASE_URL = "http://127.0.0.1:8080/mcp"

def mcp_request(method, params=None, id=1, session_id=None):
    """Отправить MCP запрос"""
    payload = {
        "jsonrpc": "2.0",
        "method": method,
        "params": params or {},
        "id": id
    }
    
    data = json.dumps(payload).encode('utf-8')
    req = urllib.request.Request(
        BASE_URL,
        data=data,
        headers={
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream"
        },
        method="POST"
    )
    
    if session_id:
        req.add_header("Mcp-Session-Id", session_id)
    
    with urllib.request.urlopen(req, timeout=10) as response:
        # Сохраняем Session-ID из заголовков
        new_session_id = response.getheader("Mcp-Session-Id")
        
        content = response.read().decode('utf-8')
        
        # Извлекаем JSON из SSE формата
        if "data:" in content:
            # SSE формат
            for line in content.split('\n'):
                if line.startswith('data:'):
                    json_data = json.loads(line[5:].strip())
                    return json_data, new_session_id
        else:
            return json.loads(content), new_session_id

# Инициализация
print("Инициализация сервера...")
init_response, session_id = mcp_request("initialize", {
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {"name": "qwen", "version": "1.0.0"}
}, id=1)
print(f"Session ID: {session_id}")
print(f"Результат инициализации: {init_response.get('result', {}).get('serverInfo', {}).get('name', 'Unknown')}")

# Получение состояния редактора
print("\nПолучение состояния редактора...")
editor_state, _ = mcp_request("tools/call", {
    "name": "manage_editor",
    "arguments": {"action": "get_state"}
}, id=2, session_id=session_id)
print(f"Ответ: {json.dumps(editor_state, indent=2, ensure_ascii=False)[:800]}")

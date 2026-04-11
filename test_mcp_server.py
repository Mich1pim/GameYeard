import urllib.request
import json
import http.client
import time

# Тестируем основной сервер (24997)
BASE_URL = "http://localhost:24997"

def make_request(method, params=None, id=1, session_id=None):
    """Отправить MCP запрос с правильными заголовками"""
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
            "Accept": "text/event-stream",  # ВАЖНО! Сервер требует этот заголовок
        },
        method="POST"
    )
    
    if session_id:
        req.add_header("Mcp-Session-Id", session_id)
    
    with urllib.request.urlopen(req, timeout=15) as response:
        new_session_id = response.getheader("Mcp-Session-Id")
        content = response.read().decode('utf-8')
        
        # Парсим SSE формат
        if "data:" in content:
            for line in content.split('\n'):
                if line.startswith('data:'):
                    return json.loads(line[5:].strip()), new_session_id
        else:
            return json.loads(content), new_session_id

def test_initialize():
    print("="*80)
    print("ТЕСТ 1: Инициализация сервера")
    print("="*80)
    
    result, session_id = make_request("initialize", {
        "protocolVersion": "2024-11-05",
        "capabilities": {},
        "clientInfo": {"name": "test-client", "version": "1.0.0"}
    }, id=1)
    
    if result.get('error'):
        print(f"❌ Ошибка: {result['error']}")
        return None
    
    server_info = result.get('result', {}).get('serverInfo', {})
    print(f"✅ Сервер: {server_info.get('name')} v{server_info.get('version')}")
    print(f"✅ Session ID: {session_id}")
    return session_id

def test_tools_list(session_id):
    print("\n" + "="*80)
    print("ТЕСТ 2: Список инструментов")
    print("="*80)
    
    result, _ = make_request("tools/list", id=2, session_id=session_id)
    
    if result.get('error'):
        print(f"❌ Ошибка: {result['error']}")
        return
    
    tools = result.get('result', {}).get('tools', [])
    print(f"✅ Найдено {len(tools)} инструментов")
    
    # Показать инструменты связанные с gameobject и scene
    print("\nКлючевые инструменты:")
    for tool in tools:
        name = tool['name']
        if 'gameobject' in name.lower() or 'scene' in name.lower() or 'editor' in name.lower():
            desc = tool.get('description', '')[:100]
            print(f"  • {name}")
            print(f"    {desc}")
            print()

def test_editor_state(session_id):
    print("\n" + "="*80)
    print("ТЕСТ 3: Состояние редактора")
    print("="*80)
    
    result, _ = make_request("tools/call", {
        "name": "manage_editor",
        "arguments": {"action": "play"}
    }, id=3, session_id=session_id)
    
    print(json.dumps(result, indent=2, ensure_ascii=False)[:800])

def test_scene_get(session_id):
    print("\n" + "="*80)
    print("ТЕСТ 4: Получить активную сцену")
    print("="*80)
    
    result, _ = make_request("tools/call", {
        "name": "manage_scene",
        "arguments": {"action": "get_active_scene"}
    }, id=4, session_id=session_id)
    
    if result.get('isError'):
        print(f"❌ Ошибка: {json.dumps(result, indent=2, ensure_ascii=False)[:500]}")
    else:
        print(f"✅ Успех: {json.dumps(result, indent=2, ensure_ascii=False)[:800]}")

# Запуск тестов
if __name__ == "__main__":
    print("🔍 Диагностика основного Unity MCP сервера (порт 24997)\n")
    
    session_id = test_initialize()
    
    if session_id:
        test_tools_list(session_id)
        test_scene_get(session_id)
        
        print("\n" + "="*80)
        print("ИТОГ: ✅ Сервер работает корректно!")
        print("="*80)
    else:
        print("\n" + "="*80)
        print("ИТОГ: ❌ Сервер не работает")
        print("="*80)

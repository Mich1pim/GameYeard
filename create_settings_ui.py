import urllib.request
import json
import time

BASE_URL = "http://localhost:24997"

def mcp(method, params=None, id=1, session_id=None):
    """Отправить MCP запрос на основной сервер (24997)"""
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
            "Accept": "application/json, text/event-stream",  # ОБА значения!
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

def show(title, result):
    """Показать результат"""
    print(f"\n{'='*60}")
    print(f"{title}")
    print('='*60)
    
    if result.get('error'):
        print(f"❌ Ошибка: {result['error']}")
    elif result.get('result'):
        print(json.dumps(result['result'], indent=2, ensure_ascii=False)[:1000])
    else:
        print(json.dumps(result, indent=2, ensure_ascii=False)[:1000])

# ============ ОСНОВНОЙ СКРИПТ СОЗДАНИЯ НАСТРОЕК ============

def main():
    print("🔧 Настройка разрешения экрана через Unity MCP (порт 24997)")
    print("="*80)
    
    # 1. Инициализация
    print("\n1️⃣ Инициализация сервера...")
    result, session_id = mcp("initialize", {
        "protocolVersion": "2024-11-05",
        "capabilities": {},
        "clientInfo": {"name": "setup-client", "version": "1.0.0"}
    }, id=1)
    
    server_info = result.get('result', {}).get('serverInfo', {})
    print(f"✅ Подключено к: {server_info.get('name')} v{server_info.get('version')}")
    print(f"📋 Session ID: {session_id}")
    
    # 2. Получить список инструментов
    print("\n2️⃣ Получение списка инструментов...")
    tools_result, _ = mcp("tools/list", session_id=session_id, id=2)
    tools = tools_result.get('result', {}).get('tools', [])
    print(f"✅ Найдено {len(tools)} инструментов")
    
    # Найти нужные инструменты
    gameobject_tools = [t for t in tools if 'gameobject' in t['name'].lower()]
    print(f"\n📦 GameObject инструменты: {[t['name'] for t in gameobject_tools]}")
    
    # 3. Загрузить сцену Menu
    print("\n3️⃣ Загрузка сцены Menu...")
    scene_result, _ = mcp("tools/call", {
        "name": "manage_scene",
        "arguments": {
            "action": "load_scene",
            "sceneName": "Menu"
        }
    }, session_id=session_id, id=3)
    print(f"📄 Сцена: {json.dumps(scene_result, indent=2, ensure_ascii=False)[:300]}")
    
    # 4. Получить иерархию сцены
    print("\n4️⃣ Получение иерархии Canvas...")
    hierarchy_result, _ = mcp("tools/call", {
        "name": "manage_scene",
        "arguments": {
            "action": "get_hierarchy",
            "page_size": 100
        }
    }, session_id=session_id, id=4)
    
    content = hierarchy_result.get('result', {}).get('content', [{}])[0].get('text', '')
    try:
        data = json.loads(content).get('data', {})
        canvas = next((item for item in data.get('items', []) if item['name'] == 'Canvas'), None)
        if canvas:
            print(f"✅ Canvas найден (ID: {canvas['instanceID']}, дети: {canvas.get('childCount', 0)})")
    except:
        print(f"ℹ️ Данные: {content[:300]}")
    
    # 5. Найти SettingsMenu
    print("\n5️⃣ Поиск SettingsMenu...")
    settings_menu_result, _ = mcp("tools/call", {
        "name": "manage_gameobject",
        "arguments": {
            "action": "find",
            "name": "SettingsMenu"
        }
    }, session_id=session_id, id=5)
    
    if not settings_menu_result.get('isError'):
        print(f"✅ SettingsMenu найден")
    else:
        print(f"❌ SettingsMenu не найден или ошибка")
    
    # 6. Создать ResolutionDropdown
    print("\n6️⃣ Создание ResolutionDropdown...")
    dropdown_result, _ = mcp("tools/call", {
        "name": "manage_gameobject",
        "arguments": {
            "action": "create",
            "name": "ResolutionDropdown",
            "parent": "Canvas/SettingsMenu"
        }
    }, session_id=session_id, id=6)
    
    if dropdown_result.get('isError'):
        print(f"❌ Ошибка: {json.dumps(dropdown_result, indent=2, ensure_ascii=False)[:300]}")
    else:
        print(f"✅ ResolutionDropdown создан")
    
    # 7. Создать ApplyButton
    print("\n7️⃣ Создание ApplyButton...")
    button_result, _ = mcp("tools/call", {
        "name": "manage_gameobject",
        "arguments": {
            "action": "create",
            "name": "ApplyButton",
            "parent": "Canvas/SettingsMenu"
        }
    }, session_id=session_id, id=7)
    
    if button_result.get('isError'):
        print(f"❌ Ошибка: {json.dumps(button_result, indent=2, ensure_ascii=False)[:300]}")
    else:
        print(f"✅ ApplyButton создан")
    
    # 8. Создать текст для кнопки
    print("\n8️⃣ Создание текста для ApplyButton...")
    text_result, _ = mcp("tools/call", {
        "name": "manage_gameobject",
        "arguments": {
            "action": "create",
            "name": "Text (TMP)",
            "parent": "Canvas/SettingsMenu/ApplyButton"
        }
    }, session_id=session_id, id=8)
    
    if text_result.get('isError'):
        print(f"❌ Ошибка: {json.dumps(text_result, indent=2, ensure_ascii=False)[:300]}")
    else:
        print(f"✅ Text создан")
    
    # 9. Добавить компонент Dropdown
    print("\n9️⃣ Добавление компонента Dropdown...")
    try:
        add_dropdown_result, _ = mcp("tools/call", {
            "name": "manage_gameobject",
            "arguments": {
                "action": "modify",
                "target": "Canvas/SettingsMenu/ResolutionDropdown",
                "addComponent": "TMPro.TMP_Dropdown"
            }
        }, session_id=session_id, id=9)
        print(f"ℹ️ Результат: {json.dumps(add_dropdown_result, indent=2, ensure_ascii=False)[:400]}")
    except Exception as e:
        print(f"⚠️ Ошибка добавления компонента (попробуем через Editor script): {e}")
    
    # 10. Добавить компонент Button
    print("\n🔟 Добавление компонента Button...")
    try:
        add_button_result, _ = mcp("tools/call", {
            "name": "manage_gameobject",
            "arguments": {
                "action": "modify",
                "target": "Canvas/SettingsMenu/ApplyButton",
                "addComponent": "UnityEngine.UI.Button"
            }
        }, session_id=session_id, id=10)
        print(f"ℹ️ Результат: {json.dumps(add_button_result, indent=2, ensure_ascii=False)[:400]}")
    except Exception as e:
        print(f"⚠️ Ошибка добавления компонента")
    
    # ФИНАЛ
    print("\n" + "="*80)
    print("✨ СОЗДАНИЕ ЗАВЕРШЕНО!")
    print("="*80)
    print("\n📝 Созданы GameObject:")
    print("  ✅ ResolutionDropdown (внутри SettingsMenu)")
    print("  ✅ ApplyButton (внутри SettingsMenu)")
    print("  ✅ Text (TMP) (внутри ApplyButton)")
    print("\n⚠️ Далее нужно:")
    print("  1. Добавить компоненты UI через Unity Editor или Editor script")
    print("  2. Запустить Tools → Setup Resolution Settings UI")
    print("  3. Сохранить сцену")
    
    return session_id

if __name__ == "__main__":
    main()

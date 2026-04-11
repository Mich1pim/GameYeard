import urllib.request
import json

BASE_URL = "http://localhost:24997"
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

def show(title, result):
    err = result.get('isError', False)
    content = result.get('result', {}).get('content', [{}])[0].get('text', '')
    print(f"\n{title} {'❌' if err else '✅'}")
    if content and len(content) < 2000:
        try:
            data = json.loads(content)
            print(json.dumps(data.get('result', data), indent=2, ensure_ascii=False)[:800])
        except:
            print(content[:500])

def main():
    print("="*80)
    print("Создание UI для настроек разрешения (Основной MCP сервер)")
    print("="*80)
    
    # 1. Инициализация
    print("\n1️⃣ Инициализация...")
    result = mcp("initialize", {
        "protocolVersion": "2024-11-05",
        "capabilities": {},
        "clientInfo": {"name": "setup", "version": "1.0"}
    }, id=1)
    print(f"✅ {result.get('result', {}).get('serverInfo', {}).get('name')} v{result.get('result', {}).get('serverInfo', {}).get('version')}")
    print(f"📋 Session: {session_id[:20]}...")
    
    # 2. Найти SettingsMenu
    print("\n2️⃣ Поиск SettingsMenu...")
    find_result = mcp("tools/call", {
        "name": "gameobject-find",
        "arguments": {
            "gameObjectRef": {"path": "Canvas/SettingsMenu"},
            "includeComponents": True
        }
    }, id=2)
    
    content = find_result.get('result', {}).get('content', [{}])[0].get('text', '')
    data = json.loads(content).get('result', {})
    settings_menu_id = data.get('Reference', {}).get('instanceID')
    components = data.get('Components', [])
    print(f"✅ SettingsMenu найден (ID: {settings_menu_id})")
    print(f"📦 Компоненты: {[c['typeName'] for c in components]}")
    
    # 3. Создать ResolutionDropdown
    print("\n3️⃣ Создание ResolutionDropdown...")
    dropdown_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "ResolutionDropdown",
            "parentGameObjectRef": {"instanceID": settings_menu_id}
        }
    }, id=3)
    
    dropdown_data = dropdown_result.get('result', {}).get('structuredContent', {}).get('result', {})
    dropdown_id = dropdown_data.get('instanceID')
    print(f"✅ ResolutionDropdown создан (ID: {dropdown_id})")
    
    # 4. Создать ApplyButton
    print("\n4️⃣ Создание ApplyButton...")
    button_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "ApplyButton",
            "parentGameObjectRef": {"instanceID": settings_menu_id}
        }
    }, id=4)
    
    button_data = button_result.get('result', {}).get('structuredContent', {}).get('result', {})
    button_id = button_data.get('instanceID')
    print(f"✅ ApplyButton создан (ID: {button_id})")
    
    # 5. Создать текст для кнопки
    print("\n5️⃣ Создание Text (TMP)...")
    text_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Text (TMP)",
            "parentGameObjectRef": {"instanceID": button_id}
        }
    }, id=5)
    
    text_data = text_result.get('result', {}).get('structuredContent', {}).get('result', {})
    text_id = text_data.get('instanceID')
    print(f"✅ Text создан (ID: {text_id})")
    
    # 6. Добавить компоненты на ResolutionDropdown
    print("\n6️⃣ Добавление компонентов на ResolutionDropdown...")
    show("RectTransform", mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": dropdown_id},
            "componentNames": ["UnityEngine.RectTransform"]
        }
    }, id=6))
    
    show("Image", mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": dropdown_id},
            "componentNames": ["UnityEngine.UI.Image"]
        }
    }, id=7))
    
    show("TMP_Dropdown", mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": dropdown_id},
            "componentNames": ["TMPro.TMP_Dropdown"]
        }
    }, id=8))
    
    # 7. Добавить компоненты на ApplyButton
    print("\n7️⃣ Добавление компонентов на ApplyButton...")
    show("RectTransform, Button, Image", mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": button_id},
            "componentNames": ["UnityEngine.RectTransform", "UnityEngine.UI.Button", "UnityEngine.UI.Image"]
        }
    }, id=9))
    
    # 8. Добавить TextMeshProUGUI на текст
    print("\n8️⃣ Добавление TextMeshProUGUI...")
    show("TextMeshProUGUI", mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": text_id},
            "componentNames": ["TMPro.TextMeshProUGUI"]
        }
    }, id=10))
    
    # ФИНАЛ
    print("\n" + "="*80)
    print("✨ UI ЭЛЕМЕНТЫ СОЗДАНЫ!")
    print("="*80)
    print("\n📋 Иерархия:")
    print(f"  SettingsMenu (ID: {settings_menu_id})")
    print(f"  ├── ResolutionDropdown (ID: {dropdown_id})")
    print(f"  │   ├── RectTransform")
    print(f"  │   ├── Image")
    print(f"  │   └── TMP_Dropdown")
    print(f"  └── ApplyButton (ID: {button_id})")
    print(f"      ├── RectTransform")
    print(f"      ├── Button")
    print(f"      ├── Image")
    print(f"      └── Text (TMP) (ID: {text_id})")
    print(f"          └── TextMeshProUGUI")
    print("\n⚠️ Далее:")
    print("  1. Откройте Unity Editor")
    print("  2. Запустите Tools → Setup Resolution Settings UI")
    print("  3. Сохраните сцену (Ctrl+S)")
    print("  4. Проверьте в Play Mode")

if __name__ == "__main__":
    main()

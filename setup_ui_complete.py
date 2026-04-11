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
    structured = result.get('result', {}).get('structuredContent', {})
    print(f"{title} {'❌' if err else '✅'}")
    if structured:
        print(f"   {json.dumps(structured, indent=2, ensure_ascii=False)[:400]}")

def main():
    print("="*80)
    print("Настройка UI элементов (создание дочерних объектов и связей)")
    print("="*80)
    
    # 1. Инициализация
    print("\n1️⃣ Инициализация...")
    mcp("initialize", {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "setup", "version": "1.0"}}, id=1)
    print(f"✅ Подключено, Session: {session_id[:20]}...")
    
    # 2. Найти существующие объекты
    print("\n2️⃣ Поиск объектов...")
    
    find_result = mcp("tools/call", {
        "name": "gameobject-find",
        "arguments": {"gameObjectRef": {"path": "Canvas/SettingsMenu/ResolutionDropdown"}}
    }, id=2)
    dropdown_id = find_result['result']['structuredContent']['result']['Reference']['instanceID']
    print(f"✅ ResolutionDropdown: {dropdown_id}")
    
    find_result = mcp("tools/call", {
        "name": "gameobject-find",
        "arguments": {"gameObjectRef": {"path": "Canvas/SettingsMenu/ApplyButton"}}
    }, id=3)
    button_id = find_result['result']['structuredContent']['result']['Reference']['instanceID']
    print(f"✅ ApplyButton: {button_id}")
    
    # Сначала создать текст
    print("\n  Создание Text (TMP)...")
    text_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Text (TMP)",
            "parentGameObjectRef": {"instanceID": button_id}
        }
    }, id=4)
    text_id = text_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": text_id},
            "componentNames": ["UnityEngine.RectTransform", "TMPro.TextMeshProUGUI"]
        }
    }, id=5)
    print(f"  ✅ Text (TMP) создан ({text_id})")
    
    # 4. Создать дочерние элементы для TMP_Dropdown
    print("\n4️⃣ Создание дочерних элементов для Dropdown...")
    
    # 4.1 Label (текст выбранного значения)
    print("  ├─ Label...")
    label_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Label",
            "parentGameObjectRef": {"instanceID": dropdown_id}
        }
    }, id=6)
    label_id = label_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": label_id},
            "componentNames": ["UnityEngine.RectTransform", "TMPro.TextMeshProUGUI"]
        }
    }, id=7)
    print(f"  ✅ Label создан ({label_id})")
    
    # 4.2 Arrow (иконка стрелки)
    print("  ├─ Arrow...")
    arrow_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Arrow",
            "parentGameObjectRef": {"instanceID": dropdown_id}
        }
    }, id=8)
    arrow_id = arrow_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": arrow_id},
            "componentNames": ["UnityEngine.RectTransform", "UnityEngine.UI.Image"]
        }
    }, id=9)
    print(f"  ✅ Arrow создан ({arrow_id})")
    
    # 4.3 Template (шаблон выпадающего списка)
    print("  ├─ Template...")
    template_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Template",
            "parentGameObjectRef": {"instanceID": dropdown_id}
        }
    }, id=10)
    template_id = template_result['result']['structuredContent']['result']['instanceID']
    
    # Добавить компоненты на Template
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": template_id},
            "componentNames": ["UnityEngine.RectTransform", "UnityEngine.UI.Image", "UnityEngine.UI.ScrollRect"]
        }
    }, id=11)
    print(f"  ✅ Template создан ({template_id})")
    
    # 4.4 Viewport (для Template)
    print("  │   ├─ Viewport...")
    viewport_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Viewport",
            "parentGameObjectRef": {"instanceID": template_id}
        }
    }, id=12)
    viewport_id = viewport_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": viewport_id},
            "componentNames": ["UnityEngine.RectTransform", "UnityEngine.UI.Image", "UnityEngine.UI.Mask"]
        }
    }, id=13)
    print(f"  │   ✅ Viewport создан ({viewport_id})")
    
    # 4.5 Content (для Viewport)
    print("  │   ├─ Content...")
    content_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Content",
            "parentGameObjectRef": {"instanceID": viewport_id}
        }
    }, id=14)
    content_id = content_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": content_id},
            "componentNames": ["UnityEngine.RectTransform", "TMPro.TMP_DropdownItem"]
        }
    }, id=15)
    print(f"  │   ✅ Content создан ({content_id})")
    
    # 4.6 Scrollbar
    print("  │   ├─ Scrollbar...")
    scrollbar_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Scrollbar",
            "parentGameObjectRef": {"instanceID": template_id}
        }
    }, id=16)
    scrollbar_id = scrollbar_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": scrollbar_id},
            "componentNames": ["UnityEngine.RectTransform", "UnityEngine.UI.Scrollbar"]
        }
    }, id=17)
    print(f"  │   ✅ Scrollbar создан ({scrollbar_id})")
    
    # 4.7 Sliding Area -> Handle
    print("  │   │   ├─ Sliding Area...")
    sliding_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Sliding Area",
            "parentGameObjectRef": {"instanceID": scrollbar_id}
        }
    }, id=18)
    sliding_id = sliding_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": sliding_id},
            "componentNames": ["UnityEngine.RectTransform"]
        }
    }, id=19)
    
    print("  │   │   ├─ Handle...")
    handle_result = mcp("tools/call", {
        "name": "gameobject-create",
        "arguments": {
            "name": "Handle",
            "parentGameObjectRef": {"instanceID": sliding_id}
        }
    }, id=20)
    handle_id = handle_result['result']['structuredContent']['result']['instanceID']
    
    mcp("tools/call", {
        "name": "gameobject-component-add",
        "arguments": {
            "gameObjectRef": {"instanceID": handle_id},
            "componentNames": ["UnityEngine.RectTransform", "UnityEngine.UI.Image"]
        }
    }, id=21)
    print(f"  │   │   ✅ Handle создан ({handle_id})")
    
    # 5. Настроить ссылки TMP_Dropdown
    print("\n5️⃣ Настройка ссылок TMP_Dropdown...")
    
    # Использовать gameobject-component-modify для настройки ссылок
    modify_result = mcp("tools/call", {
        "name": "gameobject-component-modify",
        "arguments": {
            "gameObjectRef": {"instanceID": dropdown_id},
            "componentTypeName": "TMPro.TMP_Dropdown",
            "properties": {
                "m_Template": {"instanceID": template_id},
                "m_CaptionText": {"instanceID": label_id},
                "m_CaptionImage": None,
                "m_ItemText": {"instanceID": text_id},
                "m_ItemImage": None,
                "m_Arrow": {"instanceID": arrow_id},
                "m_Viewport": {"instanceID": viewport_id},
                "m_Content": {"instanceID": content_id},
                "m_Scrollbar": {"instanceID": scrollbar_id}
            }
        }
    }, id=21)
    
    show("Настройка Dropdown", modify_result)
    
    # 5. Настроить текст кнопки "Применить"
    print("\n5️⃣ Настройка текста кнопки...")
    text_modify = mcp("tools/call", {
        "name": "gameobject-component-modify",
        "arguments": {
            "gameObjectRef": {"instanceID": text_id},
            "componentTypeName": "TMPro.TextMeshProUGUI",
            "properties": {
                "m_text": "Применить",
                "m_fontSize": 24,
                "m_enableAutoSizing": False,
                "m_alignment": 512
            }
        }
    }, id=22)
    
    show("Настройка текста", text_modify)
    
    # 6. Настроить RectTransform позиции
    print("\n6️⃣ Настройка позиций...")
    
    # Dropdown позиция
    dropdown_rect = mcp("tools/call", {
        "name": "gameobject-component-modify",
        "arguments": {
            "gameObjectRef": {"instanceID": dropdown_id},
            "componentTypeName": "UnityEngine.RectTransform",
            "properties": {
                "m_AnchorMin": {"x": 0.5, "y": 0.5},
                "m_AnchorMax": {"x": 0.5, "y": 0.5},
                "m_AnchoredPosition": {"x": 0, "y": 200},
                "m_SizeDelta": {"x": 400, "y": 50},
                "m_Pivot": {"x": 0.5, "y": 0.5}
            }
        }
    }, id=23)
    
    show("Dropdown позиция", dropdown_rect)
    
    # Button позиция
    button_rect = mcp("tools/call", {
        "name": "gameobject-component-modify",
        "arguments": {
            "gameObjectRef": {"instanceID": button_id},
            "componentTypeName": "UnityEngine.RectTransform",
            "properties": {
                "m_AnchorMin": {"x": 0.5, "y": 0.5},
                "m_AnchorMax": {"x": 0.5, "y": 0.5},
                "m_AnchoredPosition": {"x": -120, "y": -200},
                "m_SizeDelta": {"x": 200, "y": 60},
                "m_Pivot": {"x": 0.5, "y": 0.5}
            }
        }
    }, id=24)
    
    show("Button позиция", button_rect)
    
    # ФИНАЛ
    print("\n" + "="*80)
    print("✨ UI НАСТРОЕН!")
    print("="*80)
    print("\n📋 Создана полная иерархия:")
    print("  ResolutionDropdown")
    print("  ├── Label")
    print("  ├── Arrow")
    print("  └── Template")
    print("      ├── Viewport")
    print("      │   └── Content")
    print("      └── Scrollbar")
    print("          └── Sliding Area")
    print("              └── Handle")
    print("\n  ApplyButton")
    print("  └── Text (TMP) - текст: 'Применить'")
    print("\n⚠️ Теперь запустите в Unity:")
    print("   Tools → Setup Resolution Settings UI")
    print("   (Это настроит связи в SettingsMenu скрипте)")

if __name__ == "__main__":
    main()

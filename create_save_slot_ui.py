import requests
import json
import time

BASE_URL = "http://localhost:24997"
HEADERS = {
    "Content-Type": "application/json",
    "Accept": "application/json, text/event-stream"
}

def init_session():
    resp = requests.post(BASE_URL, headers=HEADERS, json={
        "jsonrpc": "2.0",
        "method": "initialize",
        "params": {
            "protocolVersion": "2024-11-05",
            "capabilities": {},
            "clientInfo": {"name": "qwen", "version": "1.0.0"}
        },
        "id": 1
    })
    session_id = resp.headers.get("Mcp-Session-Id")
    print(f"Session ID: {session_id}")
    return session_id

def call_tool(session_id, tool_name, args, req_id):
    headers = {**HEADERS, "Mcp-Session-Id": session_id}
    resp = requests.post(BASE_URL, headers=headers, json={
        "jsonrpc": "2.0",
        "method": "tools/call",
        "params": {
            "name": tool_name,
            "arguments": args
        },
        "id": req_id
    })
    data = resp.text
    # Parse SSE
    for line in data.split('\n'):
        if line.startswith('data: '):
            return json.loads(line[6:])
    return None

def main():
    session_id = init_session()
    if not session_id:
        print("Failed to initialize session")
        return

    # Step 1: Create root SaveSlotUI
    print("\n=== Step 1: Create root SaveSlotUI ===")
    result = call_tool(session_id, "gameobject-create", {"name": "SaveSlotUI"}, 10)
    print(json.dumps(result, indent=2))
    root_id = result.get('result', {}).get('structuredContent', {}).get('result', {}).get('instanceID')
    if not root_id:
        # Try alternative parsing
        text = result.get('result', {}).get('content', [{}])[0].get('text', '')
        data = json.loads(text)
        root_id = data.get('result', {}).get('instanceID')
    print(f"Root ID: {root_id}")

    # Step 2: Add components to root
    print("\n=== Step 2: Add components ===")
    result = call_tool(session_id, "gameobject-component-add", {
        "gameObjectRef": {"instanceID": root_id},
        "componentNames": ["UnityEngine.UI.Image", "UnityEngine.UI.Button", "SaveSlotUI"]
    }, 11)
    print(json.dumps(result, indent=2))

    # Step 3: Set root RectTransform size
    print("\n=== Step 3: Set root size ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": root_id},
        "componentRef": {"typeName": "UnityEngine.RectTransform"},
        "componentDiff": {
            "typeName": "UnityEngine.RectTransform",
            "props": [
                {"typeName": "UnityEngine.Vector2", "name": "sizeDelta", "value": {"x": 400, "y": 120}}
            ]
        }
    }, 12)
    print(json.dumps(result, indent=2))

    # Step 4: Create SlotTitle
    print("\n=== Step 4: Create SlotTitle ===")
    result = call_tool(session_id, "gameobject-create", {
        "name": "SlotTitle",
        "parentGameObjectRef": {"instanceID": root_id}
    }, 13)
    text = result.get('result', {}).get('content', [{}])[0].get('text', '')
    data = json.loads(text)
    slot_title_id = data.get('result', {}).get('instanceID')
    print(f"SlotTitle ID: {slot_title_id}")

    # Step 5: Add TextMeshProUGUI to SlotTitle
    print("\n=== Step 5: Add TextMeshProUGUI to SlotTitle ===")
    result = call_tool(session_id, "gameobject-component-add", {
        "gameObjectRef": {"instanceID": slot_title_id},
        "componentNames": ["TMPro.TextMeshProUGUI"]
    }, 14)
    print(json.dumps(result, indent=2))

    # Step 6: Configure SlotTitle text
    print("\n=== Step 6: Configure SlotTitle text ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": slot_title_id},
        "componentRef": {"typeName": "TMPro.TextMeshProUGUI"},
        "componentDiff": {
            "typeName": "TMPro.TextMeshProUGUI",
            "fields": [
                {"typeName": "UnityEngine.UI.Button", "name": "m_text", "value": {"typeName": "System.String", "value": "Слот 1"}},
                {"typeName": "System.Single", "name": "m_fontSize", "value": 28},
                {"typeName": "UnityEngine.Color", "name": "m_fontColor", "value": {"r": 1, "g": 1, "b": 1, "a": 1}},
                {"typeName": "TMPro.FontStyles", "name": "m_fontStyle", "value": 1}
            ]
        }
    }, 15)
    print(json.dumps(result, indent=2))

    # Step 7: Configure SlotTitle RectTransform
    print("\n=== Step 7: Configure SlotTitle RectTransform ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": slot_title_id},
        "componentRef": {"typeName": "UnityEngine.RectTransform"},
        "componentDiff": {
            "typeName": "UnityEngine.RectTransform",
            "props": [
                {"typeName": "UnityEngine.Vector2", "name": "anchorMin", "value": {"x": 0.5, "y": 1}},
                {"typeName": "UnityEngine.Vector2", "name": "anchorMax", "value": {"x": 0.5, "y": 1}},
                {"typeName": "UnityEngine.Vector2", "name": "pivot", "value": {"x": 0.5, "y": 1}},
                {"typeName": "UnityEngine.Vector2", "name": "anchoredPosition", "value": {"x": 0, "y": -10}},
                {"typeName": "UnityEngine.Vector2", "name": "sizeDelta", "value": {"x": 380, "y": 40}}
            ]
        }
    }, 16)
    print(json.dumps(result, indent=2))

    # Step 8: Create SlotInfo
    print("\n=== Step 8: Create SlotInfo ===")
    result = call_tool(session_id, "gameobject-create", {
        "name": "SlotInfo",
        "parentGameObjectRef": {"instanceID": root_id}
    }, 17)
    text = result.get('result', {}).get('content', [{}])[0].get('text', '')
    data = json.loads(text)
    slot_info_id = data.get('result', {}).get('instanceID')
    print(f"SlotInfo ID: {slot_info_id}")

    # Step 9: Add TextMeshProUGUI to SlotInfo
    print("\n=== Step 9: Add TextMeshProUGUI to SlotInfo ===")
    result = call_tool(session_id, "gameobject-component-add", {
        "gameObjectRef": {"instanceID": slot_info_id},
        "componentNames": ["TMPro.TextMeshProUGUI"]
    }, 18)

    # Step 10: Configure SlotInfo text
    print("\n=== Step 10: Configure SlotInfo text ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": slot_info_id},
        "componentRef": {"typeName": "TMPro.TextMeshProUGUI"},
        "componentDiff": {
            "typeName": "TMPro.TextMeshProUGUI",
            "fields": [
                {"typeName": "UnityEngine.UI.Button", "name": "m_text", "value": {"typeName": "System.String", "value": "День 15 | 14:30 | 01.04.2026"}},
                {"typeName": "System.Single", "name": "m_fontSize", "value": 20},
                {"typeName": "UnityEngine.Color", "name": "m_fontColor", "value": {"r": 0.5, "g": 0.5, "b": 0.5, "a": 1}}
            ]
        }
    }, 19)
    print(json.dumps(result, indent=2))

    # Step 11: Configure SlotInfo RectTransform
    print("\n=== Step 11: Configure SlotInfo RectTransform ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": slot_info_id},
        "componentRef": {"typeName": "UnityEngine.RectTransform"},
        "componentDiff": {
            "typeName": "UnityEngine.RectTransform",
            "props": [
                {"typeName": "UnityEngine.Vector2", "name": "anchorMin", "value": {"x": 0.5, "y": 0}},
                {"typeName": "UnityEngine.Vector2", "name": "anchorMax", "value": {"x": 0.5, "y": 0}},
                {"typeName": "UnityEngine.Vector2", "name": "pivot", "value": {"x": 0.5, "y": 0}},
                {"typeName": "UnityEngine.Vector2", "name": "anchoredPosition", "value": {"x": 0, "y": 10}},
                {"typeName": "UnityEngine.Vector2", "name": "sizeDelta", "value": {"x": 380, "y": 30}}
            ]
        }
    }, 20)
    print(json.dumps(result, indent=2))

    # Step 12: Create EmptySlotText
    print("\n=== Step 12: Create EmptySlotText ===")
    result = call_tool(session_id, "gameobject-create", {
        "name": "EmptySlotText",
        "parentGameObjectRef": {"instanceID": root_id}
    }, 21)
    text = result.get('result', {}).get('content', [{}])[0].get('text', '')
    data = json.loads(text)
    empty_text_id = data.get('result', {}).get('instanceID')
    print(f"EmptySlotText ID: {empty_text_id}")

    # Step 13: Add TextMeshProUGUI to EmptySlotText
    print("\n=== Step 13: Add TextMeshProUGUI to EmptySlotText ===")
    result = call_tool(session_id, "gameobject-component-add", {
        "gameObjectRef": {"instanceID": empty_text_id},
        "componentNames": ["TMPro.TextMeshProUGUI"]
    }, 22)

    # Step 14: Configure EmptySlotText
    print("\n=== Step 14: Configure EmptySlotText ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": empty_text_id},
        "componentRef": {"typeName": "TMPro.TextMeshProUGUI"},
        "componentDiff": {
            "typeName": "TMPro.TextMeshProUGUI",
            "fields": [
                {"typeName": "UnityEngine.UI.Button", "name": "m_text", "value": {"typeName": "System.String", "value": "Пусто"}},
                {"typeName": "System.Single", "name": "m_fontSize", "value": 24},
                {"typeName": "UnityEngine.Color", "name": "m_fontColor", "value": {"r": 0.5, "g": 0.5, "b": 0.5, "a": 1}},
                {"typeName": "TMPro.FontStyles", "name": "m_fontStyle", "value": 2}
            ]
        }
    }, 23)
    print(json.dumps(result, indent=2))

    # Step 15: Configure EmptySlotText RectTransform
    print("\n=== Step 15: Configure EmptySlotText RectTransform ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": empty_text_id},
        "componentRef": {"typeName": "UnityEngine.RectTransform"},
        "componentDiff": {
            "typeName": "UnityEngine.RectTransform",
            "props": [
                {"typeName": "UnityEngine.Vector2", "name": "anchorMin", "value": {"x": 0, "y": 0}},
                {"typeName": "UnityEngine.Vector2", "name": "anchorMax", "value": {"x": 1, "y": 1}},
                {"typeName": "UnityEngine.Vector2", "name": "pivot", "value": {"x": 0.5, "y": 0.5}},
                {"typeName": "UnityEngine.Vector2", "name": "anchoredPosition", "value": {"x": 0, "y": 0}},
                {"typeName": "UnityEngine.Vector2", "name": "sizeDelta", "value": {"x": 0, "y": 0}}
            ]
        }
    }, 24)
    print(json.dumps(result, indent=2))

    # Step 16: Configure root Image color
    print("\n=== Step 16: Configure root Image color ===")
    result = call_tool(session_id, "gameobject-component-modify", {
        "gameObjectRef": {"instanceID": root_id},
        "componentRef": {"typeName": "UnityEngine.UI.Image"},
        "componentDiff": {
            "typeName": "UnityEngine.UI.Image",
            "props": [
                {"typeName": "UnityEngine.Color", "name": "color", "value": {"r": 0.2, "g": 0.2, "b": 0.2, "a": 1}}
            ]
        }
    }, 25)
    print(json.dumps(result, indent=2))

    # Step 17: Save as prefab
    print("\n=== Step 17: Save as prefab ===")
    result = call_tool(session_id, "assets-prefab-create", {
        "gameObjectRef": {"instanceID": root_id},
        "prefabAssetPath": "Assets/PreFab/UI/SaveSlotUI.prefab"
    }, 26)
    print(json.dumps(result, indent=2))

    print("\n=== DONE ===")

if __name__ == "__main__":
    main()

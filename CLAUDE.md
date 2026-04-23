# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

2D survival/farming game on Unity. Single scene `MainMap` — the main game scene. Menu scene exists separately. All scripts are in `Assets/Script/`, prefabs in `Assets/PreFab/`.

## Architecture

### Singleton Pattern
Almost every manager uses `public static T Instance { get; private set; }` set in `Awake()`. Key singletons: `Player`, `PlayerHealth`, `InventoryManager`, `ItemRegistry`, `GlobalTime`, `WeatherManager`, `SlimeSpawner`, `SaveManager`, `DeathFade`.

### Event Subscription Rule
**Always subscribe to events in `Start()`, never in `Awake()` or `OnEnable()`.**  
Reason: `Awake()` on different objects in the same scene load frame runs in arbitrary order — `PlayerHealth.Instance` may be null when another component's `Awake()` runs. `Start()` is guaranteed to run after all `Awake()` calls.

### Item System
- `Item` is a ScriptableObject. The asset's **filename** is the canonical item name.
- `ItemRegistry` builds a `Dictionary<string, Item>` keyed by `item.name` (case-sensitive, exact filename match).
- `PickupItem.itemName` on prefabs **must match the ScriptableObject filename exactly** (e.g. `strawberry`, `Kabachok`, `Docka`).
- Each pickup prefab must have only **one** pickup script — `PickupItem`. The legacy `Looting` and `Loot` components cause double-pickup if left on the same prefab.

### Save System
There are **two separate code paths** for applying save data:
- `SaveManager.ApplyGameData()` — used from the main menu "Load Game" flow
- `GameSaveLoader.LoadSave()` — runs as a coroutine on `MainMap` scene start (actual path used in-game)

When adding new saveable data, update **both** files. New/load game detection: `PlayerPrefs.GetInt("new_game", 1) == 1`.

World objects that need saving implement `ISaveable` (interface in `GameData.cs`): `GetObjectId()`, `GetSaveData()`, `LoadData(WorldObjectData)`.

### Health & Death Flow
`PlayerHealth.TakeDamage()` → sets `_isDead = true` → fires `OnDied` event → `DeathFade` catches it, plays fade coroutine, calls `PlayerHealth.Respawn()` → `_isDead = false`. The `_isDead` flag prevents re-entry while the coroutine runs.

### Inventory
`InventoryManager` has four slot arrays: `inventorySlots` (hotbar, 5 slots), `chestSlots`, `craftSlots` (3×3), `endCraftSlots` (1 slot). Selected hotbar slot: keys 1–5. `GetSelectedItem(bool use)` — pass `use=true` to consume one count.

### Time & Day Cycle
`GlobalTime` — 1 real second = 1 in-game minute. Periods: Morning 6–11, Day 12–17, Evening 18–21, Night 22–5. Night activates `SlimeSpawner` and spot lights.

### Weather
Changes once per in-game day at 6:00. Affects `WeatherManager.GrowthMultiplier` (Rain ×2, Stormy ×1.5, Cloudy ×0.8, Sunny ×1). Use `[ContextMenu]` methods on `WeatherManager` to test weather in the Editor.

## MCP Server (Unity Editor Integration)

Use `unity-mcp-cli` tools to interact with the live Unity Editor:
- `scene-open` — requires `sceneRef` with `assetPath` (e.g. `"Assets/Scenes/MainMap.unity"`)
- `gameobject-find` — find by `name`, `path` in hierarchy, or `instanceID`
- `gameobject-component-add` / `gameobject-component-modify` / `gameobject-component-get`
- `script-update-or-create` — write/update `.cs` files; Unity auto-compiles
- `scene-save` — always call after editing the scene

When modifying component fields via MCP, use property names (camelCase), not serialized YAML field names.

## Key Pitfalls

- `PickupItem.itemName` ≠ ScriptableObject name → item silently not found in registry
- Duplicate pickup scripts on one prefab → items added ×2
- Subscribing to `PlayerHealth.OnDied` in `Awake/OnEnable` → NullReferenceException or missed subscription
- Both `GameSaveLoader` and `SaveManager` must be updated when adding new saved fields
- `ItemRegistry` uses `DontDestroyOnLoad` — it persists across scene loads, so it's safe to reference from any scene

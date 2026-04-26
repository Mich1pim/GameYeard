using UnityEngine;
using System.IO;
using System.Collections;
using System.Linq;

/// <summary>
/// Загружает сохранение при старте игровой сцены.
/// Вешается на GameObject в сцене MainMap.
/// </summary>
public class GameSaveLoader : MonoBehaviour
{
    [Tooltip("Загружать сохранение автоматически при старте?")]
    public bool loadOnStart = true;

    [Header("Префабы лута на земле (перетащи все)")]
    public GameObject[] pickupPrefabs;  // Все префабы лута (Money, Apple, Strawberry и т.д.)

    private bool _hasLoaded = false;
    private string _savePath;

    void Start()
    {
        int slot = PlayerPrefs.GetInt("load_slot", 1);
        _savePath = Path.Combine(Application.persistentDataPath, $"game_save_slot_{slot}.json");

        if (loadOnStart)
        {
            StartCoroutine(LoadSaveRoutine());
        }
    }

    private IEnumerator LoadSaveRoutine()
    {
        // Проверяем — это новая игра или загрузка
        bool isNewGame = PlayerPrefs.GetInt("new_game", 1) == 1;

        if (isNewGame)
        {
            Debug.Log("[GameSaveLoader] Новая игра — загрузка не требуется");
            yield break;
        }
        Debug.Log("[GameSaveLoader] Ищу Player в сцене...");

        // Ищем Player объект напрямую, а не через Instance
        int frames = 0;
        Player player = null;
        InventoryManager invManager = null;
        GlobalTime globalTime = null;

        while (frames < 300)
        {
            if (player == null)
            {
                // Ищем через FindObjectOfType или берём Instance
                player = FindObjectOfType<Player>();
                if (player == null) player = Player.Instance;
            }
            if (invManager == null)
            {
                invManager = FindObjectOfType<InventoryManager>();
                if (invManager == null) invManager = InventoryManager.Instance;
            }
            if (globalTime == null)
            {
                globalTime = FindObjectOfType<GlobalTime>();
                if (globalTime == null) globalTime = GlobalTime.Instance;
            }

            if (player != null && invManager != null && globalTime != null)
                break;

            yield return null;
            frames++;
        }

        if (player == null)
        {
            Debug.LogError("[GameSaveLoader] Player не найден в сцене!");
            yield break;
        }
        if (invManager == null)
        {
            Debug.LogError("[GameSaveLoader] InventoryManager не найден в сцене!");
            yield break;
        }
        if (globalTime == null)
        {
            Debug.LogError("[GameSaveLoader] GlobalTime не найден в сцене!");
            yield break;
        }

        Debug.Log($"[GameSaveLoader] Все найдены! Загружаю сохранение...");
        LoadSave(player, invManager, globalTime);
    }

    private void LoadSave(Player player, InventoryManager invManager, GlobalTime globalTime)
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("[GameSaveLoader] Нет файла сохранения");
            return;
        }

        string json = File.ReadAllText(_savePath);
        Debug.Log($"[GameSaveLoader] JSON загружен");

        GameData data = JsonUtility.FromJson<GameData>(json);

        if (data == null || data.playerData == null)
        {
            Debug.LogError("[GameSaveLoader] Не удалось распарсить JSON");
            return;
        }

        Debug.Log($"[GameSaveLoader] Данные: coins={data.playerData.coins}, pos=({data.playerData.positionX},{data.playerData.positionY})");

        // Применяем позицию и монеты
        player.transform.position = new Vector2(data.playerData.positionX, data.playerData.positionY);
        player.coin = data.playerData.coins;
        Debug.Log($"[GameSaveLoader] Позиция игрока: {player.transform.position}");

        // Применяем здоровье
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null && data.playerData.health > 0)
        {
            playerHealth.SetHealth(data.playerData.health);
            Debug.Log($"[GameSaveLoader] Здоровье: {data.playerData.health}");
        }

        // Применяем время
        if (data.timeData != null)
        {
            globalTime.minutes = data.timeData.minutes;
            globalTime.hours = data.timeData.hours;
            globalTime.days = data.timeData.days;
            Debug.Log($"[GameSaveLoader] Время: день {data.timeData.days} {data.timeData.hours}:{data.timeData.minutes}");
        }

        // Применяем инвентарь
        if (data.inventoryData != null)
        {
            LoadInventory(data, invManager);
        }

        // Применяем погоду
        if (data.weatherData != null)
        {
            WeatherManager weather = FindObjectOfType<WeatherManager>();
            if (weather != null)
                weather.LoadData(data.weatherData);
        }

        // Применяем мир (ресурсы, объекты)
        if (data.worldData != null && data.worldData.objects != null)
        {
            LoadWorld(data, invManager);
        }

        // Применяем слаймов (только если сейчас ночь)
        if (data.slimeData != null && data.slimeData.Length > 0)
        {
            SlimeSpawner spawner = FindObjectOfType<SlimeSpawner>();
            if (spawner != null)
                spawner.LoadSlimes(data.slimeData);
        }

        Debug.Log("[GameSaveLoader] === СОХРАНЕНИЕ ЗАГРУЖЕНО ===");
        _hasLoaded = true;
    }

    private void LoadWorld(GameData data, InventoryManager invManager)
    {
        ISaveable[] saveableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();
        Debug.Log($"[GameSaveLoader] Found {saveableObjects.Length} ISaveable objects in scene");
        foreach (var s in saveableObjects)
        {
            Debug.Log($"  - {s.GetObjectId()} ({s.GetType().Name})");
        }

        int loaded = 0;
        int spawned = 0;
        int skipped = 0;

        Debug.Log($"[GameSaveLoader] Loading {data.worldData.objects.Length} world objects from save");

        foreach (var worldObjData in data.worldData.objects)
        {
            Debug.Log($"[GameSaveLoader] Processing: {worldObjData.objectId} type={worldObjData.objectType} isDead={worldObjData.isDead}");

            // Для PickupItem — если НЕ подобран (isDead=false) ищем в сцене, если подобран — пропускаем
            if (worldObjData.objectType == "PickupItem")
            {
                if (worldObjData.isDead)
                {
                    // Был подобран — не спавним
                    Debug.Log($"  -> Picked up, skipping");
                    skipped++;
                    continue;
                }

                // Ищем существующий в сцене
                bool found = false;
                foreach (var saveable in saveableObjects)
                {
                    if (saveable.GetObjectId() == worldObjData.objectId)
                    {
                        saveable.LoadData(worldObjData);
                        loaded++;
                        found = true;
                        Debug.Log($"  -> Found existing, loaded");
                        break;
                    }
                }

                if (!found)
                {
                    // Не нашли — спавним новый
                    Debug.Log($"  -> Not found, spawning new");
                    SpawnPickupItem(worldObjData);
                    spawned++;
                }
                continue;
            }

            // Для остальных объектов — обновляем состояние
            foreach (var saveable in saveableObjects)
            {
                if (saveable.GetObjectId() == worldObjData.objectId)
                {
                    saveable.LoadData(worldObjData);
                    loaded++;
                    break;
                }
            }
        }

        Debug.Log($"[GameSaveLoader] Мир загружен: {loaded} обновлено, {spawned} создано, {skipped} пропущено");
    }

    private void SpawnPickupItem(WorldObjectData data)
    {
        Debug.Log($"[GameSaveLoader] Spawning pickup: {data.itemName} at ({data.posX}, {data.posY}), count={(int)data.health}");

        GameObject prefab = FindPickupPrefab(data.itemName);
        if (prefab == null)
        {
            Debug.LogWarning($"[GameSaveLoader] Префаб '{data.itemName}' не найден! Добавь его в массив Pickup Prefabs на GameSaveLoader.");
            if (pickupPrefabs != null)
                foreach (var p in pickupPrefabs)
                    if (p != null) Debug.Log($"  Доступен: {p.name}");
            return;
        }

        Vector3 pos = new Vector3(data.posX, data.posY, 0);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        PickupItem pickup = go.GetComponent<PickupItem>();
        if (pickup != null)
        {
            pickup.objectID = data.objectId;
            pickup.count = (int)data.health;
            Debug.Log($"[GameSaveLoader] Спавн OK: {data.itemName} x{(int)data.health}");
        }
        else
        {
            Debug.LogError($"[GameSaveLoader] На префабе '{data.itemName}' нет компонента PickupItem!");
        }
    }

    private GameObject FindPickupPrefab(string itemName)
    {
        // Убираем "(Clone)" из имени
        int cloneIdx = itemName.IndexOf(" (");
        if (cloneIdx > 0) itemName = itemName.Substring(0, cloneIdx);

        if (pickupPrefabs == null) return null;

        foreach (var prefab in pickupPrefabs)
        {
            if (prefab != null && prefab.name == itemName)
                return prefab;
        }

        // Пробуем без суффиксов и у префаба
        foreach (var prefab in pickupPrefabs)
        {
            if (prefab != null)
            {
                string cleanName = prefab.name;
                int idx = cleanName.IndexOf(" (");
                if (idx > 0) cleanName = cleanName.Substring(0, idx);
                if (cleanName == itemName)
                    return prefab;
            }
        }

        return null;
    }

    private void LoadInventory(GameData data, InventoryManager invManager)
    {
        if (data.inventoryData.slots == null) return;

        // Очищаем текущий инвентарь
        foreach (var slot in invManager.inventorySlots)
        {
            var item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null) Destroy(item.gameObject);
        }
        foreach (var slot in invManager.chestSlots)
        {
            var item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null) Destroy(item.gameObject);
        }
        // Очищаем слоты крафта
        foreach (var slot in invManager.craftSlots)
        {
            var item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null) Destroy(item.gameObject);
        }
        foreach (var slot in invManager.endCraftSlots)
        {
            var item = slot.GetComponentInChildren<InventoryItem>();
            if (item != null) Destroy(item.gameObject);
        }

        // Загружаем предметы
        foreach (var slotData in data.inventoryData.slots)
        {
            Item item = ItemRegistry.FindItem(slotData.itemName);
            if (item != null)
            {
                InventorySlot slot;

                if (slotData.isCraft)
                {
                    slot = invManager.craftSlots[slotData.slotIndex];
                }
                else if (slotData.isEndCraft)
                {
                    slot = invManager.endCraftSlots[slotData.slotIndex];
                }
                else if (slotData.isChest)
                {
                    slot = invManager.chestSlots[slotData.slotIndex];
                }
                else
                {
                    slot = invManager.inventorySlots[slotData.slotIndex];
                }

                if (invManager.inventoryItemPrefab != null)
                {
                    GameObject go = Instantiate(invManager.inventoryItemPrefab, slot.transform);
                    InventoryItem invItem = go.GetComponent<InventoryItem>();
                    invItem.InitialiseItem(item);
                    invItem.count = slotData.count;
                    invItem.RefreshCount();
                }
            }
        }

        Debug.Log($"[GameSaveLoader] Инвентарь загружен: {data.inventoryData.slots.Length} предметов");
    }
}

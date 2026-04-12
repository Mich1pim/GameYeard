using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Менеджер сохранений — одно сохранение для всей игры.
/// Сохраняет: позицию игрока, монеты, инвентарь, время.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE_NAME = "game_save.json";
    private const string HAS_SAVE_KEY = "has_game_save";

    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    void Awake()
    {
        Debug.Log($"[SaveManager] Awake called! Instance is {(Instance == null ? "null" : "set")}");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[SaveManager] Instance created: {this.gameObject.name}");
        }
        else
        {
            Debug.Log($"[SaveManager] Duplicate found, destroying: {this.gameObject.name}");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Сохранить игру
    /// </summary>
    public void SaveGame()
    {
        Debug.Log("[SaveManager] SaveGame called!");

        try
        {
            GameData gameData = CollectGameData();
            gameData.saveTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            string json = JsonUtility.ToJson(gameData, true);
            Debug.Log("[SaveManager] JSON: " + json);

            File.WriteAllText(SavePath, json);
            Debug.Log("[SaveManager] File written to: " + SavePath);

            PlayerPrefs.SetInt(HAS_SAVE_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log($"[SaveManager] Game saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Error saving game: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Загрузить игру
    /// </summary>
    public GameData LoadGame()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("[SaveManager] No save file found");
                return null;
            }

            string json = File.ReadAllText(SavePath);
            Debug.Log("[SaveManager] Loaded JSON: " + json);

            GameData gameData = JsonUtility.FromJson<GameData>(json);

            Debug.Log("[SaveManager] Game loaded");
            return gameData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Error loading game: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Удалить сохранение
    /// </summary>
    public void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                PlayerPrefs.SetInt(HAS_SAVE_KEY, 0);
                PlayerPrefs.Save();
                Debug.Log("[SaveManager] Save deleted");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Error deleting save: {e.Message}");
        }
    }

    /// <summary>
    /// Есть ли сохранение
    /// </summary>
    public bool HasSave()
    {
        return File.Exists(SavePath);
    }

    /// <summary>
    /// Получить метаданные сохранения
    /// </summary>
    public SaveMetadata GetSaveMetadata()
    {
        SaveMetadata metadata = new SaveMetadata
        {
            hasSave = false,
            saveTime = "",
            day = 0
        };

        if (!File.Exists(SavePath))
            return metadata;

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData gameData = JsonUtility.FromJson<GameData>(json);

            metadata.hasSave = true;
            metadata.saveTime = gameData.saveTime;
            metadata.day = gameData.timeData != null ? gameData.timeData.days : 0;
        }
        catch
        {
        }

        return metadata;
    }

    /// <summary>
    /// Применить загруженные данные ко всем системам
    /// </summary>
    public void ApplyGameData(GameData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveManager] Cannot apply null game data");
            return;
        }

        Debug.Log("[SaveManager] Applying game data...");

        // Позиция и монеты игрока
        if (Player.Instance != null && data.playerData != null)
        {
            Player.Instance.transform.position = new Vector2(data.playerData.positionX, data.playerData.positionY);
            Player.Instance.coin = data.playerData.coins;
            Debug.Log($"[SaveManager] Player: pos=({data.playerData.positionX},{data.playerData.positionY}) coins={data.playerData.coins}");
        }

        // Время
        if (GlobalTime.Instance != null && data.timeData != null)
        {
            GlobalTime.Instance.minutes = data.timeData.minutes;
            GlobalTime.Instance.hours = data.timeData.hours;
            GlobalTime.Instance.days = data.timeData.days;
            Debug.Log($"[SaveManager] Time: day={data.timeData.days} {data.timeData.hours}:{data.timeData.minutes}");
        }

        // Инвентарь
        if (InventoryManager.Instance != null && data.inventoryData != null)
        {
            ClearInventory();

            if (data.inventoryData.slots != null)
            {
                foreach (var slotData in data.inventoryData.slots)
                {
                    Item item = ItemRegistry.FindItem(slotData.itemName);
                    if (item != null)
                    {
                        SpawnItemInSlotByType(item, slotData.count, slotData);
                    }
                }
            }

            Debug.Log($"[SaveManager] Inventory: loaded {data.inventoryData.slots?.Length ?? 0} items");
        }

        // Мир (сундуки, ресурсы, двери)
        if (data.worldData != null && data.worldData.objects != null)
        {
            ISaveable[] saveableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();
            foreach (var worldObjData in data.worldData.objects)
            {
                // Для PickupItem — воссоздаём
                if (worldObjData.objectType == "PickupItem" && !worldObjData.isDead)
                {
                    SpawnPickupItem(worldObjData);
                    continue;
                }

                // Для остальных — обновляем
                foreach (var saveable in saveableObjects)
                {
                    if (saveable.GetObjectId() == worldObjData.objectId)
                    {
                        saveable.LoadData(worldObjData);
                        break;
                    }
                }
            }
            Debug.Log($"[SaveManager] World: loaded {data.worldData.objects.Length} objects");
        }

        Debug.Log("[SaveManager] Game data applied successfully");
    }

    /// <summary>
    /// Собрать данные из всех систем
    /// </summary>
    private GameData CollectGameData()
    {
        // Собираем слоты инвентаря
        List<InventorySlotData> invSlots = new List<InventorySlotData>();

        // Игрок
        PlayerData pData = new PlayerData();
        if (Player.Instance != null)
        {
            pData.positionX = Player.Instance.transform.position.x;
            pData.positionY = Player.Instance.transform.position.y;
            pData.coins = Player.Instance.coin;
        }

        // Время
        TimeData tData = new TimeData();
        if (GlobalTime.Instance != null)
        {
            tData.minutes = GlobalTime.Instance.minutes;
            tData.hours = GlobalTime.Instance.hours;
            tData.days = GlobalTime.Instance.days;
        }

        // Инвентарь
        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < InventoryManager.Instance.inventorySlots.Length; i++)
            {
                InventoryItem itemInSlot = InventoryManager.Instance.inventorySlots[i].GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null)
                {
                    invSlots.Add(new InventorySlotData
                    {
                        slotIndex = i,
                        itemName = itemInSlot.item.name,
                        count = itemInSlot.count,
                        isChest = false
                    });
                }
            }

            for (int i = 0; i < InventoryManager.Instance.chestSlots.Length; i++)
            {
                InventoryItem itemInSlot = InventoryManager.Instance.chestSlots[i].GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null)
                {
                    invSlots.Add(new InventorySlotData
                    {
                        slotIndex = i,
                        itemName = itemInSlot.item.name,
                        count = itemInSlot.count,
                        isChest = true
                    });
                }
            }

            // Слоты крафта (9 слотов 3x3)
            for (int i = 0; i < InventoryManager.Instance.craftSlots.Length; i++)
            {
                InventoryItem itemInSlot = InventoryManager.Instance.craftSlots[i].GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null)
                {
                    invSlots.Add(new InventorySlotData
                    {
                        slotIndex = i,
                        itemName = itemInSlot.item.name,
                        count = itemInSlot.count,
                        isChest = false,
                        isCraft = true
                    });
                }
            }

            // Слот результата крафта (1 слот)
            for (int i = 0; i < InventoryManager.Instance.endCraftSlots.Length; i++)
            {
                InventoryItem itemInSlot = InventoryManager.Instance.endCraftSlots[i].GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null)
                {
                    invSlots.Add(new InventorySlotData
                    {
                        slotIndex = i,
                        itemName = itemInSlot.item.name,
                        count = itemInSlot.count,
                        isChest = false,
                        isEndCraft = true
                    });
                }
            }
        }

        // Мир (собираем от всех ISaveable объектов)
        List<WorldObjectData> worldObjs = new List<WorldObjectData>();
        ISaveable[] saveableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();
        foreach (var saveable in saveableObjects)
        {
            worldObjs.Add(saveable.GetSaveData());
            Debug.Log($"[SaveManager] Saving world object: {saveable.GetObjectId()} ({saveable.GetType().Name})");
        }

        GameData data = new GameData
        {
            playerData = pData,
            inventoryData = new InventoryData { slots = invSlots.ToArray() },
            timeData = tData,
            worldData = new WorldData { objects = worldObjs.ToArray() }
        };

        return data;
    }

    private void ClearInventory()
    {
        if (InventoryManager.Instance == null) return;

        foreach (var slot in InventoryManager.Instance.inventorySlots)
        {
            var itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) Destroy(itemInSlot.gameObject);
        }

        foreach (var slot in InventoryManager.Instance.chestSlots)
        {
            var itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) Destroy(itemInSlot.gameObject);
        }

        // Очищаем слоты крафта
        foreach (var slot in InventoryManager.Instance.craftSlots)
        {
            var itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) Destroy(itemInSlot.gameObject);
        }
        foreach (var slot in InventoryManager.Instance.endCraftSlots)
        {
            var itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null) Destroy(itemInSlot.gameObject);
        }
    }

    private void SpawnItemInSlot(Item item, int count, InventorySlot slot)
    {
        if (InventoryManager.Instance.inventoryItemPrefab == null)
        {
            Debug.LogError("[SaveManager] inventoryItemPrefab is null!");
            return;
        }

        GameObject go = Instantiate(InventoryManager.Instance.inventoryItemPrefab, slot.transform);
        InventoryItem invItem = go.GetComponent<InventoryItem>();
        invItem.InitialiseItem(item);
        invItem.count = count;
        invItem.RefreshCount();
    }

    /// <summary>
    /// Загрузить предметы в нужный слот (учитывает тип слота)
    /// </summary>
    private void SpawnItemInSlotByType(Item item, int count, InventorySlotData slotData)
    {
        if (InventoryManager.Instance.inventoryItemPrefab == null)
        {
            Debug.LogError("[SaveManager] inventoryItemPrefab is null!");
            return;
        }

        InventorySlot slot;

        if (slotData.isCraft)
        {
            slot = InventoryManager.Instance.craftSlots[slotData.slotIndex];
        }
        else if (slotData.isEndCraft)
        {
            slot = InventoryManager.Instance.endCraftSlots[slotData.slotIndex];
        }
        else if (slotData.isChest)
        {
            slot = InventoryManager.Instance.chestSlots[slotData.slotIndex];
        }
        else
        {
            slot = InventoryManager.Instance.inventorySlots[slotData.slotIndex];
        }

        GameObject go = Instantiate(InventoryManager.Instance.inventoryItemPrefab, slot.transform);
        InventoryItem invItem = go.GetComponent<InventoryItem>();
        invItem.InitialiseItem(item);
        invItem.count = count;
        invItem.RefreshCount();
    }

    [Header("Префабы лута на земле")]
    public GameObject[] pickupPrefabs;

    private void SpawnPickupItem(WorldObjectData data)
    {
        // Убираем "(Clone)" из имени
        string itemName = data.itemName;
        int cloneIdx = itemName.IndexOf(" (");
        if (cloneIdx > 0) itemName = itemName.Substring(0, cloneIdx);

        GameObject prefab = null;
        if (pickupPrefabs != null)
        {
            foreach (var p in pickupPrefabs)
            {
                if (p != null && p.name == itemName)
                {
                    prefab = p;
                    break;
                }
            }

            // Пробуем без суффиксов у префаба
            if (prefab == null)
            {
                foreach (var p in pickupPrefabs)
                {
                    if (p != null)
                    {
                        string cleanName = p.name;
                        int idx = cleanName.IndexOf(" (");
                        if (idx > 0) cleanName = cleanName.Substring(0, idx);
                        if (cleanName == itemName)
                        {
                            prefab = p;
                            break;
                        }
                    }
                }
            }
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[SaveManager] Pickup prefab '{itemName}' not found!");
            return;
        }

        Vector3 pos = new Vector3(data.posX, data.posY, 0);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        PickupItem pickup = go.GetComponent<PickupItem>();
        if (pickup != null)
        {
            pickup.objectID = data.objectId;
            pickup.count = (int)data.health;
        }
    }
}

[Serializable]
public class SaveMetadata
{
    public bool hasSave;
    public string saveTime;
    public int day;
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string HAS_SAVE_KEY = "has_game_save";
    public const int SlotCount = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string GetSavePath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"game_save_slot_{slot}.json");

    // ─── Save ───────────────────────────────────────────────────

    public void SaveGame(int slot)
    {
        try
        {
            GameData gameData = CollectGameData();
            gameData.saveTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(GetSavePath(slot), json);

            PlayerPrefs.SetInt(HAS_SAVE_KEY, 1);
            PlayerPrefs.Save();

            Debug.Log($"[SaveManager] Saved to slot {slot}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Error saving slot {slot}: {e.Message}");
        }
    }

    // ─── Load ───────────────────────────────────────────────────

    public GameData LoadGame(int slot)
    {
        try
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Error loading slot {slot}: {e.Message}");
            return null;
        }
    }

    // ─── Existence ──────────────────────────────────────────────

    public bool HasSave(int slot) => File.Exists(GetSavePath(slot));

    public bool HasSave()
    {
        for (int i = 1; i <= SlotCount; i++)
            if (HasSave(i)) return true;
        return false;
    }

    public SaveMetadata GetSaveMetadata(int slot)
    {
        var meta = new SaveMetadata { hasSave = false, saveTime = "", day = 0 };
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return meta;

        try
        {
            GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(path));
            meta.hasSave = true;
            meta.saveTime = data.saveTime;
            meta.day = data.timeData != null ? data.timeData.days : 0;
        }
        catch { }

        return meta;
    }

    public void DeleteSave(int slot)
    {
        try
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveManager] Deleted slot {slot}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Error deleting slot {slot}: {e.Message}");
        }
    }

    // ─── Apply ──────────────────────────────────────────────────

    public void ApplyGameData(GameData data)
    {
        if (data == null) { Debug.LogError("[SaveManager] Cannot apply null game data"); return; }

        if (Player.Instance != null && data.playerData != null)
        {
            Player.Instance.transform.position = new Vector2(data.playerData.positionX, data.playerData.positionY);
            Player.Instance.coin = data.playerData.coins;
        }
        if (PlayerHealth.Instance != null && data.playerData != null && data.playerData.health > 0)
            PlayerHealth.Instance.SetHealth(data.playerData.health);

        if (GlobalTime.Instance != null && data.timeData != null)
        {
            GlobalTime.Instance.minutes = data.timeData.minutes;
            GlobalTime.Instance.hours = data.timeData.hours;
            GlobalTime.Instance.days = data.timeData.days;
        }

        if (InventoryManager.Instance != null && data.inventoryData != null)
        {
            ClearInventory();
            if (data.inventoryData.slots != null)
                foreach (var slotData in data.inventoryData.slots)
                {
                    Item item = ItemRegistry.FindItem(slotData.itemName);
                    if (item != null) SpawnItemInSlotByType(item, slotData.count, slotData);
                }
        }

        if (data.worldData != null && data.worldData.objects != null)
        {
            ISaveable[] saveableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();
            foreach (var worldObjData in data.worldData.objects)
            {
                if (worldObjData.objectType == "PickupItem" && !worldObjData.isDead)
                {
                    SpawnPickupItem(worldObjData);
                    continue;
                }
                foreach (var saveable in saveableObjects)
                {
                    if (saveable.GetObjectId() == worldObjData.objectId)
                    {
                        saveable.LoadData(worldObjData);
                        break;
                    }
                }
            }
        }

        if (data.weatherData != null && WeatherManager.Instance != null)
            WeatherManager.Instance.LoadData(data.weatherData);

        if (data.slimeData != null && SlimeSpawner.Instance != null)
            SlimeSpawner.Instance.LoadSlimes(data.slimeData);

        Debug.Log("[SaveManager] Game data applied");
    }

    // ─── Collect ────────────────────────────────────────────────

    private GameData CollectGameData()
    {
        var invSlots = new List<InventorySlotData>();

        var pData = new PlayerData();
        if (Player.Instance != null)
        {
            pData.positionX = Player.Instance.transform.position.x;
            pData.positionY = Player.Instance.transform.position.y;
            pData.coins = Player.Instance.coin;
        }
        if (PlayerHealth.Instance != null)
            pData.health = PlayerHealth.Instance.CurrentHealth;

        var tData = new TimeData();
        if (GlobalTime.Instance != null)
        {
            tData.minutes = GlobalTime.Instance.minutes;
            tData.hours = GlobalTime.Instance.hours;
            tData.days = GlobalTime.Instance.days;
        }

        if (InventoryManager.Instance != null)
        {
            CollectSlots(InventoryManager.Instance.inventorySlots, invSlots, false, false, false);
            CollectSlots(InventoryManager.Instance.chestSlots, invSlots, true, false, false);
            CollectSlots(InventoryManager.Instance.craftSlots, invSlots, false, true, false);
            CollectSlots(InventoryManager.Instance.endCraftSlots, invSlots, false, false, true);
        }

        var worldObjs = new List<WorldObjectData>();
        foreach (var saveable in FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>())
            worldObjs.Add(saveable.GetSaveData());

        return new GameData
        {
            playerData = pData,
            inventoryData = new InventoryData { slots = invSlots.ToArray() },
            timeData = tData,
            worldData = new WorldData { objects = worldObjs.ToArray() },
            weatherData = WeatherManager.Instance != null ? WeatherManager.Instance.GetSaveData() : null,
            slimeData = SlimeSpawner.Instance != null ? SlimeSpawner.Instance.GetSaveData() : null
        };
    }

    private void CollectSlots(InventorySlot[] slots, List<InventorySlotData> list, bool isChest, bool isCraft, bool isEndCraft)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var item = slots[i].GetComponentInChildren<InventoryItem>();
            if (item != null)
                list.Add(new InventorySlotData
                {
                    slotIndex = i,
                    itemName = item.item.name,
                    count = item.count,
                    isChest = isChest,
                    isCraft = isCraft,
                    isEndCraft = isEndCraft
                });
        }
    }

    private void ClearInventory()
    {
        if (InventoryManager.Instance == null) return;
        foreach (var slot in InventoryManager.Instance.inventorySlots)
        { var x = slot.GetComponentInChildren<InventoryItem>(); if (x) Destroy(x.gameObject); }
        foreach (var slot in InventoryManager.Instance.chestSlots)
        { var x = slot.GetComponentInChildren<InventoryItem>(); if (x) Destroy(x.gameObject); }
        foreach (var slot in InventoryManager.Instance.craftSlots)
        { var x = slot.GetComponentInChildren<InventoryItem>(); if (x) Destroy(x.gameObject); }
        foreach (var slot in InventoryManager.Instance.endCraftSlots)
        { var x = slot.GetComponentInChildren<InventoryItem>(); if (x) Destroy(x.gameObject); }
    }

    private void SpawnItemInSlotByType(Item item, int count, InventorySlotData slotData)
    {
        if (InventoryManager.Instance.inventoryItemPrefab == null) return;
        InventorySlot slot = slotData.isCraft ? InventoryManager.Instance.craftSlots[slotData.slotIndex]
            : slotData.isEndCraft ? InventoryManager.Instance.endCraftSlots[slotData.slotIndex]
            : slotData.isChest ? InventoryManager.Instance.chestSlots[slotData.slotIndex]
            : InventoryManager.Instance.inventorySlots[slotData.slotIndex];

        var go = Instantiate(InventoryManager.Instance.inventoryItemPrefab, slot.transform);
        var invItem = go.GetComponent<InventoryItem>();
        invItem.InitialiseItem(item);
        invItem.count = count;
        invItem.RefreshCount();
    }

    [Header("Префабы лута на земле")]
    public GameObject[] pickupPrefabs;

    private void SpawnPickupItem(WorldObjectData data)
    {
        string itemName = data.itemName;
        int ci = itemName.IndexOf(" (");
        if (ci > 0) itemName = itemName.Substring(0, ci);

        GameObject prefab = null;
        if (pickupPrefabs != null)
            foreach (var p in pickupPrefabs)
            {
                if (p == null) continue;
                string n = p.name;
                int ni = n.IndexOf(" ("); if (ni > 0) n = n.Substring(0, ni);
                if (n == itemName) { prefab = p; break; }
            }

        if (prefab == null) { Debug.LogWarning($"[SaveManager] Prefab '{itemName}' not found"); return; }

        var go = Instantiate(prefab, new Vector3(data.posX, data.posY, 0), Quaternion.identity);
        var pickup = go.GetComponent<PickupItem>();
        if (pickup != null) { pickup.objectID = data.objectId; pickup.count = (int)data.health; }
    }
}

[Serializable]
public class SaveMetadata
{
    public bool hasSave;
    public string saveTime;
    public int day;
}

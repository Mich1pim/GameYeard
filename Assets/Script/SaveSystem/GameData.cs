using System;

[Serializable]
public class GameData
{
    public string saveTime;
    public PlayerData playerData;
    public InventoryData inventoryData;
    public TimeData timeData;
    public WorldData worldData;
}

[Serializable]
public class PlayerData
{
    public float positionX;
    public float positionY;
    public int coins;
}

[Serializable]
public class InventorySlotData
{
    public string itemName;
    public int count;
    public int slotIndex;
    public bool isChest;
    public bool isCraft;        // true = слот крафта
    public bool isEndCraft;     // true = слот результата крафта
}

[Serializable]
public class InventoryData
{
    public InventorySlotData[] slots;
}

[Serializable]
public class TimeData
{
    public int minutes;
    public int hours;
    public int days;
}

[Serializable]
public class WorldData
{
    public WorldObjectData[] objects;
}

[Serializable]
public class WorldObjectData
{
    public string objectId;
    public string objectType;
    public float health;
    public int stage;
    public bool isDead;
    public bool isRespawning;
    public float respawnTimer;
    public float growthTimer;
    public float lastGrowthMinutes;
    public bool isAppleReady;
    // Для PickupItem
    public float posX;
    public float posY;
    public string itemName;
}

public interface ISaveable
{
    string GetObjectId();
    WorldObjectData GetSaveData();
    void LoadData(WorldObjectData data);
}

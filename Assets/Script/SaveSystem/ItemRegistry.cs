using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Регистратор предметов для системы сохранений.
/// Навешивается на GameObject в сцене. Предметы регистрируются при старте.
/// </summary>
public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance { get; private set; }
    
    [Header("Все предметы в игре (перетащите из проекта)")]
    public List<Item> allItems = new List<Item>();
    
    private Dictionary<string, Item> _itemsByName = new Dictionary<string, Item>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void RegisterItems()
    {
        _itemsByName.Clear();
        
        foreach (Item item in allItems)
        {
            if (item != null && !_itemsByName.ContainsKey(item.name))
            {
                _itemsByName[item.name] = item;
            }
        }
        
        Debug.Log($"[ItemRegistry] Registered {_itemsByName.Count} items");
    }
    
    /// <summary>
    /// Найти предмет по имени
    /// </summary>
    public static Item FindItem(string itemName)
    {
        if (Instance == null)
        {
            Debug.LogError("[ItemRegistry] Instance is null!");
            return null;
        }
        
        if (Instance._itemsByName.TryGetValue(itemName, out Item item))
        {
            return item;
        }
        
        Debug.LogWarning($"[ItemRegistry] Item not found: {itemName}");
        return null;
    }
    
    /// <summary>
    /// Добавить предмет в реестр (можно вызывать динамически)
    /// </summary>
    public static void RegisterItem(Item item)
    {
        if (Instance == null)
        {
            Debug.LogError("[ItemRegistry] Instance is null!");
            return;
        }
        
        if (item != null && !Instance._itemsByName.ContainsKey(item.name))
        {
            Instance._itemsByName[item.name] = item;
        }
    }
}

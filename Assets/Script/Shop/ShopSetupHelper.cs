using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor-утилита для настройки предметов магазина
/// </summary>
public class ShopSetupHelper : MonoBehaviour
{
    [System.Serializable]
    public struct ShopItem
    {
        public Item item;
        public int price;
        public int count;
    }

    [Header("Предметы для продажи")]
    public List<ShopItem> shopItems = new List<ShopItem>();

#if UNITY_EDITOR
    [ContextMenu("Apply Shop Items")]
    public void ApplyShopItems()
    {
        // Безопасное получение компонента с проверкой
        ShopInteraction shop = GetComponent<ShopInteraction>();

        if (shop == null || ReferenceEquals(shop, null))
        {
            Debug.LogError("ShopSetupHelper: ShopInteraction не найден или был уничтожен! Добавьте компонент на этот объект.");
            return;
        }

        // Проверяем что объект всё ещё существует
        if (shop.gameObject == null)
        {
            Debug.LogError("ShopSetupHelper: GameObject компонента ShopInteraction уничтожен!");
            return;
        }

        // Очищаем старые предметы
        shop.ClearShopInventory();

        // Добавляем новые
        foreach (var shopItem in shopItems)
        {
            if (shopItem.item == null) continue;
            shop.AddItemToShop(shopItem.item, shopItem.count, shopItem.price);
        }

        Debug.Log($"ShopSetupHelper: Добавлено {shopItems.Count} предметов в магазин");
    }
#endif
}

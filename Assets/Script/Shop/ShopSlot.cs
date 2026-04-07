using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Слот магазина - отображает предмет для продажи/покупки.
/// Вешается на префаб InventorySlot.
/// </summary>
public class ShopSlot : MonoBehaviour, IDropHandler
{
    public ShopInteraction shopUI;
    public Item itemForSale;
    public int price = 0;
    public int count = 1;

    public void OnDrop(PointerEventData eventData)
    {
        // Игрок продаёт предмет магазину (перетаскивает из инвентаря в слот магазина)
        GameObject dropped = eventData.pointerDrag;
        InventoryItem draggedItem = dropped.GetComponent<InventoryItem>();
        if (draggedItem == null || draggedItem.item == null) return;

        // Добавляем предмет в магазин
        if (shopUI != null)
        {
            shopUI.SellItemToShop(draggedItem.item, draggedItem.count);
            Destroy(draggedItem.gameObject);
        }
    }

    public void Setup(Item item, int itemPrice, int itemCount)
    {
        itemForSale = item;
        price = itemPrice;
        count = itemCount;
    }

    public void Clear()
    {
        itemForSale = null;
        price = 0;
        count = 0;
    }
}

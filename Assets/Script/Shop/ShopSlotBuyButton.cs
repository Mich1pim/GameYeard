using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Обработчик клика по слоту магазина для покупки предмета.
/// Вешается на префаб InventorySlot (рядом с ShopSlot).
/// </summary>
public class ShopSlotBuyButton : MonoBehaviour, IPointerClickHandler
{
    public ShopSlot shopSlot;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (shopSlot == null || shopSlot.shopUI == null) return;
        if (shopSlot.itemForSale == null) return;

        shopSlot.shopUI.BuyItem(shopSlot.itemForSale);
    }
}

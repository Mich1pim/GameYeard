using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Sprite usingItemSprite;
    public Sprite notUsingItemSprite;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        InventoryItem draggedItem = dropped.GetComponent<InventoryItem>();
        if (draggedItem == null) return;

        // Если это слот результата крафта — запрещаем дроп
        if (GetComponent<CraftResultSlot>() != null) return;

        // Берём запомненный слот крафта из предмета (запомнен в OnBeginDrag,
        // т.к. к моменту OnDrop предмет уже на Canvas и GetComponentInParent вернёт null)
        CraftSlot originCraftSlot = draggedItem.originCraftSlot;

        // ------------------------------------------------------------
        // 1. Разделение стека правой кнопкой
        // ------------------------------------------------------------
        if (draggedItem.isRightClickDrag)
        {
            // Стак уже разделён в OnBeginDrag, просто добавляем перетаскиваемую половину в слот
            if (CanAcceptItems(draggedItem.item, draggedItem.count))
            {
                AddItems(draggedItem.item, draggedItem.count);
                Destroy(draggedItem.gameObject);
                draggedItem.dropSuccess = true;
            }
            // Если нельзя принять — dropSuccess остаётся false, предмет вернётся в OnEndDrag
            NotifyCraftSlot();
            // Уведомляем исходный слот крафта
            NotifyOriginCraftSlot(originCraftSlot);
            return;
        }

        // ------------------------------------------------------------
        // 2. Обычный дроп левой кнопкой (обмен / объединение)
        // ------------------------------------------------------------
        InventoryItem existingItem = GetComponentInChildren<InventoryItem>();

        if (existingItem != null)
        {
            if (existingItem.CanMergeWith(draggedItem))
            {
                existingItem.MergeStacks(draggedItem);
                draggedItem.dropSuccess = true;
            }
            else
            {
                // Меняем местами
                Transform existingItemParent = existingItem.transform.parent;
                Transform draggedItemParent = draggedItem.parentAfterDrag;

                existingItem.transform.SetParent(draggedItemParent);
                existingItem.transform.localPosition = Vector3.zero;
                if (existingItem.TryGetComponent(out RectTransform existingRT))
                    existingRT.anchoredPosition = Vector2.zero;

                draggedItem.transform.SetParent(existingItemParent);
                draggedItem.transform.localPosition = Vector3.zero;
                if (draggedItem.TryGetComponent(out RectTransform draggedRT))
                    draggedRT.anchoredPosition = Vector2.zero;
                draggedItem.parentAfterDrag = existingItemParent;
                draggedItem.dropSuccess = true;
            }
        }
        else
        {
            draggedItem.parentAfterDrag = transform;
            draggedItem.dropSuccess = true;
        }

        NotifyCraftSlot();
        NotifyOriginCraftSlot(originCraftSlot);
    }

    /// <summary>
    /// Уведомляет CraftSlot об изменении содержимого (для системы крафта).
    /// Вызываем с задержкой в 1 кадр, т.к. OnDrop вызывается ДО OnEndDrag,
    /// и предметы ещё не успели обновить свои родительские трансформы.
    /// </summary>
    private void NotifyCraftSlot()
    {
        CraftSlot craftSlot = GetComponent<CraftSlot>();
        if (craftSlot != null)
        {
            Invoke(nameof(DelayedNotifyCraftSlot), 0.01f);
        }
    }

    private void DelayedNotifyCraftSlot()
    {
        CraftSlot craftSlot = GetComponent<CraftSlot>();
        if (craftSlot != null)
        {
            craftSlot.OnSlotChanged();
        }
    }

    /// <summary>
    /// Уведомляет исходный слот крафта (после Destroy предмета).
    /// originCraftSlot запомнен ДО Destroy, поэтому вызываем напрямую.
    /// </summary>
    private void NotifyOriginCraftSlot(CraftSlot originCraftSlot)
    {
        if (originCraftSlot != null && originCraftSlot.craftingUI != null)
        {
            originCraftSlot.craftingUI.NotifySlotChanged();
        }
    }

    // Вспомогательные методы для разделения стека
    private bool CanAcceptItems(Item item, int count)
    {
        InventoryItem itemInSlot = GetComponentInChildren<InventoryItem>();
        if (itemInSlot == null) return true;

        if (itemInSlot.item == item && itemInSlot.item.stackable)
        {
            return itemInSlot.count + count <= InventoryManager.Instance.maxStackSize;
        }
        return false;
    }

    private void AddItems(Item item, int count)
    {
        InventoryItem itemInSlot = GetComponentInChildren<InventoryItem>();
        if (itemInSlot == null)
        {
            // Создаём новый предмет
            GameObject newItemGo = Instantiate(InventoryManager.Instance.inventoryItemPrefab, transform);
            InventoryItem newItem = newItemGo.GetComponent<InventoryItem>();
            newItem.InitialiseItem(item);
            newItem.count = count;
            newItem.RefreshCount();
        }
        else if (itemInSlot.item == item && itemInSlot.item.stackable)
        {
            itemInSlot.count += count;
            itemInSlot.RefreshCount();
        }
    }

    public void Select()
    {
        if (image != null)
            image.sprite = usingItemSprite;
    }

    public void Deselect()
    {
        if (image != null)
            image.sprite = notUsingItemSprite;
    }
}
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

        // ------------------------------------------------------------
        // 1. Разделение стека правой кнопкой
        // ------------------------------------------------------------
        if (draggedItem.isRightClickDrag)
        {
            int half = draggedItem.count / 2;
            if (half <= 0)
            {
                // Нечего делить (1 предмет)
                draggedItem.dropHandled = true;
                return;
            }

            // Проверяем, можно ли добавить половину в этот слот
            if (CanAcceptItems(draggedItem.item, half))
            {
                // Уменьшаем исходный стек
                draggedItem.count -= half;
                
                // Если исходный стек стал пуст — удаляем объект
                if (draggedItem.count <= 0)
                {
                    Destroy(draggedItem.gameObject);
                }
                else
                {
                    draggedItem.RefreshCount();
                }

                // Добавляем половину в текущий слот
                AddItems(draggedItem.item, half);
                draggedItem.dropHandled = true;
            }
            else
            {
                // Нельзя добавить — отменяем разделение
                draggedItem.dropHandled = true;
            }
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
            }
            else
            {
                // Меняем местами
                Transform existingItemParent = existingItem.transform.parent;
                Transform draggedItemParent = draggedItem.parentAfterDrag;

                existingItem.transform.SetParent(draggedItemParent);
                existingItem.transform.localPosition = Vector3.zero;

                draggedItem.transform.SetParent(existingItemParent);
                draggedItem.transform.localPosition = Vector3.zero;
                draggedItem.parentAfterDrag = existingItemParent;
            }
        }
        else
        {
            draggedItem.parentAfterDrag = transform;
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
        image.sprite = usingItemSprite;
    }

    public void Deselect()
    {
        image.sprite = notUsingItemSprite;
    }
}
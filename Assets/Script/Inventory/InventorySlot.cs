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
        
        if (draggedItem != null)
        {
            InventoryItem existingItem = GetComponentInChildren<InventoryItem>();
            
            if (existingItem != null)
            {
                if (existingItem.CanMergeWith(draggedItem))
                {
                    existingItem.MergeStacks(draggedItem);
                }
                else
                {
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
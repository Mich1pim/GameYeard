using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Sprite usingItemSprite;
    public Sprite notUsingItemSprite;

    void Awake()
    {
        Deselect();
    }
    public void Select()
    {
        image.sprite = usingItemSprite;
    }

    public void Deselect()
    {
        image.sprite = notUsingItemSprite;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag =  transform;
        }
    }
}

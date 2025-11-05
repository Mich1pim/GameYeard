using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image image;
    public TextMeshProUGUI countText;
    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        if (image != null)
        {
            image.sprite = newItem.image;
            image.enabled = true;
            image.color = Color.white;
        }
        RefreshCount();
    }

    public void RefreshCount()
    {
        if (countText != null)
        {
            countText.text = count.ToString();
            countText.gameObject.SetActive(count > 1);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            parentCanvas = FindObjectOfType<Canvas>();

        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(parentCanvas.transform, true);
        
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out Vector2 localPoint);

        rectTransform.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        
        if (parentAfterDrag != null)
        {
            transform.SetParent(parentAfterDrag, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }

    public bool CanMergeWith(InventoryItem otherItem)
    {
        return item != null && 
            otherItem != null && 
            item == otherItem.item && 
            item.stackable && 
            count < InventoryManager.Instance.maxStackSize;
    }

    public void MergeStacks(InventoryItem otherItem)
    {
        int totalCount = count + otherItem.count;
        int maxStack = InventoryManager.Instance.maxStackSize;

        if (totalCount <= maxStack)
        {
            count = totalCount;
            RefreshCount();
            Destroy(otherItem.gameObject);
        }
        else
        {
            
            count = maxStack;
            otherItem.count = totalCount - maxStack;
            RefreshCount();
            otherItem.RefreshCount();
        }
    }
}
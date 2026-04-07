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

    [HideInInspector] public bool isRightClickDrag = false;
    [HideInInspector] public bool dropSuccess = false;
    private InventoryItem splitRemainder;
    private Transform splitRemainderParent;
    private Vector3 splitRemainderLocalScale;

    // Запоминаем слот крафта при начале перетаскивания
    // (в OnDrop предмет уже на Canvas и GetComponentInParent вернёт null)
    [HideInInspector] public CraftSlot originCraftSlot;

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
        isRightClickDrag = (eventData.button == PointerEventData.InputButton.Right);
        dropSuccess = false;
        splitRemainder = null;
        splitRemainderParent = null;

        // Запоминаем слот крафта ДО начала перетаскивания
        originCraftSlot = GetComponentInParent<CraftSlot>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            parentCanvas = FindObjectOfType<Canvas>();

        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        splitRemainderParent = parentAfterDrag;
        splitRemainderLocalScale = transform.localScale;

        // При правом клике — разделяем стак пополам сразу
        if (isRightClickDrag && count > 1)
        {
            int splitCount = count / 2;
            int remainCount = count - splitCount;

            // Обновляем текущий элемент (перетаскиваемый)
            count = splitCount;
            RefreshCount();

            // Создаём остаток в исходном слоте через Instantiate
            if (InventoryManager.Instance != null && InventoryManager.Instance.inventoryItemPrefab != null)
            {
                GameObject remainderObj = Instantiate(InventoryManager.Instance.inventoryItemPrefab, splitRemainderParent);
                splitRemainder = remainderObj.GetComponent<InventoryItem>();
                splitRemainder.item = item;
                splitRemainder.count = remainCount;
                splitRemainder.InitialiseItem(item);
            }
        }

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

        if (isRightClickDrag)
        {
            if (dropSuccess)
            {
                // Дроп успешен — остаток уже на месте, перетаскиваемый предмет уничтожается в слоте
                splitRemainder = null;
            }
            else
            {
                // Дроп не удался — возвращаем перетаскиваемый стак обратно к остатку
                if (splitRemainder != null && splitRemainder.gameObject != null)
                {
                    splitRemainder.count += count;
                    splitRemainder.RefreshCount();
                    Destroy(gameObject);
                }
                else if (splitRemainderParent != null)
                {
                    transform.SetParent(splitRemainderParent, false);
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.localScale = splitRemainderLocalScale;
                }
                splitRemainder = null;
            }

            // Уведомляем слот крафта ДО проверки на уничтожение,
            // т.к. при правом клике OnDrop мог Destroy(this) и gameObject может быть null
            NotifyCraftSlotIfOrigin();

            isRightClickDrag = false;
            dropSuccess = false;
            return;
        }

        // Объект мог быть уничтожен во время дропа (правый клик с успешным дропом)
        if (this == null || gameObject == null) return;

        // Обычный дроп (левая кнопка)
        if (parentAfterDrag != null)
        {
            transform.SetParent(parentAfterDrag, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        // Уведомляем слот крафта для левого клика
        NotifyCraftSlotIfOrigin();

        isRightClickDrag = false;
        dropSuccess = false;
    }

    /// <summary>
    /// Уведомляет исходный слот крафта после завершения перетаскивания.
    /// НЕ вызываем здесь NotifySlotChanged - это сделает InventorySlot.OnDrop(),
    /// чтобы избежать двойного уведомления и дублирования результатов крафта.
    /// </summary>
    private void NotifyCraftSlotIfOrigin()
    {
        // НЕ вызываем NotifySlotChanged здесь - это сделает InventorySlot.OnDrop()
        // с небольшой задержкой, чтобы предмет успел переместиться в новый слот
        originCraftSlot = null;
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
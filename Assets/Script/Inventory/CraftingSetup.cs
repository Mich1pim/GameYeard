using UnityEngine;

/// <summary>
/// Вешается на InventoryManager. При Start настраивает систему крафта.
/// Заменяет необходимость запуска Tools → Setup Crafting System.
/// </summary>
public class CraftingSetup : MonoBehaviour
{
    [Header("Ссылки (назначаются в инспекторе)")]
    [Tooltip("Контейнер 9 слотов крафта (InventorySlotsForCraft)")]
    public Transform craftSlotsContainer;

    [Tooltip("Контейнер слота результата (InventorySlotsEndCraft)")]
    public Transform endCraftSlotContainer;

    private InventoryManager _invManager;
    private CraftingManager _craftingManager;
    private CraftingUI _craftingUI;

    void Awake()
    {
        _invManager = GetComponent<InventoryManager>();

        // Добавляем CraftingManager
        _craftingManager = GetComponent<CraftingManager>();
        if (_craftingManager == null)
        {
            _craftingManager = gameObject.AddComponent<CraftingManager>();
        }

        // Добавляем CraftingUI
        _craftingUI = GetComponent<CraftingUI>();
        if (_craftingUI == null)
        {
            _craftingUI = gameObject.AddComponent<CraftingUI>();
        }

        // Находим контейнеры автоматически, если не назначены
        if (craftSlotsContainer == null)
        {
            GameObject go = GameObject.Find("InventorySlotsForCraft");
            if (go != null) craftSlotsContainer = go.transform;
        }

        if (endCraftSlotContainer == null)
        {
            GameObject go = GameObject.Find("InventorySlotsEndCraft");
            if (go != null) endCraftSlotContainer = go.transform;
        }

        if (craftSlotsContainer == null)
        {
            Debug.LogError("CraftingSetup: InventorySlotsForCraft не найден!");
            enabled = false;
            return;
        }

        if (endCraftSlotContainer == null)
        {
            Debug.LogError("CraftingSetup: InventorySlotsEndCraft не найден!");
            enabled = false;
            return;
        }

        SetupCraftSlots();
        SetupEndCraftSlot();

        // Назначаем ссылки
        _craftingUI.craftSlots = _invManager.craftSlots;
        _craftingUI.endCraftSlots = _invManager.endCraftSlots;
        _craftingUI.inventoryItemPrefab = _invManager.inventoryItemPrefab;

        Debug.Log("CraftingSetup: система крафта настроена!");
    }

    private void SetupCraftSlots()
    {
        int count = Mathf.Min(craftSlotsContainer.childCount, 9);
        _invManager.craftSlots = new InventorySlot[count];

        for (int i = 0; i < count; i++)
        {
            Transform child = craftSlotsContainer.GetChild(i);
            _invManager.craftSlots[i] = child.GetComponent<InventorySlot>();

            CraftSlot craftSlot = child.GetComponent<CraftSlot>();
            if (craftSlot == null)
            {
                craftSlot = child.gameObject.AddComponent<CraftSlot>();
            }
            craftSlot.craftingUI = _craftingUI;
        }
    }

    private void SetupEndCraftSlot()
    {
        if (endCraftSlotContainer.childCount == 0) return;

        Transform child = endCraftSlotContainer.GetChild(0);
        _invManager.endCraftSlots = new InventorySlot[1] { child.GetComponent<InventorySlot>() };

        CraftResultSlot resultSlot = child.GetComponent<CraftResultSlot>();
        if (resultSlot == null)
        {
            resultSlot = child.gameObject.AddComponent<CraftResultSlot>();
        }
        resultSlot.craftingUI = _craftingUI;
    }
}

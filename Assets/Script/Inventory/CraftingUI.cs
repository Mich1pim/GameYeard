using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingUI : MonoBehaviour
{
    [Header("Crafting Grid")]
    public InventorySlot[] craftSlots;    // 9 слотов 3x3
    public InventorySlot[] endCraftSlots; // 1 слот результата

    [Header("Result Item Prefab")]
    public GameObject inventoryItemPrefab;

    private Item[] _currentGrid = new Item[9];
    private CraftRecipe _currentRecipe;
    private int _resultCount = 0;

    void Start()
    {
        UpdateCraftingGrid();
    }

    /// <summary>
    /// Публичный метод для уведомления об изменении слота крафта.
    /// Вызывается из CraftSlot.OnSlotChanged().
    /// </summary>
    public void NotifySlotChanged()
    {
        UpdateCraftingGrid();
    }

    /// <summary>
    /// Обновляет текущую сетку крафта и проверяет рецепт.
    /// </summary>
    private void UpdateCraftingGrid()
    {
        bool changed = false;

        for (int i = 0; i < 9; i++)
        {
            InventoryItem itemInSlot = craftSlots[i].GetComponentInChildren<InventoryItem>();
            Item item = itemInSlot != null ? itemInSlot.item : null;

            if (_currentGrid[i] != item)
            {
                changed = true;
                _currentGrid[i] = item;
            }
        }

        if (changed)
        {
            CheckRecipe();
        }
    }

    /// <summary>
    /// Проверяет рецепт и обновляет слот результата.
    /// </summary>
    private void CheckRecipe()
    {
        _currentRecipe = CraftingManager.Instance.FindRecipe(_currentGrid);

        // Очищаем слот результата
        ClearResultSlot();

        if (_currentRecipe != null)
        {
            Debug.Log($"CheckRecipe: найден рецепт {_currentRecipe.name}, результат: {_currentRecipe.resultItem.name} x{_currentRecipe.resultCount}");
            _resultCount = _currentRecipe.resultCount;
            ShowResultItem(_currentRecipe.resultItem);
        }
        else
        {
            Debug.Log("CheckRecipe: рецепт не найден");
            _resultCount = 0;
        }
    }

    /// <summary>
    /// Показывает предмет результата в слоте.
    /// </summary>
    private void ShowResultItem(Item item)
    {
        if (endCraftSlots == null || endCraftSlots.Length == 0) return;

        InventorySlot resultSlot = endCraftSlots[0];
        GameObject resultItemGo = Instantiate(inventoryItemPrefab, resultSlot.transform);
        InventoryItem inventoryItem = resultItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
        inventoryItem.count = _resultCount;
        inventoryItem.RefreshCount();

        // Отключаем raycastTarget, чтобы клики проходили к CraftResultSlot
        if (inventoryItem.image != null)
        {
            inventoryItem.image.raycastTarget = false;
        }
        if (inventoryItem.countText != null && inventoryItem.countText.GetComponent<UnityEngine.UI.Image>() != null)
        {
            inventoryItem.countText.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        }
    }

    /// <summary>
    /// Очищает слот результата.
    /// </summary>
    private void ClearResultSlot()
    {
        if (endCraftSlots == null || endCraftSlots.Length == 0) return;

        InventorySlot resultSlot = endCraftSlots[0];
        InventoryItem itemInSlot = resultSlot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Destroy(itemInSlot.gameObject);
        }
    }

    /// <summary>
    /// Вызывается из CraftResultSlot при клике на слот результата — забирает результат в инвентарь.
    /// </summary>
    public void TakeResult()
    {
        if (_currentRecipe == null)
        {
            Debug.LogWarning("TakeResult: _currentRecipe == null");
            return;
        }

        // Проверяем, что в слоте результата есть предмет
        if (endCraftSlots == null || endCraftSlots.Length == 0) return;
        InventoryItem resultItem = endCraftSlots[0].GetComponentInChildren<InventoryItem>();
        if (resultItem == null)
        {
            Debug.LogWarning("TakeResult: нет предмета в слоте результата");
            return;
        }

        Debug.Log($"TakeResult: создаём {resultItem.count} x {_currentRecipe.resultItem.name}");

        // Пытаемся добавить результат в инвентарь нужное количество раз
        int countToAdd = resultItem.count;
        int addedCount = 0;

        for (int i = 0; i < countToAdd; i++)
        {
            if (InventoryManager.Instance.AddItem(_currentRecipe.resultItem))
            {
                addedCount++;
            }
            else
            {
                Debug.LogWarning("Инвентарь заполнен! Не удалось добавить все предметы.");
                break;
            }
        }

        Debug.Log($"TakeResult: добавлено {addedCount} предметов, потребляем ингредиенты...");

        if (addedCount > 0)
        {
            // Сначала очищаем слот результата
            ClearResultSlot();

            // Удаляем использованные предметы из сетки крафта
            ConsumeIngredients();

            // Обновляем рецепт ПРИНУДИТЕЛЬНО (не через UpdateCraftingGrid, 
            // т.к. Destroy отложенный и changed будет false)
            RefreshRecipeAfterConsume();
        }
    }

    /// <summary>
    /// Обновляет сетку и проверяет рецепт после потребления ингредиентов.
    /// Вызывается вручную, т.к. Destroy отложенный.
    /// </summary>
    private void RefreshRecipeAfterConsume()
    {
        for (int i = 0; i < 9; i++)
        {
            InventoryItem itemInSlot = craftSlots[i].GetComponentInChildren<InventoryItem>();
            // Учитываем, что объект может быть помечен на уничтожение (count <= 0)
            if (itemInSlot != null && itemInSlot.count > 0)
            {
                _currentGrid[i] = itemInSlot.item;
            }
            else
            {
                _currentGrid[i] = null;
            }
        }

        _currentRecipe = CraftingManager.Instance.FindRecipe(_currentGrid);

        // Очищаем слот результата
        ClearResultSlot();

        if (_currentRecipe != null)
        {
            _resultCount = _currentRecipe.resultCount;
            ShowResultItem(_currentRecipe.resultItem);
            Debug.Log($"RefreshRecipeAfterConsume: повторный рецепт {_currentRecipe.name}");
        }
        else
        {
            _resultCount = 0;
            Debug.Log("RefreshRecipeAfterConsume: ингредиентов недостаточно");
        }
    }

    /// <summary>
    /// Вызывается при дропе предмета на слот результата — забирает результат в инвентарь.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        TakeResult();
    }

    /// <summary>
    /// Удаляет по одному предмету из каждого слота, используемого в рецепте.
    /// </summary>
    private void ConsumeIngredients()
    {
        for (int i = 0; i < 9; i++)
        {
            // Удаляем только из слотов, которые используются в рецепте
            if (_currentGrid[i] != null)
            {
                InventoryItem itemInSlot = craftSlots[i].GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null)
                {
                    itemInSlot.count--;
                    if (itemInSlot.count <= 0)
                    {
                        Destroy(itemInSlot.gameObject);
                    }
                    else
                    {
                        itemInSlot.RefreshCount();
                    }
                }
            }
        }

        // Обновляем сетку после потребления
        for (int i = 0; i < 9; i++)
        {
            InventoryItem itemInSlot = craftSlots[i].GetComponentInChildren<InventoryItem>();
            _currentGrid[i] = itemInSlot != null ? itemInSlot.item : null;
        }

        // Уведомляем CraftSlot об изменениях
        for (int i = 0; i < 9; i++)
        {
            CraftSlot craftSlot = craftSlots[i].GetComponent<CraftSlot>();
            if (craftSlot != null)
            {
                craftSlot.OnSlotChanged();
            }
        }
    }
}

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;
#endif

/// <summary>
/// Editor-утилита для настройки системы крафта на существующих UI объектах.
/// Запуск: Tools → Setup Crafting System в меню Unity.
/// </summary>
public class SetupCraftingSystem : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Setup Crafting System")]
    public static void Setup()
    {
        // Важно: инвентарь должен быть активен в сцене.
        // Если инвентарь — это prefab instance, сначала откройте его (I) в play mode,
        // затем остановите play mode и запустите эту утилиту.
        // Или просто убедитесь, что InventoryBackGround активен в сцене.

        // 1. Находим InventoryManager на сцене
        InventoryManager invManager = Object.FindObjectOfType<InventoryManager>();
        if (invManager == null)
        {
            Debug.LogError("InventoryManager не найден на сцене! Убедитесь, что сцена MainMap загружена.");
            return;
        }

        // Находим InventoryBackGround (панель инвентаря)
        GameObject inventoryBackGround = null;
        foreach (var go in Object.FindObjectsOfType<GameObject>())
        {
            if (go.name == "InventoryBackGround")
            {
                inventoryBackGround = go;
                break;
            }
        }

        if (inventoryBackGround == null)
        {
            Debug.LogError("InventoryBackGround не найден! Откройте инвентарь (I) в play mode, затем остановите и запустите Setup.");
            return;
        }

        GameObject invManagerGo = invManager.gameObject;

        // 2. Добавляем CraftingManager
        CraftingManager craftingManager = invManagerGo.GetComponent<CraftingManager>();
        if (craftingManager == null)
        {
            craftingManager = invManagerGo.AddComponent<CraftingManager>();
            craftingManager.recipes = new System.Collections.Generic.List<CraftRecipe>();
            Debug.Log("CraftingManager добавлен на InventoryManager");
        }

        // 3. Добавляем CraftingUI
        CraftingUI craftingUI = invManagerGo.GetComponent<CraftingUI>();
        if (craftingUI == null)
        {
            craftingUI = invManagerGo.AddComponent<CraftingUI>();
            Debug.Log("CraftingUI добавлен на InventoryManager");
        }

        // 4. Находим контейнеры слотов
        GameObject craftContainerGO = FindChildByName(inventoryBackGround, "InventorySlotsForCraft");
        GameObject endCraftContainerGO = FindChildByName(inventoryBackGround, "InventorySlotsEndCraft");

        if (craftContainerGO == null)
        {
            Debug.LogError("InventorySlotsForCraft не найден внутри InventoryBackGround!");
            return;
        }

        if (endCraftContainerGO == null)
        {
            Debug.LogError("InventorySlotsEndCraft не найден внутри InventoryBackGround!");
            return;
        }

        Transform craftContainer = craftContainerGO.transform;
        Transform endCraftContainer = endCraftContainerGO.transform;

        // 5. Назначаем craftSlots
        InventorySlot[] craftSlots = new InventorySlot[9];
        for (int i = 0; i < craftContainer.childCount && i < 9; i++)
        {
            craftSlots[i] = craftContainer.GetChild(i).GetComponent<InventorySlot>();

            // Добавляем CraftSlot на каждый слот
            CraftSlot craftSlotComponent = craftContainer.GetChild(i).GetComponent<CraftSlot>();
            if (craftSlotComponent == null)
            {
                craftSlotComponent = craftContainer.GetChild(i).gameObject.AddComponent<CraftSlot>();
            }
            craftSlotComponent.craftingUI = craftingUI;
        }

        invManager.craftSlots = craftSlots;
        craftingUI.craftSlots = craftSlots;

        // 6. Назначаем endCraftSlots
        if (endCraftContainer.childCount > 0)
        {
            InventorySlot[] endCraftSlots = new InventorySlot[1];
            endCraftSlots[0] = endCraftContainer.GetChild(0).GetComponent<InventorySlot>();
            invManager.endCraftSlots = endCraftSlots;
            craftingUI.endCraftSlots = endCraftSlots;

            // Добавляем CraftResultSlot на слот результата
            GameObject resultSlotGo = endCraftContainer.GetChild(0).gameObject;
            CraftResultSlot resultSlot = resultSlotGo.GetComponent<CraftResultSlot>();
            if (resultSlot == null)
            {
                resultSlot = resultSlotGo.AddComponent<CraftResultSlot>();
            }
            resultSlot.craftingUI = craftingUI;

            // Убедимся, что Image на самом слоте получает клики
            Image slotImage = resultSlotGo.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.raycastTarget = true;
            }
        }

        // 7. Назначаем prefab
        if (invManager.inventoryItemPrefab != null)
        {
            craftingUI.inventoryItemPrefab = invManager.inventoryItemPrefab;
        }

        // 8. Сохраняем изменения в prefab instance
        PrefabUtility.RecordPrefabInstancePropertyModifications(invManagerGo);

        EditorUtility.SetDirty(invManagerGo);
        EditorUtility.SetDirty(craftingManager.gameObject);

        Debug.Log("=== Система крафта настроена! ===");
        Debug.Log($"- CraftSlots: {craftSlots.Length} слотов");
        Debug.Log($"- EndCraftSlot: назначен + CraftResultSlot");
        Debug.Log($"- CraftingManager: добавлен с пустым списком рецептов");
        Debug.Log($"- CraftingUI: добавлен и настроен");
        Debug.Log("");
        Debug.Log("Следующие шаги:");
        Debug.Log("1. Создайте рецепты: Tools → Create Craft Recipes");
        Debug.Log("2. Перетащите созданные рецепты в CraftingManager.recipes");
    }

    private static GameObject FindChildByName(GameObject parent, string name)
    {
        if (parent == null) return null;
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.name == name)
                return child.gameObject;

            GameObject found = FindChildByName(child.gameObject, name);
            if (found != null)
                return found;
        }
        return null;
    }
#endif
}

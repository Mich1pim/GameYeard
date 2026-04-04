using UnityEngine;

/// <summary>
/// Компонент на слоте крафта — уведомляет CraftingUI при изменении содержимого.
/// </summary>
public class CraftSlot : MonoBehaviour
{
    public CraftingUI craftingUI;

    void Start()
    {
        if (craftingUI == null)
            craftingUI = FindObjectOfType<CraftingUI>();
    }

    /// <summary>
    /// Вызывается из InventorySlot при дропе предмета.
    /// </summary>
    public void OnSlotChanged()
    {
        if (craftingUI != null)
        {
            craftingUI.NotifySlotChanged();
        }
    }
}

using UnityEngine;

public class FoodEating : MonoBehaviour
{
    [Header("Еда (перетащи Item-ассеты)")]
    [SerializeField] private Item[] foodItems;

    [Header("Восстановление здоровья за 1 еду")]
    [SerializeField] private int healAmount = 1;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (InventoryManager.Instance == null || PlayerHealth.Instance == null) return;
        if (PlayerHealth.Instance.CurrentHealth >= PlayerHealth.Instance.MaxHealth) return;

        Item selected = InventoryManager.Instance.GetSelectedItem(false);
        if (selected == null) return;
        if (!IsFoodItem(selected)) return;

        InventoryManager.Instance.GetSelectedItem(true);
        PlayerHealth.Instance.Heal(healAmount);
    }

    private bool IsFoodItem(Item item)
    {
        foreach (Item food in foodItems)
            if (food != null && food == item) return true;
        return false;
    }
}

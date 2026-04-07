using UnityEngine;

/// <summary>
/// Кнопка закрытия магазина. Вешается на кнопку X в UI магазина.
/// </summary>
public class ShopCloseButton : MonoBehaviour
{
    public ShopInteraction shopInteraction;

    public void CloseShop()
    {
        if (shopInteraction != null)
        {
            shopInteraction.CloseShop();
        }
        else
        {
            // Пытаемся найти автоматически
            shopInteraction = FindObjectOfType<ShopInteraction>();
            if (shopInteraction != null)
            {
                shopInteraction.CloseShop();
            }
        }
    }
}

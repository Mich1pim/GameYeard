using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Вешается на слот результата крафта. При клике забирает результат в инвентарь.
/// </summary>
public class CraftResultSlot : MonoBehaviour, IPointerClickHandler
{
    public CraftingUI craftingUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"CraftResultSlot: клик по слоту результата, craftingUI={craftingUI != null}");
        if (craftingUI != null)
        {
            craftingUI.TakeResult();
        }
    }
}

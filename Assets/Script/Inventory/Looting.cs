using UnityEngine;

public class Looting : MonoBehaviour
{
    public Item itemToPickUp;

    private bool wasPickedUp = false;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !wasPickedUp)
        {
            wasPickedUp = true;
            bool itemAdded = InventoryManager.Instance.AddItem(itemToPickUp);

            if (itemAdded)
            {
                Destroy(gameObject);
            }
            else
            {
                wasPickedUp = false;
            }
        }
    }
}
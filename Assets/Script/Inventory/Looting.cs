using UnityEngine;

public class Looting : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item itemToPickUp;
    
    private bool wasPickedUp = false;

    public void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player") && !wasPickedUp)
        {
            wasPickedUp = true;
            bool itemAdded = inventoryManager.AddItem(itemToPickUp);
            
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
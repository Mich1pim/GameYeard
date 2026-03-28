using UnityEngine;

public class ItemsUsing : MonoBehaviour
{
    public static ItemsUsing Instance { get; private set; }
    public InventoryManager inventoryManager;
    public Animator animator;
    public bool axe = false;
    public bool pickAxe = false;

    void Awake()
    {
        Instance = this;
    }

    void FixedUpdate()
    {
        Tools();
        animator.SetBool("Axe", IsAxe());
        animator.SetBool("PickAxe", IsPickAxe());
    }
    public void Tools()
    {
        string ax = "Axe";
        string pickax = "PickAxe";
        Item receivedItem = inventoryManager.GetSelectedItem(false);
        if (receivedItem != null)
        {
            if (receivedItem.name == ax)
            {
                axe = true;
                pickAxe = false;
            }
            else if (receivedItem.name == pickax)
            {
                pickAxe = true;
                axe = false;
            }
            else
            {
                pickAxe = false;
                axe = false;
            }
        }
        else
        {
            pickAxe = false;
            axe = false;
        }
    }

    public bool IsAxe()
    {
        return axe;
    }

    public bool IsPickAxe()
    {
        return pickAxe;
    }
}

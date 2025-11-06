using UnityEngine;

public class Chest : UsingAllObject
{
    public GameObject ChestSlots;
    public GameObject InventorySlots;
    public bool isOpen = false;
    
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (isOpen && distance > 1f)
        {
            CloseChest();
        }
    }

    protected override void Use()
    {
        base.Use();
        animator.SetTrigger("Open");
        Open();
    }

    protected override void UnUse()
    {
        base.UnUse();
        animator.SetTrigger("Close");
        Close();
    }

    public void OpenCloseChest()
    {
        if (!isOpen)
        {
            Use();
            isOpen = true;
        }
        else
        {
            UnUse();
            isOpen = false;
        }
    }
    
    public void CloseChest()
    {
        if (isOpen)
        {
            animator.SetTrigger("Close");
            Close();
            isOpen = false;
            isOpened = false;
        }
    }

    public void Close()
    {
        ChestSlots.SetActive(false);
        InventorySlots.SetActive(false);
        isOpen = false;
    }

    public void Open()
    {
        ChestSlots.SetActive(true);
        InventorySlots.SetActive(true);
        isOpen = true;
    }  
}
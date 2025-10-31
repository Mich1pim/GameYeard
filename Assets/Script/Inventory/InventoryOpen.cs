    using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class InventoryOpen : MonoBehaviour
{
    public GameObject inventory;
    public bool inventoryOpen = false;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryOpen)
                Close();
            else
                Open();
        }
    }

    public void Close()
    {
        inventory.SetActive(false);
        inventoryOpen = false;
    }

    public void Open()
    {
        inventory.SetActive(true);
        inventoryOpen = true;
    }   
}

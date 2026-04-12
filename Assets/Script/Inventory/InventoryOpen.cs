using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryOpen : MonoBehaviour
{
    public GameObject inventory;
    public bool inventoryOpen = false;

    private bool _inputEnabled = true;


    void Update()
    {
        if (!_inputEnabled) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryOpen)
                Close();
            else
                Open();
        }
    }

    public void DisableInput()
    {
        _inputEnabled = false;
        if (inventoryOpen)
            Close();
    }

    public void EnableInput()
    {
        _inputEnabled = true;
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

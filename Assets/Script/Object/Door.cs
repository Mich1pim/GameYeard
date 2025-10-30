using UnityEngine;

public class Door : UsingAllObject
{
    public bool isOpen = false;
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Use()
    {
        base.Use();
        animator.SetTrigger("Open");
    }

    protected override void UnUse()
    {
        base.UnUse();
        animator.SetTrigger("Close");
    }

    public void OpenCloseDoor()
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
}

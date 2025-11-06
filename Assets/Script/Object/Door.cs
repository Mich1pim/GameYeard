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
        isOpen = true;
        animator.SetTrigger("Open");
    }

    protected override void UnUse()
    {
        base.UnUse();
        isOpen = false;
        animator.SetTrigger("Close");
    }
}

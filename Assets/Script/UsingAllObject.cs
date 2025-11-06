using UnityEngine;

public class UsingAllObject : MonoBehaviour
{
    protected Animator animator;
    protected float distance;
    protected bool isOpened = false;
    protected KeyCode interactionKey = KeyCode.E;

    protected virtual void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        
    }

    protected virtual void Update()
    {
        distance = Vector3.Distance(GameObject.Find("Player").transform.position, gameObject.transform.position);
        if (Input.GetKeyDown(interactionKey) && distance < 1.0f)
        {
            if (isOpened)
                UnUse();
            else
                Use();
        }
    }

    protected virtual void Use()
    {
        isOpened = true;
    }

    protected virtual void UnUse()
    {
        isOpened = false;
    }
}

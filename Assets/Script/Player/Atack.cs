using UnityEngine;

public class Atack : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string attackBoolName = "Attack";
    
    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // При зажатии ЛКМ — true, при отпускании — false
        bool isAttacking = Input.GetMouseButton(0);
        animator.SetBool(attackBoolName, isAttacking);
        if(isAttacking == true)
        {
            Player.Instance.moveSpeed = 0;
        }
        else
        {
            Player.Instance.moveSpeed = 1.75f;
        }
    }
}

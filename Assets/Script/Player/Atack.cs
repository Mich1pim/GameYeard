using UnityEngine;

public class Atack : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string attackBoolName = "Attack";

    private bool _inputEnabled = true;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!_inputEnabled) return;

        bool isAttacking = Input.GetMouseButton(0);
        animator.SetBool(attackBoolName, isAttacking);
        if (isAttacking == true)
        {
            Player.Instance.moveSpeed = 0;
        }
        else
        {
            Player.Instance.moveSpeed = 1.75f;
        }
    }

    public void DisableInput()
    {
        _inputEnabled = false;
        animator.SetBool(attackBoolName, false);
        Player.Instance.moveSpeed = 1.75f;
    }

    public void EnableInput()
    {
        _inputEnabled = true;
    }
}

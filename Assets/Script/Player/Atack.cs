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
        if (DialogManager.Instance != null && DialogManager.Instance.IsActive) return;

        bool isAttacking = Input.GetMouseButton(0);
        animator.SetBool(attackBoolName, isAttacking);

        bool hasTool = ItemsUsing.Instance != null &&
                       (ItemsUsing.Instance.IsAxe() || ItemsUsing.Instance.IsPickAxe());

        if (isAttacking && hasTool)
            Player.Instance.moveSpeed = 0;
        else
            Player.Instance.moveSpeed = 1.75f;
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

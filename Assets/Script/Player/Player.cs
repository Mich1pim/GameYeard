using Mono.Cecil.Cil;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;

    private PlayerInputActions playerInputActions;
    private Rigidbody2D rb;
    private Animator animator;

    private const string horizontal = "Horizontal";
    private const string vertical = "Vertical";
    private const string lastHorizontal = "LastHorizontal";
    private const string lastVertical = "LastVertical";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }


    private Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        animator.SetFloat(horizontal, inputVector.x);
        animator.SetFloat(vertical, inputVector.y);

        if (inputVector != Vector2.zero)
        {
            animator.SetFloat(lastHorizontal, inputVector.x);
            animator.SetFloat(lastVertical, inputVector.y);
        }

        return inputVector;
    }
    void FixedUpdate()
    {
        Vector2 inputVector = GetMovementVector();

        inputVector = inputVector.normalized;

        rb.MovePosition(rb.position + inputVector * (moveSpeed * Time.fixedDeltaTime));
    }

}

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundStickForce = -2f;

    private CharacterController controller;
    private PlayerInputHandler input;
    private PlayerCursor playerCursor;

    private Vector3 verticalVelocity;

    public bool IsRunning { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        playerCursor = GetComponent<PlayerCursor>();

        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = input.Movement;

        IsRunning = Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = IsRunning ? runSpeed : moveSpeed;

        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = groundStickForce;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        controller.Move(verticalVelocity * Time.deltaTime);
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("==== MOVIMENTO ====")]
    [Range(1f, 20f)] public float walkSpeed = 7.5f;
    [Range(1f, 360f)] public float rotationSpeed = 10f;

    [Header("==== DASH ====")]
    [Range(5f, 30f)] public float dashSpeed = 18f;
    [Range(0.1f, 1f)] public float dashDuration = 0.25f;
    [Range(0f, 1f)] public float dashControl = 0.5f;
    [Range(0.1f, 2f)] public float dashCooldown = 0.6f;

    [Header("==== FÍSICA FAKE ====")]
    public float gravity = -2f;
    public float groundStickForce = -5f; // ajuda a não "flutuar"

    // DASH
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    // COMPONENTES
    private CharacterController controller;

    // MOVIMENTO
    private Vector3 inputDirection;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Inputs();
        HandleDash();
        HandleMovement();
        HandleRotation();
    }

    void Inputs()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(horizontal, 0f, vertical);

        if (inputDirection.magnitude > 1f)
            inputDirection.Normalize();
    }

    void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;

        // Ativar dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && inputDirection.sqrMagnitude > 0.01f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            // 🔥 IMPORTANTE: trava direção do dash no INPUT, não no forward
            dashDirection = inputDirection.normalized;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    void HandleMovement()
    {
        Vector3 move;

        if (isDashing)
        {
            // 🔥 mistura direção original + input atual
            Vector3 controlledDirection = 
                (dashDirection * (1f - dashControl)) + 
                (inputDirection * dashControl);

            if (controlledDirection.sqrMagnitude > 0.01f)
                controlledDirection.Normalize();

            move = controlledDirection * dashSpeed;
        }
        else
        {
            move = inputDirection * walkSpeed;
        }

        // Gravidade arcade
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = groundStickForce;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 finalMove = move + velocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    void HandleRotation()
    {
        Vector3 lookDirection;

        if (isDashing)
        {
            // usa o input pra virar DURANTE o dash
            lookDirection = inputDirection.sqrMagnitude > 0.01f 
                ? inputDirection 
                : dashDirection;
        }
        else
        {
            lookDirection = inputDirection;
        }

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("==== MOVIMENTO ====")]
    [Range(1f, 20f)] public float walkSpeed = 6.7f;
    [Range(1f, 360f)] public float rotationSpeed = 20f;

    [Header("==== DASH ====")]
    [Range(5f, 30f)]  public float dashSpeed     = 21.2f;
    [Range(0.1f, 1f)] public float dashDuration  = 0.324f;
    [Range(0.1f, 2f)] public float dashCooldown  = 0.15f;

    [Tooltip("Quão rápido o jogador RETOMA controle após o pico do dash (menor = mais tempo preso)")]
    [Range(1f, 20f)]  public float dashReturnControl = 2.03f;

    [Tooltip("Quanto o input atual desvia a trajetória DO dash (0 = nenhum, 1 = total)")]
    [Range(0f, 1f)]   public float dashSteering = 0.926f;

    [Header("==== FÍSICA FAKE ====")]
    public float gravity         = -1f;
    public float groundStickForce = -5f;

    // ── Componentes ──────────────────────────────────────────────────
    private CharacterController controller;

    // ── Movimento ────────────────────────────────────────────────────
    private Vector3 inputDirection;   // direção bruta do jogador
    private Vector3 velocity;         // componente vertical (gravidade)

    // ── Dash ─────────────────────────────────────────────────────────
    private bool    isDashing;
    private float   dashTimer;
    private float   dashCooldownTimer;
    private Vector3 dashDirection;    // direção travada no momento do dash
    private float   currentDashSpeed; // velocidade interpolada durante o dash

    // ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        ReadInput();
        TryActivateDash();
        HandleMovement();
        HandleRotation();
    }

    // ─── INPUT ───────────────────────────────────────────────────────

    void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(h, 0f, v);

        if (inputDirection.magnitude > 1f)
            inputDirection.Normalize();
    }

    // ─── DASH ────────────────────────────────────────────────────────

    void TryActivateDash()
    {
        dashCooldownTimer -= Time.deltaTime;

        if (!Input.GetKeyDown(KeyCode.LeftShift)) return;
        if (dashCooldownTimer > 0f)               return;
        if (isDashing)                            return;

        isDashing         = true;
        dashTimer         = dashDuration;
        dashCooldownTimer = dashCooldown;
        currentDashSpeed  = dashSpeed;

        // ✅ Usa o input atual; se parado, usa o forward do player
        dashDirection = inputDirection.sqrMagnitude > 0.01f
            ? inputDirection.normalized
            : transform.forward;
    }

    // ─── MOVIMENTO ───────────────────────────────────────────────────

    void HandleMovement()
    {
        Vector3 horizontalMove;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            // ── Curva suave de desaceleração usando Lerp ──
            // No início do dash: currentDashSpeed ≈ dashSpeed
            // No fim do dash:    currentDashSpeed ≈ walkSpeed
            currentDashSpeed = Mathf.Lerp(
                currentDashSpeed,
                walkSpeed,
                dashReturnControl * Time.deltaTime
            );

            // ── Steering: permite desviar levemente durante o dash ──
            Vector3 steeredDirection = dashDirection;

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                steeredDirection = Vector3.Lerp(
                    dashDirection,
                    inputDirection.normalized,
                    dashSteering * Time.deltaTime * 10f   // *10 pra ter "peso"
                );

                // Atualiza dashDirection gradualmente (steering persistente)
                dashDirection = steeredDirection.normalized;
            }

            horizontalMove = steeredDirection * currentDashSpeed;

            // ── Encerra o dash quando o timer zera ──
            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        else
        {
            horizontalMove = inputDirection * walkSpeed;
        }

        // ── Gravidade arcade ──────────────────────────────────────────
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = groundStickForce;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // ── Move ──────────────────────────────────────────────────────
        Vector3 finalMove = horizontalMove + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    // ─── ROTAÇÃO ─────────────────────────────────────────────────────

    void HandleRotation()
    {
        // Durante o dash: gira suavemente pra direção do DASH
        // Fora do dash:   gira pra direção do input
        Vector3 lookDirection = isDashing ? dashDirection : inputDirection;

        if (lookDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        // Rotação mais rápida durante o dash pra parecer responsiva
        float currentRotSpeed = isDashing ? rotationSpeed * 2f : rotationSpeed;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            currentRotSpeed * Time.deltaTime
        );
    }
}
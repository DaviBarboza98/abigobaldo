using UnityEngine;
using UnityEngine.UI;

public class Jogador : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float movSpeed = 8f;
    [SerializeField] private float runSpeed = 11f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Chão")]
    [SerializeField] private GroundCheck groundCheck;
    private bool isGrounded;

    private CharacterController controller;

    [Header("Interação (Cutting Bar)")]
    public float tempoParaCompletar = 2f;
    private float progresso = 0f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina;
    public float staminaDrain = 20f;
    public float staminaRegen = 10f;

    [Header("UI")]
    public Slider staminaBar;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;
    }

    void Update()
    {
        CheckGround();
        HandleMovement();
        HandleStamina();
        HandleUI();
        HandleInteraction();
    }

    // =========================
    // CHÃO
    // =========================
    void CheckGround()
    {
        isGrounded = groundCheck.isGrounded;
    }

    // =========================
    // MOVIMENTO + ROTAÇÃO
    // =========================
    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && stamina > 0;
        float currentSpeed = isRunning ? runSpeed : movSpeed;

        // Rotação suave
        if (inputDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Movimento
        Vector3 move = inputDir * currentSpeed;
        controller.Move(move * Time.deltaTime);

        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 20f * Time.deltaTime);
        }
    }

    // =========================
    // STAMINA
    // =========================
    void HandleStamina()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(x, 0f, z).normalized;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && stamina > 0;

        if (isRunning && inputDir != Vector3.zero)
            stamina -= staminaDrain * Time.deltaTime;
        else
            stamina += staminaRegen * Time.deltaTime;

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    // =========================
    // UI
    // =========================
    void HandleUI()
    {
        staminaBar.value = stamina;
    }

    // =========================
    // INTERAÇÃO (CUTTING BAR)
    // =========================
    void HandleInteraction()
    {
        if (Input.GetKey(KeyCode.E))
            progresso += Time.deltaTime;
        else
            progresso -= Time.deltaTime;

        progresso = Mathf.Clamp(progresso, 0f, tempoParaCompletar);

        if (progresso >= tempoParaCompletar)
        {
            // AÇÃO AQUI
            Debug.Log("Ação completa!");
            progresso = 0f;
        }
    }
}
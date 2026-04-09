using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Jogador : MonoBehaviour
{
    public float speed = 8f;
    public float runSpeed = 11f;
    public float rotationSpeed = 720f; // velocidade de rotação (graus por segundo)

    public GroundCheck gc;
    private bool isGrounded;

    private CharacterController controller;

    public float tempoParaCompletar = 2f;
    private float progresso = 0f;

    // STAMINA
    public float maxStamina = 100f;
    public float stamina;
    public float staminaDrain = 20f;
    public float staminaRegen = 10f;

    public Slider staminaBar;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;
    }

    void Update()
    {
        isGrounded = gc.isGrounded;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(x, 0f, z).normalized;

        // CORRIDA
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && stamina > 0;
        float currentSpeed = isRunning ? runSpeed : speed;

        if (isRunning && inputDir != Vector3.zero)
            stamina -= staminaDrain * Time.deltaTime;
        else
            stamina += staminaRegen * Time.deltaTime;

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        // =========================
        // ROTAÇÃO SUAVE (CORRIGIDO)
        // =========================
        if (inputDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // =========================
        // MOVIMENTO BASEADO NA DIREÇÃO
        // =========================
        Vector3 move = inputDir * currentSpeed;

        controller.Move(move * Time.deltaTime);

        // GRUDAR NO CHÃO (sem bug de pulo)
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * 20f * Time.deltaTime);
        }

        // UI
        staminaBar.value = stamina;

        // CUTTING BAR (ARRUMADO)
        if (Input.GetKey(KeyCode.E)){
            progresso += Time.deltaTime;
        }
        else{
            progresso -= Time.deltaTime;
        }

        progresso = Mathf.Clamp(progresso, 0f, tempoParaCompletar);

        if (progresso >= tempoParaCompletar)
        {
            // AÇÃO
            progresso = 0f;
        }
    }
}
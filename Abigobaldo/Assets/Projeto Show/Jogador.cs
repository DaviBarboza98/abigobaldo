using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Jogador : MonoBehaviour
{
    public float speed = 8f;
    public float runSpeed = 11f;
    public float gravidade = -9.81f;

    public GroundCheck gc;
    private bool isGrounded;

    private CharacterController controller;
    private Vector3 velocity;

    //public Slider barra;
    public float tempoParaCompletar = 2f;

    private float progresso = 0f;

    // STAMINA
    public float maxStamina = 100f;
    public float stamina;
    public float staminaDrain = 20f;
    public float staminaRegen = 10f;

    //COLISOES
    bool encostandoKitchenTable = false;
    bool encostandoCuttingTable = false;

    public Slider staminaBar;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;
    }

    void Update()
    {
        isGrounded = gc.isGrounded;

        //INPUT SEM SUAVIZAÇÃO
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //DIREÇÃO GLOBAL (evita bug de rotação)
        Vector3 move = new Vector3(x, 0f, z);
        float currentSpeed = speed;

        //STAMINA + CORRIDA
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && stamina > 0;

        if (isRunning)
        {
            currentSpeed = runSpeed;
            stamina -= staminaDrain * Time.deltaTime;
        }
        else
        {
            stamina += staminaRegen * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        //ROTACIONA PARA DIREÇÃO DO MOVIMENTO
        if (move != Vector3.zero)
        {
            transform.forward = move;
        }

        //MOVIMENTO DIRETO (SEM DESLIZE)
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);

        //GRAVIDADE
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravidade * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //UI
        staminaBar.value = stamina;

        //CUTTING BAR
        bool podeInteragir = encostandoKitchenTable;

        if (podeInteragir)

            if (Input.GetKey(KeyCode.E))
            {
                progresso += Time.deltaTime;
            }
            else
            {
                progresso -= Time.deltaTime;
            }
            else
            {
                progresso -= Time.deltaTime * 2f; // mais rápido se sair do objeto
            }

        // trava o valor entre 0 e o tempo máximo
        progresso = Mathf.Clamp(progresso, 0f, tempoParaCompletar);

        // atualiza a barra (normalizada 0-1)
        //barra.value = progresso / tempoParaCompletar;

        // completou
        if (progresso >= tempoParaCompletar)
        {
            //PEGAR O ITEM OU DEIXAR ELE CORTADO SE DER

            
            // reseta se quiser
            progresso = 0f;
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Kitchen_table"))
            encostandoKitchenTable = true;

        if (collision.gameObject.CompareTag("Cutting_table"))
            encostandoCuttingTable = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Kitchen_table"))
            encostandoKitchenTable = false;

        if (collision.gameObject.CompareTag("Cutting_table"))
            encostandoCuttingTable = false;
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("==== CONFIGURAÇÕES ====")]
    [Range(1f, 20f)] public float walkSpeed = 7.5f;
    [Range(5f, 20f)] public float dashSpeed = 15f;
    [Range(1f, 360f)] public float rotationSpeed = 7.5f;
    public float gravity = -1f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 inputDirection;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Inputs();
        Movement();
        Rotation();
    }

    void Inputs()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        inputDirection = new Vector3(horizontal, 0f, vertical);

        if (inputDirection.magnitude > 1f)
            inputDirection.Normalize();
    }

    void Movement()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? dashSpeed : walkSpeed;

        Vector3 move = inputDirection * currentSpeed;

        velocity.y = gravity;

        Vector3 finalMove = move + velocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    void Rotation()
    {
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
using UnityEngine;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundStickForce = -2f;

        private CharacterController controller;
        private PlayerInput input;
        private Vector3 verticalVelocity;

        public bool IsRunning { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInput>();
        }

        private void Update()
        {
            Move();
            ApplyGravity();
        }

        private void Move()
        {
            IsRunning = input.RunPressed;
            float speed = IsRunning ? runSpeed : moveSpeed;
            Vector3 direction = transform.right * input.Movement.x + transform.forward * input.Movement.y;

            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            controller.Move(direction * speed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && verticalVelocity.y < 0f)
                verticalVelocity.y = groundStickForce;
            else
                verticalVelocity.y += gravity * Time.deltaTime;

            controller.Move(verticalVelocity * Time.deltaTime);
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            runSpeed = Mathf.Max(moveSpeed, runSpeed);
            groundStickForce = Mathf.Min(0f, groundStickForce);
        }
    }
}

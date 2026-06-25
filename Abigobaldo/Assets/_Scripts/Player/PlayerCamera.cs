using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Mouse")]
    [SerializeField] private float sensitivity = 2f;

    [Header("Pitch Limits")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("FOV")]
    [SerializeField] private float defaultFov = 70f;
    [SerializeField] private float runningFov = 80f;
    [SerializeField] private float fovSmoothSpeed = 8f;

    [Header("Head Bob")]
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float runBobSpeed = 14f;
    [SerializeField] private float bobAmountY = 0.05f;
    [SerializeField] private float bobAmountX = 0.025f;
    [SerializeField] private float bobReturnSpeed = 8f;

    private PlayerInputHandler input;
    private PlayerMovement movement;

    private float pitch;
    private float bobTimer;
    private Vector3 originalPivotPosition;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();

        originalPivotPosition = cameraPivot.localPosition;
    }

    private void Update()
    {
        HandleLook();
        HandleFov();
        HandleHeadBob();
    }

    private void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        Vector2 lookInput = input.Look;

        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraPivot.localRotation = Quaternion.Euler(
            pitch,
            0f,
            0f
        );
    }

    private void HandleFov()
    {
        float targetFov = movement.IsRunning
            ? runningFov
            : defaultFov;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFov,
            fovSmoothSpeed * Time.deltaTime
        );
    }

    private void HandleHeadBob()
    {
        bool isMoving = input.Movement.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            float currentBobSpeed = movement.IsRunning
                ? runBobSpeed
                : walkBobSpeed;

            bobTimer += Time.deltaTime * currentBobSpeed;

            float bobY = Mathf.Sin(bobTimer) * bobAmountY;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmountX;

            cameraPivot.localPosition = originalPivotPosition + new Vector3(
                bobX,
                bobY,
                0f
            );
        }
        else
        {
            bobTimer = 0f;

            cameraPivot.localPosition = Vector3.Lerp(
                cameraPivot.localPosition,
                originalPivotPosition,
                bobReturnSpeed * Time.deltaTime
            );
        }
    }
}
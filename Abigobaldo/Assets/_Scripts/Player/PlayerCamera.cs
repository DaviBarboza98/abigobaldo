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

    private PlayerInputHandler input;
    private PlayerMovement movement;

    private float pitch;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        HandleLook();
        HandleFov();
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
}
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    [Header("-- REFERENCIAS --")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform model;

    [Header("-- VALORES --")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
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

        HideHead();
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

        if (input.RotateHeld)
            return;

        Vector2 lookInput = input.Look;
        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        transform.rotation *= Quaternion.Euler(0f, mouseX, 0f);

        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
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

    private void HideHead()
    {
        if (model == null)
            return;

        string[] partsToHide =
        {
            "OlhoE",
            "OlhoD",
            "SombrancelhaE",
            "SombrancelhaD",
            "Nariz",
            "CabeÃ§a",
            "Cabelin",
            "Bigode"
        };

        foreach (string partName in partsToHide)
        {
            Transform part = model.Find(partName);

            if (part == null)
                continue;

            MeshRenderer renderer = part.GetComponent<MeshRenderer>();

            if (renderer != null)
                renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }
}

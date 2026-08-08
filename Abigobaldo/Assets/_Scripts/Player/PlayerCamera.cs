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
    [SerializeField] private string[] hiddenFirstPersonParts = { "Head" };

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

        HideFirstPersonParts();
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

    private void HideFirstPersonParts()
    {
        if (model == null)
            return;

        HideModelParts(hiddenFirstPersonParts);

        string[] legacyPartsToHide =
        {
            "OlhoE",
            "OlhoD",
            "SombrancelhaE",
            "SombrancelhaD",
            "Nariz",
            "Cabeca",
            "Cabelin",
            "Bigode"
        };

        HideModelParts(legacyPartsToHide);
    }

    private void HideModelParts(string[] partNames)
    {
        if (partNames == null)
            return;

        foreach (string partName in partNames)
            HideModelPart(partName);
    }

    private void HideModelPart(string partName)
    {
        if (string.IsNullOrWhiteSpace(partName))
            return;

        Transform part = FindDeepChild(model, partName);

        if (part == null)
            return;

        foreach (Renderer renderer in part.GetComponentsInChildren<Renderer>())
            renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (root.name == childName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindDeepChild(root.GetChild(i), childName);

            if (result != null)
                return result;
        }

        return null;
    }
}

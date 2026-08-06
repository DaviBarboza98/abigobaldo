using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerCamera : MonoBehaviour
{
    [Header("-- REFERÊNCIAS --")]

    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform itemHolder;

    [Header("-- MODELO --")]

    [SerializeField] private Transform model;

    [Header("-- VALORES --")]

    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("-- FOV --")]

    [SerializeField] private float defaultFov = 70f;
    [SerializeField] private float runningFov = 80f;
    [SerializeField] private float fovSmoothSpeed = 8f;

    [Header("-- COLISÃO DO ITEM --")]

    [SerializeField] private float collisionCheckRadius = 0.03f;
    [SerializeField] private LayerMask itemCollisionLayers = ~0;

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

        // star: Se estiver segurando R, não mexe a câmera
        if (input.RotatePressed)
        return;

        Vector2 lookInput = input.Look;

        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        // ==========================================
        // ROTAÇÃO HORIZONTAL
        // ==========================================

        Quaternion horizontalRotation =
            transform.rotation *
            Quaternion.Euler(0f, mouseX, 0f);

        if (CanRotateCamera(horizontalRotation, pitch))
        {
            transform.rotation = horizontalRotation;
        }

        // ==========================================
        // ROTAÇÃO VERTICAL
        // ==========================================

        float newPitch = pitch - mouseY;

        newPitch = Mathf.Clamp(
            newPitch,
            minPitch,
            maxPitch
        );

        if (CanRotateCamera(transform.rotation, newPitch))
        {
            pitch = newPitch;

            cameraPivot.localRotation =
                Quaternion.Euler(
                    pitch,
                    0f,
                    0f
                );
        }
    }

    private bool CanRotateCamera(
        Quaternion playerRotation,
        float targetPitch
    )
    {
        if (itemHolder == null)
            return true;

        Item item =
            itemHolder.GetComponentInChildren<Item>();

        if (item == null)
            return true;

        Collider[] itemColliders =
            item.GetComponentsInChildren<Collider>();

        if (itemColliders.Length == 0)
            return true;

        // Guardamos as rotações atuais.
        Quaternion oldPlayerRotation =
            transform.rotation;

        Quaternion oldPivotRotation =
            cameraPivot.localRotation;

        // Aplicamos temporariamente a rotação desejada.
        transform.rotation = playerRotation;

        cameraPivot.localRotation =
            Quaternion.Euler(
                targetPitch,
                0f,
                0f
            );

        Physics.SyncTransforms();

        bool collisionDetected = false;

        foreach (Collider itemCollider in itemColliders)
        {
            if (itemCollider == null)
                continue;

            if (!itemCollider.enabled)
                continue;

            Vector3 center =
                itemCollider.bounds.center;

            float radius =
                GetColliderRadius(itemCollider);

            Collider[] overlaps =
                Physics.OverlapSphere(
                    center,
                    radius + collisionCheckRadius,
                    itemCollisionLayers,
                    QueryTriggerInteraction.Ignore
                );

            foreach (Collider other in overlaps)
            {
                if (other == itemCollider)
                    continue;

                if (other.transform.IsChildOf(itemHolder))
                    continue;

                if (other.transform.IsChildOf(transform))
                    continue;

                collisionDetected = true;
                break;
            }

            if (collisionDetected)
                break;
        }

        // Restauramos imediatamente.
        transform.rotation = oldPlayerRotation;

        cameraPivot.localRotation =
            oldPivotRotation;

        Physics.SyncTransforms();

        return !collisionDetected;
    }

    private float GetColliderRadius(Collider collider)
    {
        if (collider is SphereCollider sphere)
        {
            float scale =
                Mathf.Max(
                    collider.transform.lossyScale.x,
                    collider.transform.lossyScale.y,
                    collider.transform.lossyScale.z
                );

            return sphere.radius * scale;
        }

        if (collider is CapsuleCollider capsule)
        {
            float scale =
                Mathf.Max(
                    collider.transform.lossyScale.x,
                    collider.transform.lossyScale.y,
                    collider.transform.lossyScale.z
                );

            return capsule.radius * scale;
        }

        Bounds bounds = collider.bounds;

        return Mathf.Max(
            bounds.extents.x,
            bounds.extents.y,
            bounds.extents.z
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
            "Cabeça",
            "Cabelin",
            "Bigode"
        };

        foreach (string partName in partsToHide)
        {
            Transform part = model.Find(partName);

            if (part == null)
                continue;

            MeshRenderer renderer =
                part.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderer.shadowCastingMode =
                    ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
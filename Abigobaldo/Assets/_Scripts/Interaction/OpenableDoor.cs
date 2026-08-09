using UnityEngine;

public class OpenableDoor : MonoBehaviour, IHoldInteractable
{
    private enum DoorRotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Abertura")]
    [SerializeField] private Transform pivot;
    [SerializeField] private DoorRotationAxis rotationAxis = DoorRotationAxis.Z;
    [SerializeField] private float maxOpenAngle = 90f;
    [SerializeField] private float followSpeed = 30f;
    [SerializeField] private bool invertDirection;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs;

    private Quaternion closedRotation;
    private float currentAngle;
    private float holdStartYaw;
    private float holdStartAngle;
    private bool isHolding;

    private Transform Pivot => pivot != null ? pivot : transform;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Door);
        closedRotation = Pivot.localRotation;
    }

    public void BeginHold(PlayerInteraction player)
    {
        isHolding = true;
        holdStartAngle = currentAngle;
        holdStartYaw = player != null && player.PlayerCamera != null
            ? player.PlayerCamera.transform.eulerAngles.y
            : transform.eulerAngles.y;
    }

    public void UpdateHold(PlayerInteraction player)
    {
        if (!isHolding || player == null || player.PlayerCamera == null)
            return;

        float direction = invertDirection ? 1f : -1f;
        float yawDelta = Mathf.DeltaAngle(holdStartYaw, player.PlayerCamera.transform.eulerAngles.y);
        float targetAngle = Mathf.Clamp(holdStartAngle + yawDelta * direction, 0f, maxOpenAngle);

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, followSpeed * Time.deltaTime);
        Pivot.localRotation = closedRotation * Quaternion.Euler(GetRotationEuler(currentAngle * (invertDirection ? -1f : 1f)));
    }

    public void EndHold(PlayerInteraction player)
    {
        isHolding = false;

        if (showDebugLogs)
            Debug.Log($"{name}: porta em {currentAngle:0} graus.");
    }

    private void OnValidate()
    {
        maxOpenAngle = Mathf.Clamp(maxOpenAngle, 0f, 180f);
        followSpeed = Mathf.Max(0.01f, followSpeed);
    }

    private Vector3 GetRotationEuler(float angle)
    {
        switch (rotationAxis)
        {
            case DoorRotationAxis.X:
                return new Vector3(angle, 0f, 0f);
            case DoorRotationAxis.Y:
                return new Vector3(0f, angle, 0f);
            default:
                return new Vector3(0f, 0f, angle);
        }
    }
}

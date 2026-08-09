using UnityEngine;

public class OpenableDoor : MonoBehaviour, IHoldInteractable
{
    [Header("Abertura")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float maxOpenAngle = 90f;
    [SerializeField] private float followSpeed = 14f;
    [SerializeField] private bool invertDirection;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs;

    private Quaternion closedRotation;
    private float currentAngle;
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
    }

    public void UpdateHold(PlayerInteraction player)
    {
        if (!isHolding || player == null || player.PlayerCamera == null)
            return;

        Vector3 toPlayer = player.transform.position - Pivot.position;
        Vector3 playerForward = player.PlayerCamera.transform.forward;
        float signed = Vector3.SignedAngle(toPlayer.FlattenY(), playerForward.FlattenY(), Vector3.up);
        float direction = invertDirection ? -1f : 1f;
        float targetAngle = Mathf.Clamp(signed * direction, 0f, maxOpenAngle);

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, followSpeed * Time.deltaTime);
        Pivot.localRotation = closedRotation * Quaternion.Euler(0f, currentAngle * direction, 0f);
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
}

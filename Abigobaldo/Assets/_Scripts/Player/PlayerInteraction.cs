using UnityEngine;

[RequireComponent(typeof(PlayerHold))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("=== REFERENCES ===")]
    [SerializeField] private Camera playerCamera;

    [Header("=== SETTINGS ===")]
    [SerializeField] private float interactDistance = 3f;

    private PlayerHold hold;

    private void Awake()
    {
        hold = GetComponent<PlayerHold>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f));

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            return;

        Item item = hit.collider.GetComponent<Item>();

        if (item != null)
        {
            if (!hold.HasItem)
                hold.PickUp(item);

            return;
        }

        // Futuramente:
        // Counter
        // FryingPan
        // Plate
        // Trash
    }
}
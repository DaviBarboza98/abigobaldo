using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ItemHolder itemHolder;

    [Header("Interação")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private PlayerInputHandler input;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        HandleInteraction();
        HandleDrop();
        HandleThrow();
    }

    private void HandleInteraction()
    {
        if (!input.InteractPressed)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayers
        ))
            return;

        ItemSpawner spawner =
            hit.collider.GetComponentInParent<ItemSpawner>();

        if (spawner != null)
        {
            HandleSpawner(spawner);
            return;
        }

        Item item =
            hit.collider.GetComponentInParent<Item>();

        if (item != null)
        {
            HandleItem(item);
        }
    }

    private void HandleSpawner(ItemSpawner spawner)
    {
        if (!itemHolder.IsEmpty())
            return;

        Item item = spawner.SpawnItem();

        if (item == null)
            return;

        if (!itemHolder.TryPickUp(item))
            Destroy(item.gameObject);
    }

    private void HandleItem(Item item)
    {
        if (!itemHolder.IsEmpty())
            return;

        itemHolder.TryPickUp(item);
    }

    private void HandleDrop()
    {
        if (!input.DropPressed)
            return;

        itemHolder.DropItem();
    }

    private void HandleThrow()
    {
        if (!input.ThrowPressed)
            return;

        itemHolder.ThrowItem();
    }
}
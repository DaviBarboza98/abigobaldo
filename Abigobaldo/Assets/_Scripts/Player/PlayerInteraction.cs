using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ItemHolder itemHolder;

    [Header("Interacao")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private float highlightRefreshInterval = 0.04f;

    private PlayerInputHandler input;
    private readonly RaycastHit[] interactionHits = new RaycastHit[16];
    private Highlightable currentHighlight;
    private float nextHighlightRefreshTime;

    public ItemHolder ItemHolder => itemHolder;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (itemHolder == null)
            itemHolder = GetComponentInChildren<ItemHolder>();
    }

    private void Update()
    {
        if (itemHolder == null)
        {
            ClearCurrentHighlight();
            return;
        }

        UpdateHighlight();
        TryInteract();
        HandleDrop();
        HandleThrow();
        HandleHoldZoom();
        HandleRotation();
    }

    private void TryInteract()
    {
        if (!input.InteractPressed)
            return;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!TryGetInteractionHit(ray, out RaycastHit hit))
            return;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (TryInteractWithHeldPlate(interactable))
            return;

        if (!itemHolder.IsEmpty() && interactable != null)
        {
            interactable.Interact(this);
            return;
        }

        if (TryHandleEmptyHandContainer(interactable))
            return;

        ItemSpawner spawner = hit.collider.GetComponentInParent<ItemSpawner>();

        if (spawner != null)
        {
            HandleSpawner(spawner);
            return;
        }

        Item item = hit.collider.GetComponentInParent<Item>();

        if (item != null)
        {
            HandleItem(item);
            return;
        }

        if (interactable != null)
            interactable.Interact(this);
    }

    private bool TryGetInteractionHit(Ray ray, out RaycastHit bestHit)
    {
        int hitCount = Physics.RaycastNonAlloc(
            ray,
            interactionHits,
            interactionDistance,
            interactionLayers,
            QueryTriggerInteraction.Collide
        );

        bestHit = default;
        float bestDistance = float.PositiveInfinity;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = interactionHits[i];

            if (IsCurrentHeldItemCollider(hit.collider))
                continue;

            if (hit.distance >= bestDistance)
                continue;

            bestHit = hit;
            bestDistance = hit.distance;
        }

        return bestDistance < float.PositiveInfinity;
    }

    private void UpdateHighlight()
    {
        if (Time.unscaledTime < nextHighlightRefreshTime)
            return;

        nextHighlightRefreshTime = Time.unscaledTime + highlightRefreshInterval;

        if (playerCamera == null)
        {
            ClearCurrentHighlight();
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!TryGetInteractionHit(ray, out RaycastHit hit))
        {
            ClearCurrentHighlight();
            return;
        }

        Highlightable nextHighlight = GetHighlightableFromHit(hit.collider);

        if (nextHighlight == currentHighlight)
            return;

        ClearCurrentHighlight();
        currentHighlight = nextHighlight;

        if (currentHighlight != null)
            currentHighlight.SetHighlighted(true);
    }

    private Highlightable GetHighlightableFromHit(Collider hitCollider)
    {
        if (hitCollider == null)
            return null;

        GameObject highlightRoot = GetHighlightRoot(hitCollider);

        if (highlightRoot == null)
            return null;

        Highlightable highlightable = highlightRoot.GetComponent<Highlightable>();

        if (highlightable == null)
            highlightable = highlightRoot.AddComponent<Highlightable>();

        return highlightable;
    }

    private static GameObject GetHighlightRoot(Collider hitCollider)
    {
        Item item = hitCollider.GetComponentInParent<Item>();

        if (item != null)
            return item.gameObject;

        ItemSpawner spawner = hitCollider.GetComponentInParent<ItemSpawner>();

        if (spawner != null)
            return spawner.gameObject;

        IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
        Component interactableComponent = interactable as Component;

        return interactableComponent != null
            ? interactableComponent.gameObject
            : null;
    }

    private void ClearCurrentHighlight()
    {
        if (currentHighlight == null)
            return;

        currentHighlight.SetHighlighted(false);
        currentHighlight = null;
    }

    private bool IsCurrentHeldItemCollider(Collider targetCollider)
    {
        if (targetCollider == null || itemHolder == null || itemHolder.IsEmpty())
            return false;

        Item currentItem = itemHolder.CurrentItem;

        return currentItem != null && targetCollider.transform.IsChildOf(currentItem.transform);
    }

    private bool TryInteractWithHeldPlate(IInteractable interactable)
    {
        if (itemHolder.IsEmpty())
            return false;

        PlateContainer heldPlate = itemHolder.CurrentItem.GetComponent<PlateContainer>();

        if (heldPlate == null)
            return false;

        if (interactable is ItemContainer container)
        {
            container.TryMoveOutputToPlate(heldPlate);
            return true;
        }

        return false;
    }

    private bool TryHandleEmptyHandContainer(IInteractable interactable)
    {
        if (!itemHolder.IsEmpty())
            return false;

        ItemContainer container = interactable as ItemContainer;

        if (container == null)
            return false;

        if (container.Type == ContainerType.Liquidificador)
        {
            container.Interact(this);
            return true;
        }

        if (container.HasReadyOutput)
        {
            container.Interact(this);
            return true;
        }

        if (container.TryPickUpContainer(itemHolder))
            return true;

        container.Interact(this);
        return true;
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
        {
            TryPlateLooseItem(item);
            return;
        }

        itemHolder.TryPickUp(item);
    }

    private bool TryPlateLooseItem(Item item)
    {
        if (item == null || itemHolder.IsEmpty())
            return false;

        if (item == itemHolder.CurrentItem)
            return false;

        PlateContainer heldPlate = itemHolder.CurrentItem.GetComponent<PlateContainer>();

        if (heldPlate == null)
            return false;

        return heldPlate.TryAddLooseItem(item);
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

    private void HandleRotation()
    {
        if (!input.RotateHeld)
            return;

        if (itemHolder.IsEmpty())
            return;

        if (playerCamera == null)
            return;

        itemHolder.RotateItem(input.Look, playerCamera.transform);
    }

    private void HandleHoldZoom()
    {
        if (Mathf.Approximately(input.HoldZoom, 0f))
            return;

        itemHolder.ZoomHeldItem(input.HoldZoom);
    }

    private void OnValidate()
    {
        interactionDistance = Mathf.Max(0f, interactionDistance);
        highlightRefreshInterval = Mathf.Max(0.01f, highlightRefreshInterval);
    }
}

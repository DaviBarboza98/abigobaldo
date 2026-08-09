using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ItemHolder itemHolder;

    [Header("Interacao")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers;
    [SerializeField] private float highlightRefreshInterval = 0.04f;

    private PlayerInputHandler input;
    private readonly RaycastHit[] interactionHits = new RaycastHit[16];
    private Highlightable currentHighlight;
    private IHoldInteractable currentHoldInteractable;
    private float nextHighlightRefreshTime;

    public ItemHolder ItemHolder => itemHolder;
    public Camera PlayerCamera => playerCamera;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (itemHolder == null)
            itemHolder = GetComponentInChildren<ItemHolder>();

        if (interactionLayers.value == 0 || interactionLayers.value == ~0)
            interactionLayers = GameLayers.InteractionMask;
    }

    private void Update()
    {
        if (itemHolder == null)
        {
            ClearCurrentHighlight();
            return;
        }

        UpdateHighlight();
        HandleHeldInteraction();
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

        IHoldInteractable holdInteractable = hit.collider.GetComponentInParent<IHoldInteractable>();

        if (holdInteractable != null)
        {
            BeginHeldInteraction(holdInteractable);
            return;
        }

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

        ObjetoSpawner objetoSpawner = hit.collider.GetComponentInParent<ObjetoSpawner>();

        if (objetoSpawner != null)
        {
            HandleSpawner(objetoSpawner);
            return;
        }

        ItemSpawner legacySpawner = hit.collider.GetComponentInParent<ItemSpawner>();

        if (legacySpawner != null)
        {
            HandleLegacySpawner(legacySpawner);
            return;
        }

        Objeto objeto = hit.collider.GetComponentInParent<Objeto>();

        if (objeto != null)
        {
            HandleObjeto(objeto);
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

    private GameObject GetHighlightRoot(Collider hitCollider)
    {
        ObjetoHomeSlot homeSlot = hitCollider.GetComponentInParent<ObjetoHomeSlot>();

        if (homeSlot != null)
        {
            Objeto heldObjeto = itemHolder != null ? itemHolder.CurrentObjeto : null;
            return homeSlot.ShouldHighlightFor(heldObjeto) ? homeSlot.gameObject : null;
        }

        Objeto objeto = hitCollider.GetComponentInParent<Objeto>();

        if (objeto != null)
            return objeto.gameObject;

        ObjetoSpawner spawner = hitCollider.GetComponentInParent<ObjetoSpawner>();

        if (spawner != null)
            return spawner.gameObject;

        ItemSpawner legacySpawner = hitCollider.GetComponentInParent<ItemSpawner>();

        if (legacySpawner != null)
            return legacySpawner.gameObject;

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

        Objeto currentItem = itemHolder.CurrentObjeto;

        return currentItem != null && targetCollider.transform.IsChildOf(currentItem.transform);
    }

    private bool TryInteractWithHeldPlate(IInteractable interactable)
    {
        if (itemHolder.IsEmpty())
            return false;

        PlateContainer heldPlate = itemHolder.CurrentObjeto.GetComponent<PlateContainer>();

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

    private void HandleSpawner(ObjetoSpawner spawner)
    {
        if (!itemHolder.IsEmpty())
            return;

        Objeto objeto = spawner.SpawnObjeto();

        if (objeto == null)
            return;

        if (!itemHolder.TryPickUp(objeto))
            Destroy(objeto.gameObject);
    }

    private void HandleLegacySpawner(ItemSpawner spawner)
    {
        if (!itemHolder.IsEmpty())
            return;

        Objeto item = spawner.SpawnObjeto();

        if (item == null)
            return;

        if (!itemHolder.TryPickUp(item))
            Destroy(item.gameObject);
    }

    private void HandleObjeto(Objeto item)
    {
        if (!itemHolder.IsEmpty())
        {
            TryPlateLooseItem(item);
            return;
        }

        itemHolder.TryPickUp(item);
    }

    private bool TryPlateLooseItem(Objeto item)
    {
        if (item == null || itemHolder.IsEmpty())
            return false;

        if (item == itemHolder.CurrentObjeto)
            return false;

        PlateContainer heldPlate = itemHolder.CurrentObjeto.GetComponent<PlateContainer>();

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

    private void HandleHeldInteraction()
    {
        if (currentHoldInteractable == null)
            return;

        if (input.InteractHeld)
        {
            currentHoldInteractable.UpdateHold(this);
            return;
        }

        currentHoldInteractable.EndHold(this);
        currentHoldInteractable = null;
    }

    private void BeginHeldInteraction(IHoldInteractable holdInteractable)
    {
        if (currentHoldInteractable != null && currentHoldInteractable != holdInteractable)
            currentHoldInteractable.EndHold(this);

        currentHoldInteractable = holdInteractable;
        currentHoldInteractable.BeginHold(this);
    }

    private void OnValidate()
    {
        interactionDistance = Mathf.Max(0f, interactionDistance);
        highlightRefreshInterval = Mathf.Max(0.01f, highlightRefreshInterval);
    }
}

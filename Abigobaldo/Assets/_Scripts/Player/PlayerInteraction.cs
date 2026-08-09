using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Holder holder;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers;
    [SerializeField] private float highlightRefreshInterval = 0.04f;

    private PlayerInputHandler input;
    private readonly RaycastHit[] interactionHits = new RaycastHit[16];
    private Highlightable currentHighlight;
    private IHoldInteractable currentHoldInteractable;
    private float nextHighlightRefreshTime;

    public Holder Holder => holder;
    public Camera PlayerCamera => playerCamera;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (holder == null)
            holder = GetComponentInChildren<Holder>();

        if (interactionLayers.value == 0 || interactionLayers.value == ~0)
            interactionLayers = GameLayers.InteractionMask;
    }

    private void Update()
    {
        if (holder == null)
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

        if (!holder.IsEmpty() && interactable != null)
        {
            interactable.Interact(this);
            return;
        }

        if (holder.IsEmpty() && interactable is BlenderCup blenderCup)
        {
            blenderCup.Interact(this);
            return;
        }

        if (TryHandleEmptyHandContainer(interactable))
            return;

        ObjectSpawner objectSpawner = hit.collider.GetComponentInParent<ObjectSpawner>();

        if (objectSpawner != null)
        {
            HandleSpawner(objectSpawner);
            return;
        }

        HoldableObject targetObject = hit.collider.GetComponentInParent<HoldableObject>();

        if (targetObject != null)
        {
            HandleObject(targetObject);
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

            if (IsCurrentHeldObjectCollider(hit.collider))
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
        ObjectReturnPoint returnPoint = hitCollider.GetComponentInParent<ObjectReturnPoint>();
        if (returnPoint != null)
        {
            HoldableObject heldObject = holder != null ? holder.CurrentObject : null;
            return returnPoint.ShouldHighlightFor(heldObject) ? returnPoint.gameObject : null;
        }

        Highlightable explicitHighlight = hitCollider.GetComponentInParent<Highlightable>();
        if (explicitHighlight != null)
            return explicitHighlight.gameObject;

        HoldableObject targetObject = hitCollider.GetComponentInParent<HoldableObject>();

        if (targetObject != null)
            return targetObject.gameObject;

        ObjectSpawner spawner = hitCollider.GetComponentInParent<ObjectSpawner>();

        if (spawner != null)
            return spawner.gameObject;

        IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();
        Component interactableComponent = interactable as Component;

        if (interactableComponent == null)
        {
            IHoldInteractable holdInteractable = hitCollider.GetComponentInParent<IHoldInteractable>();
            interactableComponent = holdInteractable as Component;
        }

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

    private bool IsCurrentHeldObjectCollider(Collider targetCollider)
    {
        if (targetCollider == null || holder == null || holder.IsEmpty())
            return false;

        HoldableObject currentObject = holder.CurrentObject;

        return currentObject != null && targetCollider.transform.IsChildOf(currentObject.transform);
    }

    private bool TryInteractWithHeldPlate(IInteractable interactable)
    {
        if (holder.IsEmpty())
            return false;

        PlateContainer heldPlate = holder.CurrentObject.GetComponent<PlateContainer>();

        if (heldPlate == null)
            return false;

        if (interactable is IRecipeStation station)
            return station.TryMoveOutputToPlate(heldPlate);

        return false;
    }

    private bool TryHandleEmptyHandContainer(IInteractable interactable)
    {
        if (!holder.IsEmpty())
            return false;

        IRecipeStation station = interactable as IRecipeStation;

        if (station == null)
            return false;

        if (station.HasReadyOutput)
        {
            station.Interact(this);
            return true;
        }

        if (station.TryPickUpContainer(holder))
            return true;

        station.Interact(this);
        return true;
    }

    private void HandleSpawner(ObjectSpawner spawner)
    {
        if (!holder.IsEmpty())
            return;

        spawner.SpawnObject(holder);
    }

    private void HandleObject(HoldableObject targetObject)
    {
        if (!holder.IsEmpty())
        {
            TryPlateLooseObject(targetObject);
            return;
        }

        holder.TryPickUp(targetObject);
    }

    private bool TryPlateLooseObject(HoldableObject targetObject)
    {
        if (targetObject == null || holder.IsEmpty())
            return false;

        if (targetObject == holder.CurrentObject)
            return false;

        PlateContainer heldPlate = holder.CurrentObject.GetComponent<PlateContainer>();

        if (heldPlate == null)
            return false;

        return heldPlate.TryAddLooseObject(targetObject);
    }

    private void HandleDrop()
    {
        if (!input.DropPressed)
            return;

        holder.DropItem();
    }

    private void HandleThrow()
    {
        if (!input.ThrowPressed)
            return;

        Vector3 throwDirection = playerCamera != null
            ? playerCamera.transform.forward
            : transform.forward;

        holder.ThrowItem(throwDirection);
    }

    private void HandleRotation()
    {
        if (!input.RotateHeld)
            return;

        if (holder.IsEmpty())
            return;

        if (playerCamera == null)
            return;

        holder.RotateItem(input.Look, playerCamera.transform);
    }

    private void HandleHoldZoom()
    {
        if (Mathf.Approximately(input.HoldZoom, 0f))
            return;

        holder.ZoomHeldItem(input.HoldZoom);
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


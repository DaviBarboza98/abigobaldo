using UnityEngine;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Holder holder;
        [SerializeField] private float interactionDistance = 4f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private float highlightRefreshInterval = 0.04f;

        private readonly RaycastHit[] hits = new RaycastHit[16];
        private PlayerInput input;
        private IHoldInteractable currentHoldInteractable;
        private OutlineHighlightable currentHighlight;
        private float nextHighlightRefreshTime;

        public Camera PlayerCamera => playerCamera;
        public Holder Holder => holder;

        private void Awake()
        {
            input = GetComponent<PlayerInput>();

            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            if (holder == null)
                holder = GetComponentInChildren<Holder>();
        }

        private void Update()
        {
            UpdateHighlight();
            HandleHeldInteraction();
            TryInteract();
            TryPick();
            TryBeginHoldFromHeldKey();
            TryDrop();
            TryThrow();
            TryZoomHeldObject();
            TryRotateHeldObject();
        }

        private void TryInteract()
        {
            if (!input.InteractPressed || playerCamera == null)
                return;

            if (!TryGetHit(out RaycastHit hit))
                return;

            if (TryHandleContainerInteraction(hit.collider))
                return;

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            interactable?.Interact(this);
        }

        private bool TryHandleContainerInteraction(Collider hitCollider)
        {
            if (holder == null || holder.IsEmpty || hitCollider == null)
                return false;

            HoldableObject heldObject = holder.CurrentObject;
            IObjectContainer heldContainer = heldObject.GetComponent<IObjectContainer>();
            IObjectContainer hitContainer = GetContainer(hitCollider);
            HoldableObject hitObject = hitCollider.GetComponentInParent<HoldableObject>();

            if (!IsPlate(heldContainer) && !IsPlate(hitContainer))
                return false;

            if (heldContainer != null && hitContainer != null)
            {
                if (heldContainer.HasContent && heldContainer.TryMoveLastObjectTo(hitContainer, this))
                {
                    TrySwitchHeldContainerToTarget(hitContainer);
                    return true;
                }

                if (hitContainer.HasContent && hitContainer.TryMoveLastObjectTo(heldContainer, this))
                    return true;

                return false;
            }

            if (hitContainer != null && hitObject != heldObject)
            {
                if (hitContainer.TryInsertObject(heldObject, this))
                {
                    TryPickUpContainerAfterInsert(hitContainer);
                    return true;
                }
            }

            if (heldContainer != null && hitObject != null && hitObject != heldObject)
                return heldContainer.TryInsertObject(hitObject, this);

            return false;
        }

        private static IObjectContainer GetContainer(Collider hitCollider)
        {
            return hitCollider != null ? hitCollider.GetComponentInParent<IObjectContainer>() : null;
        }

        private static bool IsPlate(IObjectContainer container)
        {
            return container is Plate;
        }

        private void TryPickUpContainerAfterInsert(IObjectContainer container)
        {
            if (container == null || holder == null || !holder.IsEmpty || container.Holdable == null)
                return;

            if (container.Holdable.CanBeHeld)
                holder.TryPickUp(container.Holdable);
        }

        private void TrySwitchHeldContainerToTarget(IObjectContainer targetContainer)
        {
            if (targetContainer == null || holder == null || targetContainer.Holdable == null || !targetContainer.Holdable.CanBeHeld)
                return;

            holder.Drop();
            holder.TryPickUp(targetContainer.Holdable);
        }

        private void TryPick()
        {
            if (!input.PickPressed || playerCamera == null)
                return;

            if (!TryGetHit(out RaycastHit hit))
                return;

            HoldableObject target = hit.collider.GetComponentInParent<HoldableObject>();

            if (target == null)
            {
                IPickupInteractable pickupInteractable = hit.collider.GetComponentInParent<IPickupInteractable>();

                if (pickupInteractable != null)
                {
                    pickupInteractable.PickInteract(this);
                    return;
                }

                IHoldInteractable holdInteractable = hit.collider.GetComponentInParent<IHoldInteractable>();

                if (holdInteractable != null)
                {
                    BeginHeldInteraction(holdInteractable);
                    return;
                }

                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

                if (interactable != null
                    && interactable is not ContainerStation
                    && interactable is not Plate)
                {
                    interactable.Interact(this);
                }

                return;
            }

            if (!target.CanBeHeld)
            {
                ContainerStation container = hit.collider.GetComponentInParent<ContainerStation>();
                container?.PickInteract(this);
                return;
            }

            if (holder != null && !holder.IsEmpty && target != holder.CurrentObject)
                holder.Drop();

            holder?.TryPickUp(target);
        }

        private void TryDrop()
        {
            if (input.DropPressed)
                holder?.Drop();
        }

        private void TryThrow()
        {
            if (!input.ThrowPressed)
                return;

            Vector3 direction = playerCamera != null ? playerCamera.transform.forward : transform.forward;
            holder?.Throw(direction);
        }

        private void TryZoomHeldObject()
        {
            if (!Mathf.Approximately(input.HoldZoom, 0f))
                holder?.Zoom(input.HoldZoom);
        }

        private void TryRotateHeldObject()
        {
            if (!input.RotateHeld || playerCamera == null)
                return;

            holder?.Rotate(input.Look, playerCamera.transform);
        }

        private bool TryGetHit(out RaycastHit bestHit)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            int hitCount = Physics.RaycastNonAlloc(ray, hits, interactionDistance, interactionLayers, QueryTriggerInteraction.Collide);

            bestHit = default;
            float bestDistance = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hits[i];

                if (IsHeldObjectCollider(hit.collider) || hit.distance >= bestDistance)
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

            if (playerCamera == null || !TryGetHit(out RaycastHit hit))
            {
                ClearHighlight();
                return;
            }

            OutlineHighlightable nextHighlight = GetHighlightable(hit.collider);

            if (nextHighlight == currentHighlight)
                return;

            ClearHighlight();
            currentHighlight = nextHighlight;

            if (currentHighlight != null)
                currentHighlight.SetHighlighted(true);
        }

        private OutlineHighlightable GetHighlightable(Collider hitCollider)
        {
            GameObject root = GetHighlightRoot(hitCollider);

            if (root == null)
                return null;

            OutlineHighlightable highlightable = root.GetComponent<OutlineHighlightable>();

            if (highlightable == null)
                highlightable = root.AddComponent<OutlineHighlightable>();

            return highlightable;
        }

        private GameObject GetHighlightRoot(Collider hitCollider)
        {
            if (hitCollider == null)
                return null;

            OutlineHighlightable explicitHighlight = hitCollider.GetComponentInParent<OutlineHighlightable>();

            if (explicitHighlight != null)
                return explicitHighlight.gameObject;

            HoldableObject holdableObject = hitCollider.GetComponentInParent<HoldableObject>();

            if (holdableObject != null)
                return holdableObject.gameObject;

            Component holdInteractable = hitCollider.GetComponentInParent<IHoldInteractable>() as Component;

            if (holdInteractable != null)
                return holdInteractable.gameObject;

            Component interactable = hitCollider.GetComponentInParent<IInteractable>() as Component;

            if (interactable != null)
                return interactable.gameObject;

            Component pickupInteractable = hitCollider.GetComponentInParent<IPickupInteractable>() as Component;

            if (pickupInteractable != null)
                return pickupInteractable.gameObject;

            return null;
        }

        private void ClearHighlight()
        {
            if (currentHighlight == null)
                return;

            currentHighlight.SetHighlighted(false);
            currentHighlight = null;
        }

        private bool IsHeldObjectCollider(Collider targetCollider)
        {
            if (targetCollider == null || holder == null || holder.IsEmpty)
                return false;

            HoldableObject currentObject = holder.CurrentObject;
            return currentObject != null && targetCollider.transform.IsChildOf(currentObject.transform);
        }

        private void HandleHeldInteraction()
        {
            if (currentHoldInteractable == null)
                return;

            if (input.PickHeld)
            {
                currentHoldInteractable.UpdateHold(this);
                return;
            }

            currentHoldInteractable.EndHold(this);
            currentHoldInteractable = null;
        }

        private void TryBeginHoldFromHeldKey()
        {
            if (currentHoldInteractable != null || !input.PickHeld || input.PickPressed)
                return;

            TryBeginHoldFromCurrentHit();
        }

        private bool TryBeginHoldFromCurrentHit()
        {
            if (playerCamera == null || !TryGetHit(out RaycastHit hit))
                return false;

            IHoldInteractable holdInteractable = hit.collider.GetComponentInParent<IHoldInteractable>();

            if (holdInteractable == null)
                return false;

            BeginHeldInteraction(holdInteractable);
            return true;
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
}

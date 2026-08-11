using UnityEngine;

namespace Abigobaldo.Demo
{
    [RequireComponent(typeof(DemoPlayerInput))]
    public class DemoPlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private DemoHolder holder;
        [SerializeField] private float interactionDistance = 4f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private float highlightRefreshInterval = 0.04f;

        private readonly RaycastHit[] hits = new RaycastHit[16];
        private DemoPlayerInput input;
        private IDemoHoldInteractable currentHoldInteractable;
        private DemoOutlineHighlightable currentHighlight;
        private float nextHighlightRefreshTime;

        public Camera PlayerCamera => playerCamera;
        public DemoHolder Holder => holder;

        private void Awake()
        {
            input = GetComponent<DemoPlayerInput>();

            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            if (holder == null)
                holder = GetComponentInChildren<DemoHolder>();
        }

        private void Update()
        {
            UpdateHighlight();
            HandleHeldInteraction();
            TryBeginHoldFromHeldKey();
            TryInteract();
            TryPick();
            TryDrop();
            TryThrow();
            TryZoomHeldObject();
            TryRotateHeldObject();
        }

        private void TryInteract()
        {
            if (!input.InteractPressed || playerCamera == null)
                return;

            if (TryBeginHoldFromCurrentHit())
                return;

            if (!TryGetHit(out RaycastHit hit))
                return;

            IDemoInteractable interactable = hit.collider.GetComponentInParent<IDemoInteractable>();
            interactable?.Interact(this);
        }

        private void TryPick()
        {
            if (!input.PickPressed || playerCamera == null)
                return;

            if (!TryGetHit(out RaycastHit hit))
                return;

            DemoHoldableObject target = hit.collider.GetComponentInParent<DemoHoldableObject>();

            if (target == null)
            {
                IDemoPickupInteractable pickupInteractable = hit.collider.GetComponentInParent<IDemoPickupInteractable>();
                pickupInteractable?.PickInteract(this);
                return;
            }

            if (!target.CanBeHeld)
                return;

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

            DemoOutlineHighlightable nextHighlight = GetHighlightable(hit.collider);

            if (nextHighlight == currentHighlight)
                return;

            ClearHighlight();
            currentHighlight = nextHighlight;

            if (currentHighlight != null)
                currentHighlight.SetHighlighted(true);
        }

        private DemoOutlineHighlightable GetHighlightable(Collider hitCollider)
        {
            GameObject root = GetHighlightRoot(hitCollider);

            if (root == null)
                return null;

            DemoOutlineHighlightable highlightable = root.GetComponent<DemoOutlineHighlightable>();

            if (highlightable == null)
                highlightable = root.AddComponent<DemoOutlineHighlightable>();

            return highlightable;
        }

        private GameObject GetHighlightRoot(Collider hitCollider)
        {
            if (hitCollider == null)
                return null;

            DemoOutlineHighlightable explicitHighlight = hitCollider.GetComponentInParent<DemoOutlineHighlightable>();

            if (explicitHighlight != null)
                return explicitHighlight.gameObject;

            DemoHoldableObject holdableObject = hitCollider.GetComponentInParent<DemoHoldableObject>();

            if (holdableObject != null)
                return holdableObject.gameObject;

            Component holdInteractable = hitCollider.GetComponentInParent<IDemoHoldInteractable>() as Component;

            if (holdInteractable != null)
                return holdInteractable.gameObject;

            Component interactable = hitCollider.GetComponentInParent<IDemoInteractable>() as Component;

            if (interactable != null)
                return interactable.gameObject;

            Component pickupInteractable = hitCollider.GetComponentInParent<IDemoPickupInteractable>() as Component;

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

            DemoHoldableObject currentObject = holder.CurrentObject;
            return currentObject != null && targetCollider.transform.IsChildOf(currentObject.transform);
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

        private void TryBeginHoldFromHeldKey()
        {
            if (currentHoldInteractable != null || !input.InteractHeld || input.InteractPressed)
                return;

            TryBeginHoldFromCurrentHit();
        }

        private bool TryBeginHoldFromCurrentHit()
        {
            if (playerCamera == null || !TryGetHit(out RaycastHit hit))
                return false;

            IDemoHoldInteractable holdInteractable = hit.collider.GetComponentInParent<IDemoHoldInteractable>();

            if (holdInteractable == null)
                return false;

            BeginHeldInteraction(holdInteractable);
            return true;
        }

        private void BeginHeldInteraction(IDemoHoldInteractable holdInteractable)
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

using UnityEngine;
using UnityEngine.Serialization;

namespace Abigobaldo.Game
{
    public class BlenderCupContent : MonoBehaviour, IInteractable, IObjectContainer
    {
        [FormerlySerializedAs("contentRoot")]
        [SerializeField] private Transform contentAnchor;
        [SerializeField] private BlenderStation station;

        private HoldableObject holdableObject;
        private Rigidbody body;
        private Transform homeParent;
        private Vector3 homeLocalPosition;
        private Quaternion homeLocalRotation;
        private Vector3 homeLocalScale;

        public HoldableObject HoldableObject => holdableObject;
        public HoldableObject Holdable => holdableObject;
        public Transform ContentAnchor => contentAnchor != null ? contentAnchor : transform;
        public Transform ContentRoot => ContentAnchor;
        public bool IsAttached => homeParent != null && transform.parent == homeParent;
        public bool HasContent => station != null && station.HasContent;
        public bool IsDirectInteractionTarget => true;

        private void Awake()
        {
            homeParent = transform.parent;
            homeLocalPosition = transform.localPosition;
            homeLocalRotation = transform.localRotation;
            homeLocalScale = transform.localScale;
            CacheReferences();
        }

        public void Interact(PlayerInteractor player)
        {
            CacheReferences();
            station?.InteractWithCup(player);
        }

        public bool TryAttachHome()
        {
            if (homeParent == null)
                return false;

            CacheReferences();
            holdableObject.PlaceOnDock(homeParent);
            transform.SetLocalPositionAndRotation(homeLocalPosition, homeLocalRotation);
            transform.localScale = homeLocalScale;
            return true;
        }

        public bool TryPlateInto(Plate plate)
        {
            CacheReferences();
            return station != null && station.TryPlateInto(plate);
        }

        public bool TryInsertObject(HoldableObject item, PlayerInteractor player)
        {
            CacheReferences();
            return station != null && station.TryInsertObject(item, player);
        }

        public bool TryTakeLastObject(PlayerInteractor player)
        {
            CacheReferences();
            return station != null && station.TryTakeLastObject(player);
        }

        public bool TryMoveLastObjectTo(IObjectContainer target, PlayerInteractor player)
        {
            CacheReferences();
            return station != null && station.TryMoveLastObjectTo(target, player);
        }

        private void SetAttachedPhysics()
        {
            if (body == null)
                return;

            if (!body.isKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = true;
        }

        private void CacheReferences()
        {
            if (holdableObject == null)
                holdableObject = GetComponent<HoldableObject>();

            if (body == null)
                body = GetComponent<Rigidbody>();

            if (station == null)
                station = GetComponentInParent<BlenderStation>();
        }

        private void OnValidate()
        {
            if (contentAnchor == null)
                contentAnchor = transform;
        }
    }
}

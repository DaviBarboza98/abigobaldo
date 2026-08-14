using UnityEngine;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(Rigidbody))]
    public class HoldableObject : MonoBehaviour
    {
        [SerializeField] private bool canBeHeld = true;
        [SerializeField] private bool canBeThrown = true;
        [SerializeField] private bool startAttached;
        [SerializeField] private Transform gripPoint;

        private Rigidbody body;
        private bool pickupLocked;
        private bool isHeld;
        private Quaternion savedHeldRotation = Quaternion.identity;
        private bool hasSavedHeldRotation;

        public Rigidbody Rigidbody => body;
        public bool CanBeHeld => canBeHeld && !pickupLocked;
        public bool CanBeThrown => canBeThrown;
        public Transform GripPoint => gripPoint;
        public bool IsHeld => isHeld;

        public bool TryGetSavedHeldRotation(out Quaternion rotation)
        {
            rotation = savedHeldRotation;
            return hasSavedHeldRotation;
        }

        public void SaveHeldRotation(Quaternion rotation)
        {
            savedHeldRotation = rotation;
            hasSavedHeldRotation = true;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            EnsureDynamicMeshCollidersAreConvex();

            if (startAttached)
                SetAttachedPhysics();
        }

        public void PickUp(Vector3 holderPosition, Quaternion holderRotation)
        {
            NotifyPickedUp();
            transform.SetParent(null);
            isHeld = true;
            pickupLocked = false;
            AlignToHolder(holderPosition, holderRotation);

            body.isKinematic = false;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.detectCollisions = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Drop()
        {
            transform.SetParent(null);
            isHeld = false;
            pickupLocked = false;
            body.isKinematic = false;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = true;
            body.detectCollisions = true;
            NotifyDropped();
        }

        public void Throw(Vector3 direction, float force)
        {
            Drop();
            body.AddForce(direction.normalized * force, ForceMode.Impulse);
        }

        public void SetPickupLocked(bool locked)
        {
            pickupLocked = locked;
        }

        public void PlaceInContainer(Transform anchor)
        {
            if (anchor != null)
            {
                transform.SetParent(anchor);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            isHeld = false;
            pickupLocked = true;
            SetAttachedPhysics();
            body.detectCollisions = false;
        }

        public void PlaceOnDock(Transform anchor)
        {
            if (anchor != null)
            {
                transform.SetParent(anchor);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            isHeld = false;
            pickupLocked = false;
            SetAttachedPhysics();
        }

        public void RemoveFromContainer()
        {
            transform.SetParent(null);
            isHeld = false;
            pickupLocked = false;
            body.detectCollisions = true;
            body.isKinematic = false;
        }

        private void AlignToHolder(Vector3 holderPosition, Quaternion holderRotation)
        {
            if (gripPoint == null)
            {
                transform.SetPositionAndRotation(holderPosition, holderRotation);
                return;
            }

            Quaternion targetRotation = holderRotation * Quaternion.Inverse(gripPoint.localRotation);
            transform.rotation = targetRotation;
            transform.position = holderPosition - (gripPoint.position - transform.position);
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

            body.isKinematic = true;
            body.useGravity = false;
            body.detectCollisions = true;
        }

        private void EnsureDynamicMeshCollidersAreConvex()
        {
            if (body == null || body.isKinematic)
                return;

            foreach (MeshCollider meshCollider in GetComponentsInChildren<MeshCollider>())
            {
                if (meshCollider != null && !meshCollider.convex)
                    meshCollider.convex = true;
            }
        }

        private void NotifyPickedUp()
        {
            foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is IHoldableLifecycle listener)
                    listener.OnPickedUp();
            }
        }

        private void NotifyDropped()
        {
            foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is IHoldableLifecycle listener)
                    listener.OnDropped();
            }
        }
    }
}

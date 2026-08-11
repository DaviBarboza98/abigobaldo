using UnityEngine;

namespace Abigobaldo.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    public class DemoHoldableObject : MonoBehaviour
    {
        [SerializeField] private bool canBeHeld = true;
        [SerializeField] private bool canBeThrown = true;
        [SerializeField] private bool startAttached;
        [SerializeField] private Transform gripPoint;

        private Rigidbody body;
        private bool pickupLocked;

        public Rigidbody Rigidbody => body;
        public bool CanBeHeld => canBeHeld && !pickupLocked;
        public bool CanBeThrown => canBeThrown;
        public Transform GripPoint => gripPoint;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            EnsureDynamicMeshCollidersAreConvex();

            if (startAttached)
                SetAttachedPhysics();
        }

        public void PickUp(Vector3 holderPosition, Quaternion holderRotation)
        {
            transform.SetParent(null);
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
            body.isKinematic = false;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = true;
            body.detectCollisions = true;
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

            SetAttachedPhysics();
        }

        public void RemoveFromContainer()
        {
            transform.SetParent(null);
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

            body.isKinematic = true;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
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
    }
}

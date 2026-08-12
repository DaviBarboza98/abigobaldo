using UnityEngine;

namespace Abigobaldo.Game
{
    public class Holder : MonoBehaviour
    {
        [SerializeField] private float throwForce = 8f;
        [SerializeField] private float positionFollowStrength = 18f;
        [SerializeField] private float maxFollowSpeed = 9f;
        [SerializeField] private float rotationFollowStrength = 18f;
        [SerializeField] private float maxAngularSpeed = 12f;
        [SerializeField] private float minDistanceOffset = -0.75f;
        [SerializeField] private float maxDistanceOffset = 1.25f;
        [SerializeField] private float zoomSpeed = 0.0015f;
        [SerializeField] private float rotationSensitivity = 0.25f;

        private HoldableObject currentObject;
        private Vector3 defaultLocalPosition;
        private Quaternion localTargetRotation = Quaternion.identity;
        private float distanceOffset;
        private Collider[] playerColliders;
        private Collider[] heldColliders;

        public HoldableObject CurrentObject => currentObject;
        public bool IsEmpty => currentObject == null;

        private void Awake()
        {
            defaultLocalPosition = transform.localPosition;
        }

        private void FixedUpdate()
        {
            FollowHeldObject();
        }

        public bool TryPickUp(HoldableObject target)
        {
            if (target == null || currentObject != null || !target.CanBeHeld)
                return false;

            currentObject = target;
            distanceOffset = 0f;
            localTargetRotation = Quaternion.identity;
            transform.localPosition = defaultLocalPosition;
            heldColliders = currentObject.GetComponentsInChildren<Collider>();

            currentObject.PickUp(transform.position, GetTargetRotation());
            SetPlayerCollisionIgnored(true);
            return true;
        }

        public bool Drop()
        {
            HoldableObject target = RemoveObject();

            if (target == null)
                return false;

            target.Drop();
            return true;
        }

        public bool Throw(Vector3 direction)
        {
            if (currentObject == null || !currentObject.CanBeThrown)
                return false;

            HoldableObject target = RemoveObject();
            target.Throw(direction, throwForce);
            return true;
        }

        public bool TryGetHeldIdentity(out ObjectIdentity identity)
        {
            identity = currentObject != null ? currentObject.GetComponent<ObjectIdentity>() : null;
            return identity != null;
        }

        public bool TryGetHeldComponent<T>(out T component) where T : Component
        {
            component = currentObject != null ? currentObject.GetComponent<T>() : null;
            return component != null;
        }

        public bool ConsumeHeldObject()
        {
            HoldableObject target = RemoveObject();

            if (target == null)
                return false;

            Destroy(target.gameObject);
            return true;
        }

        public HoldableObject ReleaseHeldObject()
        {
            return RemoveObject();
        }

        public bool ReplaceWith(HoldableObject prefab, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (prefab == null)
                return false;

            if (!IsEmpty)
                Drop();

            HoldableObject instance = Instantiate(prefab, spawnPosition, spawnRotation);
            instance.name = prefab.name;
            return TryPickUp(instance);
        }

        public bool Zoom(float scrollDelta)
        {
            if (currentObject == null)
                return false;

            distanceOffset = Mathf.Clamp(distanceOffset + scrollDelta * zoomSpeed, minDistanceOffset, maxDistanceOffset);
            transform.localPosition = defaultLocalPosition + Vector3.forward * distanceOffset;
            return true;
        }

        public bool Rotate(Vector2 mouseDelta, Transform cameraTransform)
        {
            if (currentObject == null || cameraTransform == null)
                return false;

            Quaternion targetRotation = GetTargetRotation();
            Quaternion yaw = Quaternion.AngleAxis(mouseDelta.x * rotationSensitivity, cameraTransform.up);
            Quaternion pitch = Quaternion.AngleAxis(-mouseDelta.y * rotationSensitivity, cameraTransform.right);
            localTargetRotation = Quaternion.Inverse(transform.rotation) * (yaw * pitch * targetRotation);

            CookableItem cookable = currentObject.GetComponent<CookableItem>();
            HoldableObject mixedPrefab = cookable != null ? cookable.AddHandMix(mouseDelta.magnitude) : null;

            if (mixedPrefab != null)
                TransformHeldInto(mixedPrefab);

            return true;
        }

        public bool TransformHeldInto(HoldableObject prefab)
        {
            if (prefab == null || currentObject == null)
                return false;

            Vector3 position = currentObject.transform.position;
            Quaternion rotation = currentObject.transform.rotation;
            SetPlayerCollisionIgnored(false);
            Destroy(currentObject.gameObject);
            currentObject = null;
            heldColliders = null;

            HoldableObject instance = Instantiate(prefab, position, rotation);
            instance.name = prefab.name;
            return TryPickUp(instance);
        }

        private HoldableObject RemoveObject()
        {
            if (currentObject == null)
                return null;

            SetPlayerCollisionIgnored(false);
            HoldableObject target = currentObject;
            currentObject = null;
            heldColliders = null;
            return target;
        }

        private void FollowHeldObject()
        {
            if (currentObject == null || currentObject.Rigidbody == null)
                return;

            Rigidbody body = currentObject.Rigidbody;
            Vector3 toTarget = transform.position - GetGripWorldPosition();
            body.velocity = Vector3.ClampMagnitude(toTarget * positionFollowStrength, maxFollowSpeed);
            body.angularVelocity = GetTargetAngularVelocity(body.rotation);
        }

        private Vector3 GetGripWorldPosition()
        {
            return currentObject.GripPoint != null ? currentObject.GripPoint.position : currentObject.Rigidbody.position;
        }

        private Vector3 GetTargetAngularVelocity(Quaternion currentRotation)
        {
            Quaternion delta = GetTargetRotation() * Quaternion.Inverse(currentRotation);
            delta.ToAngleAxis(out float angle, out Vector3 axis);

            if (angle > 180f)
                angle -= 360f;

            if (axis == Vector3.zero)
                return Vector3.zero;

            return Vector3.ClampMagnitude(axis.normalized * (angle * Mathf.Deg2Rad * rotationFollowStrength), maxAngularSpeed);
        }

        private Quaternion GetTargetRotation()
        {
            return transform.rotation * localTargetRotation;
        }

        private void SetPlayerCollisionIgnored(bool ignored)
        {
            if (currentObject == null)
                return;

            if (playerColliders == null || playerColliders.Length == 0)
                playerColliders = transform.root.GetComponentsInChildren<Collider>();

            if (heldColliders == null || heldColliders.Length == 0)
                heldColliders = currentObject.GetComponentsInChildren<Collider>();

            foreach (Collider heldCollider in heldColliders)
            {
                if (heldCollider == null)
                    continue;

                foreach (Collider playerCollider in playerColliders)
                {
                    if (playerCollider != null && playerCollider != heldCollider)
                        Physics.IgnoreCollision(heldCollider, playerCollider, ignored);
                }
            }
        }

        private void OnValidate()
        {
            throwForce = Mathf.Max(0f, throwForce);
            positionFollowStrength = Mathf.Max(0f, positionFollowStrength);
            maxFollowSpeed = Mathf.Max(0.01f, maxFollowSpeed);
            rotationFollowStrength = Mathf.Max(0f, rotationFollowStrength);
            maxAngularSpeed = Mathf.Max(0.01f, maxAngularSpeed);
            minDistanceOffset = Mathf.Min(minDistanceOffset, 0f);
            maxDistanceOffset = Mathf.Max(maxDistanceOffset, 0f);
            zoomSpeed = Mathf.Max(0f, zoomSpeed);
            rotationSensitivity = Mathf.Max(0f, rotationSensitivity);
        }
    }
}

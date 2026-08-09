using UnityEngine;

public class Holder : MonoBehaviour
{
    [Header("Drop And Throw")]
    [SerializeField] private float throwForce = 8f;

    [Header("Physics Follow")]
    [SerializeField] private float positionFollowStrength = 18f;
    [SerializeField] private float maxFollowSpeed = 9f;
    [SerializeField] private float rotationFollowStrength = 18f;
    [SerializeField] private float maxAngularSpeed = 12f;
    [SerializeField] private float collisionSkin = 0.03f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private int depenetrationIterations = 3;
    [SerializeField] private float overlapCheckInterval = 0.12f;

    [Header("Object Distance")]
    [SerializeField] private float minDistanceOffset = -0.75f;
    [SerializeField] private float maxDistanceOffset = 1.25f;
    [SerializeField] private float zoomSpeed = 0.0015f;

    [Header("Object Rotation")]
    [SerializeField] private float objectRotationSensitivity = 0.25f;

    private HoldableObject currentObject;
    private Vector3 defaultLocalPosition;
    private float distanceOffset;
    private Quaternion localTargetRotation = Quaternion.identity;
    private Collider[] playerColliders;
    private Collider[] heldObjectColliders;
    private readonly Collider[] overlapBuffer = new Collider[24];
    private float nextOverlapCheckTime;
    private float heldObjectBoundsRadius = 0.5f;

    public HoldableObject CurrentObject => currentObject;

    private void FixedUpdate()
    {
        if (collisionLayers.value == 0)
            collisionLayers = GameLayers.PhysicsObjectCollisionMask;

        FollowHeldObject();
    }

    private void Awake()
    {
        defaultLocalPosition = transform.localPosition;
    }

    public bool IsEmpty()
    {
        return currentObject == null;
    }

    public bool TryPickUp(HoldableObject targetObject)
    {
        if (targetObject == null)
            return false;

        if (currentObject != null)
            return false;

        if (!targetObject.CanBeHeld)
            return false;

        currentObject = targetObject;
        distanceOffset = 0f;
        localTargetRotation = Quaternion.identity;
        transform.localPosition = defaultLocalPosition;
        heldObjectColliders = currentObject.GetComponentsInChildren<Collider>();
        heldObjectBoundsRadius = CalculateHeldObjectBoundsRadius();
        nextOverlapCheckTime = Time.fixedTime;

        currentObject.PickUp(transform.position, GetTargetRotation());
        SetPlayerCollisionIgnored(true);

        return true;
    }

    public HoldableObject RemoveObject()
    {
        if (currentObject == null)
            return null;

        SetPlayerCollisionIgnored(false);

        HoldableObject targetObject = currentObject;
        currentObject = null;
        heldObjectColliders = null;

        return targetObject;
    }

    public bool RotateItem(Vector2 mouseDelta, Transform cameraTransform)
    {
        if (currentObject == null)
            return false;

        if (cameraTransform == null)
            return false;

        float rotationX = -mouseDelta.y * objectRotationSensitivity;
        float rotationY = mouseDelta.x * objectRotationSensitivity;

        Quaternion targetRotation = GetTargetRotation();
        Quaternion yaw = Quaternion.AngleAxis(rotationY, cameraTransform.up);
        Quaternion pitch = Quaternion.AngleAxis(rotationX, cameraTransform.right);

        targetRotation = yaw * pitch * targetRotation;
        localTargetRotation = Quaternion.Inverse(transform.rotation) * targetRotation;

        return true;
    }

    public bool ZoomHeldItem(float scrollDelta)
    {
        if (currentObject == null)
            return false;

        distanceOffset = Mathf.Clamp(
            distanceOffset + scrollDelta * zoomSpeed,
            minDistanceOffset,
            maxDistanceOffset
        );

        transform.localPosition = defaultLocalPosition + Vector3.forward * distanceOffset;
        return true;
    }

    public bool DropItem()
    {
        HoldableObject targetObject = RemoveObject();

        if (targetObject == null)
            return false;

        targetObject.Drop();
        return true;
    }

    public bool ThrowItem()
    {
        return ThrowItem(transform.forward);
    }

    public bool ThrowItem(Vector3 direction)
    {
        if (currentObject == null)
            return false;

        if (!currentObject.CanBeThrown)
            return false;

        HoldableObject targetObject = RemoveObject();
        targetObject.Throw(direction, throwForce);

        return true;
    }

    private void FollowHeldObject()
    {
        if (currentObject == null)
            return;

        Rigidbody objectBody = currentObject.Rigidbody;

        if (objectBody == null)
            return;

        TryResolveOverlaps(objectBody);

        Vector3 toTarget = transform.position - objectBody.position;
        Vector3 targetVelocity = Vector3.ClampMagnitude(
            toTarget * positionFollowStrength,
            maxFollowSpeed
        );

        objectBody.velocity = GetBlockedVelocity(objectBody, targetVelocity);
        objectBody.angularVelocity = GetTargetAngularVelocity(objectBody.rotation);

        if (targetVelocity == Vector3.zero)
            TryResolveOverlaps(objectBody);
    }

    private Vector3 GetBlockedVelocity(Rigidbody itemBody, Vector3 targetVelocity)
    {
        float speed = targetVelocity.magnitude;

        if (speed <= Mathf.Epsilon)
            return Vector3.zero;

        Vector3 direction = targetVelocity / speed;
        float moveDistance = speed * Time.fixedDeltaTime;

        if (!itemBody.SweepTest(
            direction,
            out RaycastHit hit,
            moveDistance + collisionSkin,
            QueryTriggerInteraction.Ignore
        ))
        {
            return targetVelocity;
        }

        if (!CanBlockAgainst(hit.collider))
            return targetVelocity;

        return Vector3.zero;
    }

    private void TryResolveOverlaps(Rigidbody itemBody)
    {
        if (Time.fixedTime < nextOverlapCheckTime)
            return;

        ResolveOverlaps(itemBody);
        nextOverlapCheckTime = Time.fixedTime + overlapCheckInterval;
    }

    private void ResolveOverlaps(Rigidbody itemBody)
    {
        if (heldObjectColliders == null || heldObjectColliders.Length == 0)
        {
            heldObjectColliders = currentObject.GetComponentsInChildren<Collider>();
            heldObjectBoundsRadius = CalculateHeldObjectBoundsRadius();
        }

        for (int iteration = 0; iteration < depenetrationIterations; iteration++)
        {
            bool solvedAnyOverlap = false;
            int overlapCount = Physics.OverlapSphereNonAlloc(
                itemBody.worldCenterOfMass,
                heldObjectBoundsRadius + collisionSkin,
                overlapBuffer,
                collisionLayers,
                QueryTriggerInteraction.Ignore
            );

            foreach (Collider objectCollider in heldObjectColliders)
            {
                if (objectCollider == null || !objectCollider.enabled)
                    continue;

                for (int i = 0; i < overlapCount; i++)
                {
                    Collider other = overlapBuffer[i];

                    if (!CanSeparateFrom(objectCollider, other))
                        continue;

                    if (!Physics.ComputePenetration(
                        objectCollider,
                        objectCollider.transform.position,
                        objectCollider.transform.rotation,
                        other,
                        other.transform.position,
                        other.transform.rotation,
                        out Vector3 direction,
                        out float distance
                    ))
                    {
                        continue;
                    }

                    itemBody.position += direction * (distance + collisionSkin);
                    itemBody.velocity = Vector3.zero;
                    itemBody.angularVelocity = Vector3.zero;
                    solvedAnyOverlap = true;
                }
            }

            if (!solvedAnyOverlap)
                return;
        }
    }

    private bool CanBlockAgainst(Collider other)
    {
        if (other == null || !other.enabled)
            return false;

        if (currentObject != null && other.transform.IsChildOf(currentObject.transform))
            return false;

        if (other.transform.IsChildOf(transform.root))
            return false;

        return true;
    }

    private float CalculateHeldObjectBoundsRadius()
    {
        if (heldObjectColliders == null || heldObjectColliders.Length == 0)
            return 0.5f;

        Bounds bounds = heldObjectColliders[0].bounds;

        for (int i = 1; i < heldObjectColliders.Length; i++)
        {
            Collider objectCollider = heldObjectColliders[i];

            if (objectCollider != null && objectCollider.enabled)
                bounds.Encapsulate(objectCollider.bounds);
        }

        return Mathf.Max(0.1f, bounds.extents.magnitude);
    }

    private bool CanSeparateFrom(Collider objectCollider, Collider other)
    {
        if (!CanBlockAgainst(other))
            return false;

        return other != objectCollider;
    }

    private Vector3 GetTargetAngularVelocity(Quaternion currentRotation)
    {
        Quaternion delta = GetTargetRotation() * Quaternion.Inverse(currentRotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f)
            angle -= 360f;

        if (axis == Vector3.zero)
            return Vector3.zero;

        Vector3 angularVelocity = axis.normalized * (angle * Mathf.Deg2Rad * rotationFollowStrength);
        return Vector3.ClampMagnitude(angularVelocity, maxAngularSpeed);
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

        if (heldObjectColliders == null || heldObjectColliders.Length == 0)
            heldObjectColliders = currentObject.GetComponentsInChildren<Collider>();

        foreach (Collider objectCollider in heldObjectColliders)
        {
            if (objectCollider == null)
                continue;

            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null)
                    continue;

                if (objectCollider == playerCollider)
                    continue;

                Physics.IgnoreCollision(objectCollider, playerCollider, ignored);
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
        collisionSkin = Mathf.Max(0f, collisionSkin);
        depenetrationIterations = Mathf.Max(1, depenetrationIterations);
        overlapCheckInterval = Mathf.Max(0.02f, overlapCheckInterval);
        minDistanceOffset = Mathf.Min(minDistanceOffset, 0f);
        maxDistanceOffset = Mathf.Max(maxDistanceOffset, 0f);
        zoomSpeed = Mathf.Max(0f, zoomSpeed);
        objectRotationSensitivity = Mathf.Max(0f, objectRotationSensitivity);
    }
}



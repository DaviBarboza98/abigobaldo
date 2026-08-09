using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    [Header("Largar e arremessar")]
    [SerializeField] private float throwForce = 8f;

    [Header("Seguimento fisico")]
    [SerializeField] private float positionFollowStrength = 18f;
    [SerializeField] private float maxFollowSpeed = 9f;
    [SerializeField] private float rotationFollowStrength = 18f;
    [SerializeField] private float maxAngularSpeed = 12f;
    [SerializeField] private float collisionSkin = 0.03f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private int depenetrationIterations = 3;
    [SerializeField] private float overlapCheckInterval = 0.12f;

    [Header("Distancia do item")]
    [SerializeField] private float minDistanceOffset = -0.75f;
    [SerializeField] private float maxDistanceOffset = 1.25f;
    [SerializeField] private float zoomSpeed = 0.0015f;

    [Header("Rotacao do item")]
    [SerializeField] private float itemRotationSensitivity = 0.25f;

    private Objeto currentObjeto;
    private Vector3 defaultLocalPosition;
    private float distanceOffset;
    private Quaternion localTargetRotation = Quaternion.identity;
    private Collider[] playerColliders;
    private Collider[] heldItemColliders;
    private readonly Collider[] overlapBuffer = new Collider[24];
    private float nextOverlapCheckTime;
    private float heldItemBoundsRadius = 0.5f;

    public Objeto CurrentObjeto => currentObjeto;
    public Item CurrentItem => currentObjeto as Item;

    private void FixedUpdate()
    {
        if (collisionLayers.value == 0)
            collisionLayers = GameLayers.PhysicsObjectCollisionMask;

        FollowHeldItem();
    }

    private void Awake()
    {
        defaultLocalPosition = transform.localPosition;
    }

    public bool IsEmpty()
    {
        return currentObjeto == null;
    }

    public bool TryPickUp(Item item)
    {
        return TryPickUp(item as Objeto);
    }

    public bool TryPickUp(Objeto item)
    {
        if (item == null)
            return false;

        if (currentObjeto != null)
            return false;

        if (!item.CanBeHeld)
            return false;

        currentObjeto = item;
        distanceOffset = 0f;
        localTargetRotation = Quaternion.identity;
        transform.localPosition = defaultLocalPosition;
        heldItemColliders = currentObjeto.GetComponentsInChildren<Collider>();
        heldItemBoundsRadius = CalculateHeldItemBoundsRadius();
        nextOverlapCheckTime = Time.fixedTime;

        currentObjeto.HomeSlot?.MarkPickedUp(currentObjeto);
        currentObjeto.PickUp(transform.position, GetTargetRotation());
        SetPlayerCollisionIgnored(true);

        return true;
    }

    public Objeto RemoveObjeto()
    {
        if (currentObjeto == null)
            return null;

        SetPlayerCollisionIgnored(false);

        Objeto item = currentObjeto;
        currentObjeto = null;
        heldItemColliders = null;

        return item;
    }

    public Item RemoveItem()
    {
        return RemoveObjeto() as Item;
    }

    public bool RotateItem(Vector2 mouseDelta, Transform cameraTransform)
    {
        if (currentObjeto == null)
            return false;

        if (cameraTransform == null)
            return false;

        float rotationX = -mouseDelta.y * itemRotationSensitivity;
        float rotationY = mouseDelta.x * itemRotationSensitivity;

        Quaternion targetRotation = GetTargetRotation();
        Quaternion yaw = Quaternion.AngleAxis(rotationY, cameraTransform.up);
        Quaternion pitch = Quaternion.AngleAxis(rotationX, cameraTransform.right);

        targetRotation = yaw * pitch * targetRotation;
        localTargetRotation = Quaternion.Inverse(transform.rotation) * targetRotation;

        return true;
    }

    public bool ZoomHeldItem(float scrollDelta)
    {
        if (currentObjeto == null)
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
        Objeto item = RemoveObjeto();

        if (item == null)
            return false;

        item.Drop();
        return true;
    }

    public bool ThrowItem()
    {
        return ThrowItem(transform.forward);
    }

    public bool ThrowItem(Vector3 direction)
    {
        if (currentObjeto == null)
            return false;

        if (!currentObjeto.CanBeThrown)
            return false;

        Objeto item = RemoveObjeto();
        item.Throw(direction, throwForce);

        return true;
    }

    private void FollowHeldItem()
    {
        if (currentObjeto == null)
            return;

        Rigidbody itemBody = currentObjeto.Rigidbody;

        if (itemBody == null)
            return;

        TryResolveOverlaps(itemBody);

        Vector3 toTarget = transform.position - itemBody.position;
        Vector3 targetVelocity = Vector3.ClampMagnitude(
            toTarget * positionFollowStrength,
            maxFollowSpeed
        );

        itemBody.velocity = GetBlockedVelocity(itemBody, targetVelocity);
        itemBody.angularVelocity = GetTargetAngularVelocity(itemBody.rotation);

        if (targetVelocity == Vector3.zero)
            TryResolveOverlaps(itemBody);
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
        if (heldItemColliders == null || heldItemColliders.Length == 0)
        {
            heldItemColliders = currentObjeto.GetComponentsInChildren<Collider>();
            heldItemBoundsRadius = CalculateHeldItemBoundsRadius();
        }

        for (int iteration = 0; iteration < depenetrationIterations; iteration++)
        {
            bool solvedAnyOverlap = false;
            int overlapCount = Physics.OverlapSphereNonAlloc(
                itemBody.worldCenterOfMass,
                heldItemBoundsRadius + collisionSkin,
                overlapBuffer,
                collisionLayers,
                QueryTriggerInteraction.Ignore
            );

            foreach (Collider itemCollider in heldItemColliders)
            {
                if (itemCollider == null || !itemCollider.enabled)
                    continue;

                for (int i = 0; i < overlapCount; i++)
                {
                    Collider other = overlapBuffer[i];

                    if (!CanSeparateFrom(itemCollider, other))
                        continue;

                    if (!Physics.ComputePenetration(
                        itemCollider,
                        itemCollider.transform.position,
                        itemCollider.transform.rotation,
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

        if (currentObjeto != null && other.transform.IsChildOf(currentObjeto.transform))
            return false;

        if (other.transform.IsChildOf(transform.root))
            return false;

        return true;
    }

    private float CalculateHeldItemBoundsRadius()
    {
        if (heldItemColliders == null || heldItemColliders.Length == 0)
            return 0.5f;

        Bounds bounds = heldItemColliders[0].bounds;

        for (int i = 1; i < heldItemColliders.Length; i++)
        {
            Collider itemCollider = heldItemColliders[i];

            if (itemCollider != null && itemCollider.enabled)
                bounds.Encapsulate(itemCollider.bounds);
        }

        return Mathf.Max(0.1f, bounds.extents.magnitude);
    }

    private bool CanSeparateFrom(Collider itemCollider, Collider other)
    {
        if (!CanBlockAgainst(other))
            return false;

        return other != itemCollider;
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
        if (currentObjeto == null)
            return;

        if (playerColliders == null || playerColliders.Length == 0)
            playerColliders = transform.root.GetComponentsInChildren<Collider>();

        if (heldItemColliders == null || heldItemColliders.Length == 0)
            heldItemColliders = currentObjeto.GetComponentsInChildren<Collider>();

        foreach (Collider itemCollider in heldItemColliders)
        {
            if (itemCollider == null)
                continue;

            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null)
                    continue;

                if (itemCollider == playerCollider)
                    continue;

                Physics.IgnoreCollision(itemCollider, playerCollider, ignored);
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
        itemRotationSensitivity = Mathf.Max(0f, itemRotationSensitivity);
    }
}

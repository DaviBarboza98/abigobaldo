using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoldableObject : MonoBehaviour
{
    public enum ObjectRole
    {
        Common,
        Ingredient,
        Tool,
        Container,
        BlenderCup,
        Plate
    }

    [Header("Data")]
    [SerializeField] private ObjectData objectData;

    [Header("Classification")]
    [SerializeField] private ObjectRole role = ObjectRole.Common;

    [Header("Properties")]
    [SerializeField] private bool canBeHeld = true;
    [SerializeField] private bool canBeThrown = true;

    private Rigidbody rb;
    private MonoBehaviour[] holdStateBehaviours;
    private bool hasRuntimeCookState;
    private ObjectCookState runtimeCookState;
    private bool hasRuntimeTint;
    private Color runtimeTint;
    private Material runtimeMaterial;

    public ObjectData Data => objectData;
    public ObjectRole Role => role;
    public Rigidbody Rigidbody => rb;
    public string ObjectName => objectData != null ? objectData.DisplayName : gameObject.name;
    public bool CanBeHeld => canBeHeld;
    public bool CanBeThrown => canBeThrown;
    public ObjectCookState CookState => hasRuntimeCookState ? runtimeCookState : objectData != null ? objectData.CookState : ObjectCookState.Raw;
    public bool HasRuntimeTint => hasRuntimeTint;
    public Color RuntimeTint => runtimeTint;
    public Material RuntimeMaterial => runtimeMaterial;

    protected virtual void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.HoldableObject);

        rb = GetComponent<Rigidbody>();
        EnsureDynamicMeshCollidersAreConvex();
        holdStateBehaviours = GetComponents<MonoBehaviour>();
    }

    public void Configure(ObjectData data, bool held, bool thrown)
    {
        objectData = data;
        canBeHeld = held;
        canBeThrown = thrown;
    }

    public void SetRuntimeCookVisual(ObjectCookState state, Color? tint)
    {
        SetRuntimeCookVisual(state, tint, null);
    }

    public void SetRuntimeCookVisual(ObjectCookState state, Color? tint, Material material)
    {
        hasRuntimeCookState = true;
        runtimeCookState = state;
        hasRuntimeTint = tint.HasValue;
        runtimeMaterial = material;

        if (runtimeMaterial != null)
        {
            ApplyMaterial(runtimeMaterial);
            return;
        }

        if (!tint.HasValue)
            return;

        runtimeTint = tint.Value;
        ApplyTint(runtimeTint);
    }

    public virtual void PickUp(Vector3 position, Quaternion rotation)
    {
        transform.SetParent(null);
        transform.SetPositionAndRotation(position, rotation);

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.detectCollisions = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        NotifyPickedUp();
    }

    public virtual void Drop()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;

        transform.SetParent(null);
        NotifyDropped();
    }

    public virtual void Throw(Vector3 direction, float force)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;

        transform.SetParent(null);
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        NotifyThrown();
    }

    public void PlaceAt(Transform anchor)
    {
        if (anchor == null)
            return;

        transform.SetParent(null);
        transform.SetPositionAndRotation(anchor.position, anchor.rotation);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.detectCollisions = true;
        NotifyDropped();
    }

    private void NotifyPickedUp()
    {
        RefreshReceivers();

        foreach (MonoBehaviour behaviour in holdStateBehaviours)
        {
            HoldStateReceiver receiver = behaviour as HoldStateReceiver;
            receiver?.OnPickedUp();
        }
    }

    private void NotifyDropped()
    {
        RefreshReceivers();

        foreach (MonoBehaviour behaviour in holdStateBehaviours)
        {
            HoldStateReceiver receiver = behaviour as HoldStateReceiver;
            receiver?.OnDropped();
        }
    }

    private void NotifyThrown()
    {
        RefreshReceivers();

        foreach (MonoBehaviour behaviour in holdStateBehaviours)
        {
            HoldStateReceiver receiver = behaviour as HoldStateReceiver;
            receiver?.OnThrown();
        }
    }

    private void RefreshReceivers()
    {
        if (holdStateBehaviours == null || holdStateBehaviours.Length == 0)
            holdStateBehaviours = GetComponents<MonoBehaviour>();
    }

    private void ApplyTint(Color tint)
    {
        foreach (Renderer targetRenderer in GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in targetRenderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", tint);
                else if (material.HasProperty("_Color"))
                    material.color = tint;
            }
        }
    }

    private void ApplyMaterial(Material material)
    {
        if (material == null)
            return;

        foreach (Renderer targetRenderer in GetComponentsInChildren<Renderer>())
            targetRenderer.material = material;
    }

    private void EnsureDynamicMeshCollidersAreConvex()
    {
        if (rb == null || rb.isKinematic)
            return;

        foreach (MeshCollider meshCollider in GetComponentsInChildren<MeshCollider>())
        {
            if (meshCollider == null || meshCollider.convex)
                continue;

            meshCollider.convex = true;
        }
    }
}


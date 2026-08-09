using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Objeto : MonoBehaviour
{
    public enum ObjetoRole
    {
        Comum,
        Ingrediente,
        Ferramenta,
        Container,
        CopoLiquidificador,
        Prato
    }

    [Header("Dados")]
    [SerializeField] private ItemData objetoData;

    [Header("Classificacao")]
    [SerializeField] private ObjetoRole role = ObjetoRole.Comum;

    [Header("Propriedades")]
    [SerializeField] private bool canBeHeld = true;
    [SerializeField] private bool canBeThrown = true;

    private Rigidbody rb;
    private MonoBehaviour[] holdStateBehaviours;

    public ItemData Data => objetoData;
    public ObjetoRole Role => role;
    public Rigidbody Rigidbody => rb;
    public string ObjetoName => objetoData != null ? objetoData.DisplayName : gameObject.name;
    public bool CanBeHeld => canBeHeld;
    public bool CanBeThrown => canBeThrown;

    protected virtual void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Objeto);

        rb = GetComponent<Rigidbody>();
        EnsureDynamicMeshCollidersAreConvex();
        holdStateBehaviours = GetComponents<MonoBehaviour>();
    }

    public void Configure(ItemData data, bool held, bool thrown)
    {
        objetoData = data;
        canBeHeld = held;
        canBeThrown = thrown;
    }

    public virtual void PickUp(Vector3 position, Quaternion rotation)
    {
        transform.SetParent(null);
        transform.SetPositionAndRotation(position, rotation);

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
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

        transform.SetParent(null);
        NotifyDropped();
    }

    public virtual void Throw(Vector3 direction, float force)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;

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
        NotifyDropped();
    }

    private void NotifyPickedUp()
    {
        RefreshReceivers();

        foreach (MonoBehaviour behaviour in holdStateBehaviours)
        {
            ItemHoldStateReceiver receiver = behaviour as ItemHoldStateReceiver;
            receiver?.OnPickedUp();
        }
    }

    private void NotifyDropped()
    {
        RefreshReceivers();

        foreach (MonoBehaviour behaviour in holdStateBehaviours)
        {
            ItemHoldStateReceiver receiver = behaviour as ItemHoldStateReceiver;
            receiver?.OnDropped();
        }
    }

    private void NotifyThrown()
    {
        RefreshReceivers();

        foreach (MonoBehaviour behaviour in holdStateBehaviours)
        {
            ItemHoldStateReceiver receiver = behaviour as ItemHoldStateReceiver;
            receiver?.OnThrown();
        }
    }

    private void RefreshReceivers()
    {
        if (holdStateBehaviours == null || holdStateBehaviours.Length == 0)
            holdStateBehaviours = GetComponents<MonoBehaviour>();
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

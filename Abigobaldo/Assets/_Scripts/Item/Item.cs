using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    [Header("Dados")]
    [SerializeField] private ItemData itemData;

    [Header("Propriedades")]
    [SerializeField] private bool canBeHeld = true;
    [SerializeField] private bool canBeThrown = true;

    private Rigidbody rb;
    private MonoBehaviour[] holdStateBehaviours;

    public ItemData Data => itemData;
    public Rigidbody Rigidbody => rb;
    public string ItemName => itemData != null ? itemData.DisplayName : gameObject.name;
    public bool CanBeHeld => canBeHeld;
    public bool CanBeThrown => canBeThrown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        holdStateBehaviours = GetComponents<MonoBehaviour>();
    }

    public void Configure(ItemData data, bool held, bool thrown)
    {
        itemData = data;
        canBeHeld = held;
        canBeThrown = thrown;
    }

    public void PickUp(Vector3 position, Quaternion rotation)
    {
        transform.SetParent(null);
        transform.SetPositionAndRotation(position, rotation);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        NotifyPickedUp();
    }

    public void Drop()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;

        transform.SetParent(null);
        NotifyDropped();
    }

    public void Throw(Vector3 direction, float force)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;

        transform.SetParent(null);
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        NotifyThrown();
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
}

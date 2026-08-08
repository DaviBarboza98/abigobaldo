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

    public ItemData Data => itemData;
    public Rigidbody Rigidbody => rb;
    public string ItemName => itemData != null ? itemData.DisplayName : gameObject.name;
    public bool CanBeHeld => canBeHeld;
    public bool CanBeThrown => canBeThrown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
    }

    public void Drop()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;

        transform.SetParent(null);
    }

    public void Throw(Vector3 direction, float force)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;

        transform.SetParent(null);
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }
}

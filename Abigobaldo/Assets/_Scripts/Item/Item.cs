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

    public string ItemName =>
        itemData != null
            ? itemData.DisplayName
            : gameObject.name;

    public bool CanBeHeld => canBeHeld;
    public bool CanBeThrown => canBeThrown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform holder)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;

        transform.SetParent(holder);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop(Vector3 position)
    {
        transform.SetParent(null);
        transform.position = position;

        rb.isKinematic = false;
        rb.useGravity = true;
    }

    public void Throw(
        Vector3 position,
        Vector3 direction,
        float force
    )
    {
        transform.SetParent(null);
        transform.position = position;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(
            direction * force,
            ForceMode.Impulse
        );
    }
}
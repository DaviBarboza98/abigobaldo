using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Item : MonoBehaviour
{
    [SerializeField]
    private ItemData itemData;

    public ItemData Data => itemData;

    public ItemState State { get; private set; } = ItemState.Raw;

    public bool IsHeld { get; private set; }

    public bool IsInsideContainer { get; private set; }

    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetState(ItemState newState)
    {
        State = newState;
    }

    public void PickUp(Transform holder)
    {
        IsHeld = true;

        transform.SetParent(holder);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        col.enabled = false;
    }

    public void Drop()
    {
        IsHeld = false;

        transform.SetParent(null);

        rb.isKinematic = false;

        col.enabled = true;
    }

    public void Throw(Vector3 force)
    {
        Drop();

        rb.AddForce(force, ForceMode.Impulse);
    }

    public void SetInsideContainer(bool value)
    {
        IsInsideContainer = value;
    }
}
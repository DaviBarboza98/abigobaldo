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
    public bool IsHoldable => itemData != null && itemData.Holdable;

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
        if (holder == null)
            return;

        IsHeld = true;
        transform.SetParent(holder, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;

        col.enabled = false;
    }

    public void Drop(Vector3 dropDirection = default)
    {
        IsHeld = false;
        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;

        if (dropDirection != Vector3.zero)
        {
            transform.position += dropDirection.normalized * 0.25f;
        }
    }

    public void Throw(Vector3 force)
    {
        Drop(force);
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
    }

    public void SetInsideContainer(bool value)
    {
        IsInsideContainer = value;
    }
}
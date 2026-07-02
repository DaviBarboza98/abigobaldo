using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Item : MonoBehaviour
{
    // ==========================================
    // INFO
    // ==========================================

    [Header("=== INFO ===")]

    [SerializeField] private ItemType itemType;
    [SerializeField] private ItemState itemState = ItemState.Raw;

    // ==========================================
    // REFERENCES
    // ==========================================

    [Header("=== REFERENCES ===")]

    [SerializeField] private Transform holdPoint;

    // ==========================================
    // COMPONENTS
    // ==========================================

    private Rigidbody rb;
    private Collider itemCollider;

    // ==========================================
    // PROPERTIES
    // ==========================================

    public ItemType Type => itemType;

    public ItemState State
    {
        get => itemState;
        set => itemState = value;
    }

    public Transform HoldPoint => holdPoint;

    // ==========================================
    // UNITY
    // ==========================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    // ==========================================
    // PUBLIC
    // ==========================================

    public void PickUp(Transform holdAnchor)
    {
        transform.SetParent(holdAnchor);

        if (holdPoint != null)
        {
            transform.localPosition = -holdPoint.localPosition;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        rb.isKinematic = true;
        itemCollider.enabled = false;
    }

    public void Drop()
    {
        transform.SetParent(null);

        rb.isKinematic = false;
        itemCollider.enabled = true;
    }

    public void Throw(Vector3 force)
    {
        Drop();

        rb.AddForce(force, ForceMode.Impulse);
    }

    public void ChangeState(ItemState newState)
    {
        itemState = newState;
    }
}
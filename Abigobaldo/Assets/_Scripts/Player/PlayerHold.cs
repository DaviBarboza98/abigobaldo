using UnityEngine;

public class PlayerHold : MonoBehaviour
{
    [Header("=== REFERENCES ===")]
    [SerializeField] private Transform holdAnchor;

    [Header("=== THROW ===")]
    [SerializeField] private float throwForce = 8f;

    public Item HeldItem { get; private set; }

    public bool HasItem => HeldItem != null;

    public void PickUp(Item item)
    {
        if (HasItem)
            return;

        HeldItem = item;

        item.PickUp(holdAnchor);
    }

    public void Drop()
    {
        if (!HasItem)
            return;

        HeldItem.Drop();

        HeldItem = null;
    }

    public void Throw()
    {
        if (!HasItem)
            return;

        Item item = HeldItem;

        HeldItem = null;

        item.Throw(transform.forward * throwForce);
    }
}
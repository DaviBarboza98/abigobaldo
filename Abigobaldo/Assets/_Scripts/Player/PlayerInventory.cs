using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Transform ItemHolder;

    public Item CurrentItem { get; private set; }

    public bool HasItem => CurrentItem != null;

    public void PickUp(Item item)
    {
        if (item == null)
            return;

        CurrentItem = item;
        item.transform.SetParent(ItemHolder);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        var rb = item.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    public void Drop()
    {
        if (!HasItem)
            return;

        CurrentItem.transform.SetParent(null);
        CurrentItem = null;
    }

    public void Throw(float force)
    {
        if (!HasItem)
            return;

        var rb = CurrentItem.GetComponent<Rigidbody>();
        CurrentItem.transform.SetParent(null);
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * force, ForceMode.Impulse);
        }

        CurrentItem = null;
    }

    public void Clear()
    {
        CurrentItem = null;
    }
}
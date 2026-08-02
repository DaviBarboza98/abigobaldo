using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private Transform itemHolder;

    public Transform ItemHolder => itemHolder;

    public Item CurrentItem { get; private set; }

    public bool HasItem => CurrentItem != null;

    public void PickUp(Item item)
    {
        if (item == null)
            return;

        if (HasItem)
            return;

        if (!item.IsHoldable)
            return;

        CurrentItem = item;
        item.PickUp(itemHolder);
    }

    public void Drop()
    {
        if (!HasItem)
            return;

        CurrentItem.Drop(transform.forward);
        CurrentItem = null;
    }

    public void Throw(float force)
    {
        if (!HasItem)
            return;

        Vector3 throwDirection = transform.forward * force;

        CurrentItem.Throw(throwDirection);
        CurrentItem = null;
    }

    public Item Peek()
    {
        return CurrentItem;
    }

    public Item Remove()
    {
        if (!HasItem)
            return null;

        Item item = CurrentItem;
        CurrentItem = null;

        return item;
    }

    public bool IsHolding(ItemData itemData)
    {
        if (!HasItem)
            return false;

        return CurrentItem.Data == itemData;
    }

    public void DestroyHeldItem()
    {
        if (!HasItem)
            return;

        Destroy(CurrentItem.gameObject);
        CurrentItem = null;
    }

    public void Clear()
    {
        CurrentItem = null;
    }
}
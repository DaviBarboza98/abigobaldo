using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    [Header("Arremesso")]
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private float throwForce = 8f;

    private Item currentItem;

    public Item CurrentItem => currentItem;

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public bool TryPickUp(Item item)
    {
        if (item == null)
            return false;

        if (currentItem != null)
            return false;

        if (!item.CanBeHeld)
            return false;

        currentItem = item;

        item.PickUp(transform);

        return true;
    }

    public bool DropItem()
    {
        if (currentItem == null)
            return false;

        Item item = currentItem;
        currentItem = null;

        Vector3 position =
            transform.position +
            transform.forward * dropDistance;

        item.Drop(position);

        return true;
    }

    public bool ThrowItem()
    {
        if (currentItem == null)
            return false;

        if (!currentItem.CanBeThrown)
            return false;

        Item item = currentItem;
        currentItem = null;

        Vector3 position =
            transform.position +
            transform.forward * dropDistance;

        item.Throw(
            position,
            transform.forward,
            throwForce
        );

        return true;
    }
}
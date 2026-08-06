using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    [Header("Largar e arremessar")]
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private float throwForce = 8f;

    [Header("Rotação do item")]
    [SerializeField] private float itemRotationSensitivity = 0.25f;

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

    public Item RemoveItem()
    {
        if (currentItem == null)
            return null;

        Item item = currentItem;
        currentItem = null;

        return item;
    }

    public bool RotateItem(
        Vector2 mouseDelta,
        Transform cameraTransform
    )
    {
        if (currentItem == null)
            return false;

        if (cameraTransform == null)
            return false;

        float rotationX =
            -mouseDelta.y * itemRotationSensitivity;

        float rotationY =
            mouseDelta.x * itemRotationSensitivity;

        currentItem.transform.Rotate(
            cameraTransform.up,
            rotationY,
            Space.World
        );

        currentItem.transform.Rotate(
            cameraTransform.right,
            rotationX,
            Space.World
        );

        return true;
    }

    public bool DropItem()
    {
        if (currentItem == null)
            return false;

        Item item = RemoveItem();

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

        Item item = RemoveItem();

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
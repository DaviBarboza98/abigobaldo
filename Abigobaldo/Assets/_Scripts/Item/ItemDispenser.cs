using UnityEngine;

public class ItemDispenser : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private GameObject itemPrefab;

    public void Interact(PlayerInteraction player)
    {
        ItemHolder holder = player.ItemHolder;

        if (!holder.IsEmpty())
            return;

        if (itemPrefab == null)
            return;

        GameObject itemObject = Instantiate(itemPrefab);

        Item item = itemObject.GetComponent<Item>();

        if (item == null)
        {
            Destroy(itemObject);
            return;
        }

        if (!holder.TryPickUp(item))
        {
            Destroy(itemObject);
        }
    }
}
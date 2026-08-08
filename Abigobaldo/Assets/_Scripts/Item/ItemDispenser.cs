using UnityEngine;

public class ItemDispenser : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private GameObject itemPrefab;

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;

        ItemHolder holder = player.ItemHolder;

        if (holder == null)
            return;

        if (!holder.IsEmpty())
            return;

        if (!TryCreateItem(out Item item))
            return;

        if (!holder.TryPickUp(item))
            Destroy(item.gameObject);
    }

    private bool TryCreateItem(out Item item)
    {
        item = null;

        if (itemPrefab == null)
            return false;

        GameObject itemObject = Instantiate(itemPrefab);
        item = itemObject.GetComponent<Item>();

        if (item != null)
            return true;

        Destroy(itemObject);
        return false;
    }
}

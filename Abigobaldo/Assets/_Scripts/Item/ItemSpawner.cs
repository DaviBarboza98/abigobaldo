using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item para Spawnar")]
    [SerializeField] private GameObject itemPrefab;

    public Item SpawnItem()
    {
        TrySpawnItem(out Item item);
        return item;
    }

    public bool TrySpawnItem(out Item item)
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

using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item para Spawnar")]
    [SerializeField] private GameObject itemPrefab;

    public Item SpawnItem()
    {
        if (itemPrefab == null)
            return null;

        GameObject itemObject = Instantiate(itemPrefab);

        Item item = itemObject.GetComponent<Item>();

        if (item == null)
        {
            Destroy(itemObject);
            return null;
        }

        return item;
    }
}
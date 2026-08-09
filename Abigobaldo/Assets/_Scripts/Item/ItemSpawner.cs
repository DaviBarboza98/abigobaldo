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

    public Objeto SpawnObjeto()
    {
        TrySpawnObjeto(out Objeto objeto);
        return objeto;
    }

    public bool TrySpawnItem(out Item item)
    {
        bool spawned = TrySpawnObjeto(out Objeto objeto);
        item = objeto as Item;
        return spawned && item != null;
    }

    public bool TrySpawnObjeto(out Objeto item)
    {
        item = null;

        if (itemPrefab == null)
            return false;

        GameObject itemObject = Instantiate(itemPrefab);
        item = itemObject.GetComponent<Objeto>();

        if (item != null)
            return true;

        Destroy(itemObject);
        return false;
    }
}

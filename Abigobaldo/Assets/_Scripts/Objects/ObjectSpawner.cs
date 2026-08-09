using UnityEngine;

public class ObjectSpawner : MonoBehaviour, IInteractable
{
    [Header("Object")]
    [SerializeField] private GameObject objectPrefab;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Spawner);
    }

    public HoldableObject SpawnObject(Holder holder)
    {
        TrySpawnIntoHolder(holder, out HoldableObject spawnedObject);
        return spawnedObject;
    }

    public bool TrySpawnIntoHolder(Holder holder, out HoldableObject spawnedObject)
    {
        spawnedObject = null;

        if (objectPrefab == null || holder == null || !holder.IsEmpty())
            return false;

        GameObject objectInstance = Instantiate(objectPrefab, holder.transform.position, holder.transform.rotation);
        spawnedObject = objectInstance.GetComponent<HoldableObject>();

        if (spawnedObject != null && holder.TryPickUp(spawnedObject))
            return true;

        Destroy(objectInstance);
        spawnedObject = null;
        return false;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.Holder == null)
            return;

        if (!player.Holder.IsEmpty())
            return;

        TrySpawnIntoHolder(player.Holder, out _);
    }
}



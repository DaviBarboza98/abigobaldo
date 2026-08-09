using UnityEngine;

public class ObjetoSpawner : MonoBehaviour, IInteractable
{
    [Header("Objeto")]
    [SerializeField] private GameObject objetoPrefab;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Spawner);
    }

    public Objeto SpawnObjeto(ItemHolder holder)
    {
        TrySpawnIntoHolder(holder, out Objeto objeto);
        return objeto;
    }

    public bool TrySpawnIntoHolder(ItemHolder holder, out Objeto objeto)
    {
        objeto = null;

        if (objetoPrefab == null || holder == null || !holder.IsEmpty())
            return false;

        GameObject objetoObject = Instantiate(objetoPrefab, holder.transform.position, holder.transform.rotation);
        objeto = objetoObject.GetComponent<Objeto>();

        if (objeto != null && holder.TryPickUp(objeto))
            return true;

        Destroy(objetoObject);
        objeto = null;
        return false;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        if (!player.ItemHolder.IsEmpty())
            return;

        TrySpawnIntoHolder(player.ItemHolder, out _);
    }
}

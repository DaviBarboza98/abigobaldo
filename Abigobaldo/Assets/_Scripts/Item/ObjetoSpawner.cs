using UnityEngine;

public class ObjetoSpawner : MonoBehaviour, IInteractable
{
    [Header("Objeto")]
    [SerializeField] private GameObject objetoPrefab;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Spawner);
    }

    public Objeto SpawnObjeto()
    {
        TrySpawnObjeto(out Objeto objeto);
        return objeto;
    }

    public Objeto SpawnObjeto(ItemHolder holder)
    {
        TrySpawnObjeto(holder, out Objeto objeto);
        return objeto;
    }

    public bool TrySpawnObjeto(out Objeto objeto)
    {
        return TrySpawnObjeto(null, out objeto);
    }

    public bool TrySpawnObjeto(ItemHolder holder, out Objeto objeto)
    {
        objeto = null;

        if (objetoPrefab == null)
            return false;

        Vector3 position = holder != null ? holder.transform.position : transform.position;
        Quaternion rotation = holder != null ? holder.transform.rotation : transform.rotation;
        GameObject objetoObject = Instantiate(objetoPrefab, position, rotation);
        objeto = objetoObject.GetComponent<Objeto>();

        if (objeto != null)
            return true;

        Destroy(objetoObject);
        return false;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        if (!player.ItemHolder.IsEmpty())
            return;

        if (!TrySpawnObjeto(player.ItemHolder, out Objeto objeto))
            return;

        if (!player.ItemHolder.TryPickUp(objeto))
            Destroy(objeto.gameObject);
    }
}

using UnityEngine;

public class ObjetoSpawner : MonoBehaviour, IInteractable
{
    [Header("Objeto")]
    [SerializeField] private GameObject objetoPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool pickUpOnSpawn = true;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Spawner);
    }

    public Objeto SpawnObjeto()
    {
        TrySpawnObjeto(out Objeto objeto);
        return objeto;
    }

    public bool TrySpawnObjeto(out Objeto objeto)
    {
        objeto = null;

        if (objetoPrefab == null)
            return false;

        Transform target = spawnPoint != null ? spawnPoint : transform;
        GameObject objetoObject = Instantiate(objetoPrefab, target.position, target.rotation);
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

        if (pickUpOnSpawn && !player.ItemHolder.IsEmpty())
            return;

        if (!TrySpawnObjeto(out Objeto objeto))
            return;

        if (pickUpOnSpawn && !player.ItemHolder.TryPickUp(objeto))
            Destroy(objeto.gameObject);
    }
}

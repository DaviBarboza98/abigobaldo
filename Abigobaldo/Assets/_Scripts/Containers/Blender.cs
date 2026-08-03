using UnityEngine;

public class Blender : MonoBehaviour, IInteractable
{
    [Header("Item Holders")]
    [SerializeField] private ItemHolder holder1;
    [SerializeField] private ItemHolder holder2;

    [Header("Resultado")]
    [SerializeField] private GameObject flourPrefab;

    [Header("Tempo")]
    [SerializeField] private float processingTime = 10f;

    private CookingProcess process;
    private bool processing;

    private void Awake()
    {
        process = new CookingProcess(
            processingTime,
            0f
        );
    }

    private void Update()
    {
        if (!processing)
            return;

        process.Update(Time.deltaTime);

        if (process.IsReady)
            FinishProcessing();
    }

    public void Interact(PlayerInteraction player)
    {
        ItemHolder playerHolder = player.ItemHolder;

        if (playerHolder == null)
            return;

        // Se o jogador estiver segurando algo,
        // tenta colocar no liquidificador.
        if (!playerHolder.IsEmpty())
        {
            TryStoreItem(playerHolder);
            return;
        }

        // Se estiver com a mão vazia,
        // tenta pegar o resultado.
        TryTakeItem(playerHolder);
    }

    private void TryStoreItem(ItemHolder playerHolder)
    {
        if (processing)
            return;

        Item item = playerHolder.CurrentItem;

        if (item == null)
            return;

        ItemHolder targetHolder = GetAvailableHolder();

        if (targetHolder == null)
            return;

        playerHolder.RemoveItem();

        if (!targetHolder.TryPickUp(item))
        {
            playerHolder.TryPickUp(item);
            return;
        }

        TryStartBlender();
    }

    private void TryTakeItem(ItemHolder playerHolder)
    {
        // Durante o processamento não pode pegar.
        if (processing)
            return;

        Item item = GetResultItem();

        if (item == null)
            return;

        if (!playerHolder.TryPickUp(item))
            return;

        RemoveItemFromHolder(item);
    }

    private ItemHolder GetAvailableHolder()
    {
        if (holder1 != null && holder1.IsEmpty())
            return holder1;

        if (holder2 != null && holder2.IsEmpty())
            return holder2;

        return null;
    }

    private Item GetResultItem()
    {
        if (holder1 != null && !holder1.IsEmpty())
            return holder1.CurrentItem;

        if (holder2 != null && !holder2.IsEmpty())
            return holder2.CurrentItem;

        return null;
    }

    private void RemoveItemFromHolder(Item item)
    {
        if (holder1 != null &&
            holder1.CurrentItem == item)
        {
            holder1.RemoveItem();
            return;
        }

        if (holder2 != null &&
            holder2.CurrentItem == item)
        {
            holder2.RemoveItem();
        }
    }

    private void TryStartBlender()
    {
        if (processing)
            return;

        if (holder1 == null || holder2 == null)
            return;

        if (holder1.IsEmpty() || holder2.IsEmpty())
            return;

        string item1 = holder1.CurrentItem.ItemName;
        string item2 = holder2.CurrentItem.ItemName;

        bool validRecipe =
            (item1 == "Milho" && item2 == "Água") ||
            (item1 == "Água" && item2 == "Milho");

        if (!validRecipe)
            return;

        processing = true;
        process.Start();
    }

    private void FinishProcessing()
    {
        processing = false;

        Item item1 = holder1.RemoveItem();
        Item item2 = holder2.RemoveItem();

        if (item1 != null)
            Destroy(item1.gameObject);

        if (item2 != null)
            Destroy(item2.gameObject);

        if (flourPrefab == null)
        {
            Debug.LogWarning(
                "Blender não possui um Fubá Prefab."
            );

            return;
        }

        GameObject flour =
            Instantiate(flourPrefab);

        Item flourItem =
            flour.GetComponent<Item>();

        if (flourItem == null)
        {
            Debug.LogError(
                "O prefab de Fubá não possui Item."
            );

            Destroy(flour);
            return;
        }

        holder1.TryPickUp(flourItem);
    }
}
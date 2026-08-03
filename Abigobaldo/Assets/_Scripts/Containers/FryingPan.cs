using UnityEngine;

public class FryingPan : MonoBehaviour, IInteractable
{
    [Header("Item Holder")]
    [SerializeField] private ItemHolder holder;

    [Header("Resultado")]
    [SerializeField] private GameObject friedEggPrefab;

    [Header("Casca")]
    [SerializeField] private GameObject eggShellPrefab;

    [Header("Tempo")]
    [SerializeField] private float cookingTime = 12f;
    [SerializeField] private float burningTime = 7f;

    private CookingProcess process;
    private bool cooking;

    private PlayerInteraction currentPlayer;

    private void Awake()
    {
        process = new CookingProcess(
            cookingTime,
            burningTime
        );
    }

    private void Update()
    {
        if (!cooking)
            return;

        process.Update(Time.deltaTime);

        if (process.IsBurned)
        {
            FinishBurning();
        }
    }

    public void Interact(PlayerInteraction player)
    {
        currentPlayer = player;

        ItemHolder playerHolder =
            player.ItemHolder;

        if (playerHolder == null)
            return;

        // Mão ocupada → tenta colocar o item.
        if (!playerHolder.IsEmpty())
        {
            TryStoreItem(playerHolder);
            return;
        }

        // Mão vazia → tenta pegar o ovo.
        TryTakeItem(playerHolder);
    }

    private void TryStoreItem(ItemHolder playerHolder)
    {
        if (cooking)
            return;

        if (holder == null)
            return;

        if (!holder.IsEmpty())
            return;

        Item item =
            playerHolder.CurrentItem;

        if (item == null)
            return;

        if (item.ItemName != "Ovo")
            return;

        playerHolder.RemoveItem();

        if (!holder.TryPickUp(item))
        {
            playerHolder.TryPickUp(item);
            return;
        }

        SpawnShellForPlayer();

        cooking = true;
        process.Start();
    }

    private void TryTakeItem(ItemHolder playerHolder)
    {
        if (holder == null)
            return;

        Item item =
            holder.CurrentItem;

        if (item == null)
            return;

        if (!playerHolder.TryPickUp(item))
            return;

        holder.RemoveItem();

        cooking = false;
        process.Reset();
    }

    private void SpawnShellForPlayer()
    {
        if (currentPlayer == null)
            return;

        if (eggShellPrefab == null)
            return;

        ItemHolder playerHolder =
            currentPlayer.ItemHolder;

        if (playerHolder == null)
            return;

        if (!playerHolder.IsEmpty())
            return;

        GameObject shell =
            Instantiate(eggShellPrefab);

        Item shellItem =
            shell.GetComponent<Item>();

        if (shellItem == null)
        {
            Destroy(shell);
            return;
        }

        playerHolder.TryPickUp(shellItem);
    }

    private void FinishBurning()
    {
        cooking = false;

        Item burnedItem =
            holder.RemoveItem();

        if (burnedItem != null)
            Destroy(burnedItem.gameObject);

        process.Reset();
    }
}
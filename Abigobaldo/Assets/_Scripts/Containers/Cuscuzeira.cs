using UnityEngine;

public class Cuscuzeira : MonoBehaviour, IInteractable
{
    [Header("Item Holder")]
    [SerializeField] private ItemHolder holder;

    [Header("Resultado")]
    [SerializeField] private GameObject couscousPrefab;

    [Header("Tempo")]
    [SerializeField] private float cookingTime = 12f;
    [SerializeField] private float burningTime = 7f;

    private CookingProcess process;
    private bool cooking;

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
        ItemHolder playerHolder =
            player.ItemHolder;

        if (playerHolder == null)
            return;

        // Jogador está segurando alguma coisa.
        if (!playerHolder.IsEmpty())
        {
            TryStoreItem(playerHolder);
            return;
        }

        // Mão vazia.
        TryTakeItem(playerHolder);
    }

    private void TryStoreItem(ItemHolder playerHolder)
    {
        if (holder == null)
            return;

        // Não deixa colocar outro item
        // enquanto já existe algo dentro.
        if (!holder.IsEmpty())
            return;

        Item item =
            playerHolder.CurrentItem;

        if (item == null)
            return;

        // Por enquanto, somente Fubá
        // inicia a receita.
        if (item.ItemName != "Fubá")
            return;

        playerHolder.RemoveItem();

        if (!holder.TryPickUp(item))
        {
            playerHolder.TryPickUp(item);
            return;
        }

        StartCooking();
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

        // IMPORTANTE:
        // Se pegou antes de terminar,
        // o processo é cancelado.
        //
        // Se colocar novamente,
        // começa novamente do zero.
        cooking = false;
        process.Reset();
    }

    private void StartCooking()
    {
        cooking = true;

        // Sempre começa novamente.
        process.Reset();
        process.Start();
    }

    private void FinishCooking()
    {
        cooking = false;

        Item flour =
            holder.RemoveItem();

        if (flour != null)
            Destroy(flour.gameObject);

        if (couscousPrefab == null)
        {
            Debug.LogWarning(
                "Cuscuzeira não possui um prefab de Cuscuz."
            );

            return;
        }

        GameObject couscous =
            Instantiate(couscousPrefab);

        Item couscousItem =
            couscous.GetComponent<Item>();

        if (couscousItem == null)
        {
            Debug.LogError(
                "O prefab de Cuscuz não possui Item."
            );

            Destroy(couscous);
            return;
        }

        holder.TryPickUp(couscousItem);
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
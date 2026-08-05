using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ItemHolder itemHolder;

    [Header("Interação")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private PlayerInputHandler input;

    public ItemHolder ItemHolder => itemHolder;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (itemHolder == null)
            itemHolder = GetComponentInChildren<ItemHolder>();
    }

    private void Update()
    {
        HandleInteraction();
        HandleDrop();
        HandleThrow();
        HandleRotation(); //star adicionou isso

    }

    private void HandleInteraction()
    {
        if (!input.InteractPressed)
            return;

        if (playerCamera == null)
            return;

        if (itemHolder == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayers
        ))
        {
            return;
        }

        // ==========================================
        // 1. ITEM SPAWNER
        // ==========================================

        ItemSpawner spawner =
            hit.collider.GetComponentInParent<ItemSpawner>();

        if (spawner != null)
        {
            HandleSpawner(spawner);
            return;
        }

        // ==========================================
        // 2. ITEM
        // ==========================================

        Item item =
            hit.collider.GetComponentInParent<Item>();

        if (item != null)
        {
            HandleItem(item);
            return;
        }

        // ==========================================
        // 3. INTERACTABLE
        // ==========================================

        IInteractable interactable =
            hit.collider.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            interactable.Interact(this);
        }
    }

    private void HandleSpawner(ItemSpawner spawner)
    {
        if (!itemHolder.IsEmpty())
            return;

        Item item = spawner.SpawnItem();

        if (item == null)
            return;

        if (!itemHolder.TryPickUp(item))
        {
            Destroy(item.gameObject);
        }
    }

    private void HandleItem(Item item)
    {
        if (!itemHolder.IsEmpty())
            return;

        itemHolder.TryPickUp(item);
    }

    private void HandleDrop()
    {
        if (!input.DropPressed)
            return;

        if (itemHolder == null)
            return;

        itemHolder.DropItem();
    }

    private void HandleThrow()
    {
        if (!input.ThrowPressed)
            return;

        if (itemHolder == null)
            return;

        itemHolder.ThrowItem();
    }

    private void HandleRotation() //star adicionou isso
{
    // star: se não apertou R não faz nada
    if (!input.RotatePressed)
        return;

    // star: se o holder não existir ou estiver vazio não faz nada
    if (itemHolder == null || itemHolder.IsEmpty())
        return;

    // star:gira o "segurador de itens" e o item que está dentro em 90 graus no eixo Y
    itemHolder.transform.Rotate(0f, 90f, 0f, Space.Self);
}
}

//star: criei um private void novo pra criar o negócio q faz o item girar quando o player aperta R
using UnityEngine;

public class StoveSlot : MonoBehaviour, IInteractable
{
    [SerializeField] private ContainerType acceptedType = ContainerType.Frigideira;
    [SerializeField] private Transform containerAnchor;
    [SerializeField] private ParticleEmitterController flameParticles;

    private ItemContainer currentContainer;
    private Collider slotCollider;

    public Transform ContainerAnchor => containerAnchor != null ? containerAnchor : transform;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.HomeSlot);
    }

    public static StoveSlot CreateFor(ItemContainer container, Vector3 size)
    {
        GameObject slotObject = new GameObject($"{container.name}_StoveSlot");
        slotObject.transform.SetPositionAndRotation(container.transform.position, container.transform.rotation);
        GameLayers.SetLayerRecursivelyIfDefault(slotObject, GameLayers.HomeSlot);

        BoxCollider collider = slotObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = size;

        StoveSlot slot = slotObject.AddComponent<StoveSlot>();
        slot.acceptedType = container.Type;
        slot.containerAnchor = slotObject.transform;
        slot.currentContainer = container;
        slot.slotCollider = collider;
        slot.UpdateFlame();

        return slot;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null || player.ItemHolder.IsEmpty())
            return;

        Objeto heldItem = player.ItemHolder.CurrentObjeto;
        ItemContainer heldContainer = heldItem != null ? heldItem.GetComponent<ItemContainer>() : null;

        if (heldContainer == null || heldContainer.Type != acceptedType)
            return;

        player.ItemHolder.RemoveObjeto();
        currentContainer = heldContainer;
        heldContainer.DockToSlot(this);
        UpdateFlame();
    }

    public void ClearIfCurrent(ItemContainer container)
    {
        if (currentContainer != container)
            return;

        currentContainer = null;
        UpdateFlame();
    }

    private void UpdateFlame()
    {
        if (slotCollider == null)
            slotCollider = GetComponent<Collider>();

        if (slotCollider != null)
            slotCollider.enabled = currentContainer == null;

        if (flameParticles == null)
            return;

        if (currentContainer != null)
            flameParticles.Play();
        else
            flameParticles.Stop();
    }
}

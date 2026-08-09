using UnityEngine;

public class BlenderCupSlot : MonoBehaviour, IInteractable
{
    [Header("Aceite")]
    [SerializeField] private ItemData acceptedData;

    [Header("Liquidificador")]
    [SerializeField] private Blender linkedBlender;
    [SerializeField] private bool disableDockedColliders = true;

    private Objeto currentObjeto;
    private Collider slotCollider;
    private Collider[] dockedColliders;

    public Transform Anchor => transform;
    public bool IsOccupied => currentObjeto != null;
    public Objeto CurrentObjeto => currentObjeto;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.HomeSlot);
        slotCollider = GetComponent<Collider>();

        if (linkedBlender == null)
            linkedBlender = GetComponentInParent<Blender>();

        if (linkedBlender != null)
            linkedBlender.SetRequiredCupSlot(this);
    }

    public bool CanAccept(Objeto objeto)
    {
        if (objeto == null || IsOccupied)
            return false;

        if (acceptedData != null && objeto.Data != acceptedData)
            return false;

        return objeto.Role == Objeto.ObjetoRole.CopoLiquidificador || acceptedData != null;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        if (player.ItemHolder.IsEmpty())
        {
            RemoveDocked(player.ItemHolder);
            return;
        }

        Objeto heldObjeto = player.ItemHolder.CurrentObjeto;

        if (!CanAccept(heldObjeto))
            return;

        player.ItemHolder.RemoveObjeto();
        Dock(heldObjeto);
    }

    public void Dock(Objeto objeto)
    {
        if (!CanAccept(objeto))
            return;

        currentObjeto = objeto;
        objeto.PlaceAt(Anchor);
        objeto.transform.SetParent(Anchor, true);

        dockedColliders = objeto.GetComponentsInChildren<Collider>();
        SetDockedCollidersEnabled(false);

        if (linkedBlender != null)
            linkedBlender.NotifyRequiredCupChanged();
    }

    public bool RemoveDocked(ItemHolder holder)
    {
        if (holder == null || !holder.IsEmpty() || currentObjeto == null)
            return false;

        Objeto objeto = currentObjeto;
        currentObjeto = null;

        if (linkedBlender != null)
            linkedBlender.NotifyRequiredCupChanged();

        objeto.transform.SetParent(null, true);
        SetDockedCollidersEnabled(true);
        dockedColliders = null;
        return holder.TryPickUp(objeto);
    }

    private void SetDockedCollidersEnabled(bool enabled)
    {
        if (!disableDockedColliders || dockedColliders == null)
            return;

        foreach (Collider dockedCollider in dockedColliders)
        {
            if (dockedCollider != null && dockedCollider != slotCollider)
                dockedCollider.enabled = enabled;
        }
    }
}

using UnityEngine;

public class ObjetoHomeSlot : MonoBehaviour, IInteractable
{
    [SerializeField] private Objeto acceptedObjeto;
    [SerializeField] private Transform anchor;
    [SerializeField] private bool acceptsOnlyOriginalObject = true;
    [SerializeField] private bool showOnlyWhenHoldingAcceptedObject = true;

    private Collider slotCollider;
    private Highlightable highlightable;
    private Objeto currentObjeto;

    public Transform Anchor => anchor != null ? anchor : transform;
    public Objeto AcceptedObjeto => acceptedObjeto;
    public bool IsAvailable => currentObjeto == null || currentObjeto == acceptedObjeto;

    public static ObjetoHomeSlot CreateFor(Objeto objeto, Vector3 padding)
    {
        GameObject slotObject = new GameObject($"{objeto.name}_HomeSlot");
        slotObject.transform.SetPositionAndRotation(objeto.transform.position, objeto.transform.rotation);
        GameLayers.SetLayerRecursivelyIfDefault(slotObject, GameLayers.HomeSlot);
        Bounds objetoBounds = GetRendererBounds(objeto);

        BoxCollider collider = slotObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = slotObject.transform.InverseTransformPoint(objetoBounds.center);
        collider.size = GetColliderSizeFor(objetoBounds, padding);

        ObjetoHomeSlot slot = slotObject.AddComponent<ObjetoHomeSlot>();
        slot.acceptedObjeto = objeto;
        slot.anchor = slotObject.transform;
        slot.slotCollider = collider;
        slot.currentObjeto = objeto;
        slot.highlightable = slotObject.AddComponent<Highlightable>();
        slot.highlightable.SetHighlighted(false);
        objeto.SetHomeSlot(slot);

        return slot;
    }

    private void Awake()
    {
        if (anchor == null)
            anchor = transform;

        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.HomeSlot);

        slotCollider = GetComponent<Collider>();
        highlightable = GetComponent<Highlightable>();

        if (acceptedObjeto != null)
        {
            currentObjeto = acceptedObjeto;
            acceptedObjeto.SetHomeSlot(this);
        }
    }

    public bool CanAccept(Objeto objeto)
    {
        if (objeto == null)
            return false;

        if (acceptsOnlyOriginalObject && acceptedObjeto != null && objeto != acceptedObjeto)
            return false;

        return IsAvailable;
    }

    public void MarkPickedUp(Objeto objeto)
    {
        if (objeto == currentObjeto)
            currentObjeto = null;
    }

    public void Place(Objeto objeto)
    {
        if (!CanAccept(objeto))
            return;

        objeto.PlaceAt(Anchor);
        currentObjeto = objeto;
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null || player.ItemHolder.IsEmpty())
            return;

        Objeto heldObjeto = player.ItemHolder.CurrentObjeto;

        if (!CanAccept(heldObjeto))
            return;

        player.ItemHolder.RemoveObjeto();
        Place(heldObjeto);
    }

    public bool ShouldHighlightFor(Objeto heldObjeto)
    {
        if (!showOnlyWhenHoldingAcceptedObject)
            return true;

        return CanAccept(heldObjeto);
    }

    private static Vector3 GetColliderSizeFor(Bounds bounds, Vector3 padding)
    {
        if (bounds.size == Vector3.zero)
            return Vector3.one * 0.5f;

        return bounds.size + padding;
    }

    private static Bounds GetRendererBounds(Objeto objeto)
    {
        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(objeto.transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = targetRenderer.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(targetRenderer.bounds);
        }

        return hasBounds ? bounds : new Bounds(objeto.transform.position, Vector3.zero);
    }

}

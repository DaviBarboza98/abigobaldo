using UnityEngine;

public class ObjetoReturnPoint : MonoBehaviour, IInteractable, IHighlightStateReceiver
{
    [SerializeField] private Objeto acceptedObjeto;
    [SerializeField] private Transform anchor;
    [SerializeField] private Vector3 colliderSize = Vector3.one * 0.6f;
    [SerializeField] private bool showMarkerOnlyOnHighlight = true;

    private Renderer markerRenderer;

    public void Initialize(Objeto objeto, Vector3 size)
    {
        acceptedObjeto = objeto;
        colliderSize = size;

        if (anchor == null)
            anchor = transform;

        EnsureCollider();
        EnsureMarker();
        EnsureHighlight();
    }

    private void Awake()
    {
        if (anchor == null)
            anchor = transform;

        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Interactable);
        EnsureCollider();
        EnsureMarker();
        EnsureHighlight();
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null || player.ItemHolder.IsEmpty())
            return;

        Objeto held = player.ItemHolder.CurrentObjeto;
        if (held == null || held != acceptedObjeto)
            return;

        player.ItemHolder.RemoveObjeto();
        held.PlaceAt(anchor);

        foreach (MonoBehaviour behaviour in held.GetComponents<MonoBehaviour>())
        {
            if (behaviour is ObjetoReturnStateReceiver receiver)
                receiver.OnReturnedToOrigin();
        }
    }

    public bool ShouldHighlightFor(Objeto heldObjeto)
    {
        return heldObjeto != null && heldObjeto == acceptedObjeto;
    }

    public void OnHighlightChanged(bool highlighted)
    {
        if (markerRenderer != null && showMarkerOnlyOnHighlight)
            markerRenderer.enabled = highlighted;
    }

    private void EnsureCollider()
    {
        Collider existing = GetComponent<Collider>();
        if (existing != null)
        {
            existing.isTrigger = true;
            return;
        }

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = colliderSize;
    }

    private void EnsureHighlight()
    {
        if (GetComponent<Highlightable>() == null)
            gameObject.AddComponent<Highlightable>();
    }

    private void EnsureMarker()
    {
        if (markerRenderer != null)
            return;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "ReturnPointMarker";
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localRotation = Quaternion.identity;
        marker.transform.localScale = colliderSize;

        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
            Destroy(markerCollider);

        markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null)
            markerRenderer.enabled = !showMarkerOnlyOnHighlight;
    }
}

using UnityEngine;

public class ObjetoHomeSlot : MonoBehaviour, IInteractable, IHighlightStateReceiver
{
    [SerializeField] private Objeto acceptedObjeto;
    [SerializeField] private Transform anchor;
    [SerializeField] private bool acceptsOnlyOriginalObject = true;
    [SerializeField] private bool showOnlyWhenHoldingAcceptedObject = true;
    [SerializeField] private Transform ghostRoot;

    private Collider slotCollider;
    private Highlightable highlightable;
    private Renderer[] ghostRenderers;
    private Material ghostMaterial;
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
        slot.BuildGhostFrom(objeto);
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
        ghostRenderers = ghostRoot != null ? ghostRoot.GetComponentsInChildren<Renderer>(true) : null;
        SetGhostVisible(false);

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
        SetGhostVisible(false);
    }

    public bool ShouldHighlightFor(Objeto heldObjeto)
    {
        if (!showOnlyWhenHoldingAcceptedObject)
            return true;

        return CanAccept(heldObjeto);
    }

    public void OnHighlightChanged(bool highlighted)
    {
        SetGhostVisible(highlighted && currentObjeto == null);
    }

    private void BuildGhostFrom(Objeto objeto)
    {
        if (objeto == null)
            return;

        GameObject root = new GameObject("PlacementGhost");
        root.transform.SetParent(transform, false);
        ghostRoot = root.transform;
        ghostMaterial = CreateGhostMaterial();

        MeshRenderer[] meshRenderers = objeto.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer sourceRenderer in meshRenderers)
        {
            MeshFilter sourceFilter = sourceRenderer.GetComponent<MeshFilter>();

            if (sourceFilter == null || sourceFilter.sharedMesh == null)
                continue;

            GameObject ghostPart = new GameObject(sourceRenderer.name);
            ghostPart.transform.SetParent(root.transform, false);
            CopyRelativeTransform(objeto.transform, sourceRenderer.transform, ghostPart.transform);

            MeshFilter ghostFilter = ghostPart.AddComponent<MeshFilter>();
            ghostFilter.sharedMesh = sourceFilter.sharedMesh;

            MeshRenderer ghostRenderer = ghostPart.AddComponent<MeshRenderer>();
            ghostRenderer.sharedMaterials = CreateMaterialArray(sourceRenderer.sharedMaterials.Length);
            ghostRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ghostRenderer.receiveShadows = false;
        }

        SkinnedMeshRenderer[] skinnedRenderers = objeto.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (SkinnedMeshRenderer sourceRenderer in skinnedRenderers)
        {
            if (sourceRenderer.sharedMesh == null)
                continue;

            GameObject ghostPart = new GameObject(sourceRenderer.name);
            ghostPart.transform.SetParent(root.transform, false);
            CopyRelativeTransform(objeto.transform, sourceRenderer.transform, ghostPart.transform);

            SkinnedMeshRenderer ghostRenderer = ghostPart.AddComponent<SkinnedMeshRenderer>();
            ghostRenderer.sharedMesh = sourceRenderer.sharedMesh;
            ghostRenderer.sharedMaterials = CreateMaterialArray(sourceRenderer.sharedMaterials.Length);
            ghostRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ghostRenderer.receiveShadows = false;
        }

        ghostRenderers = root.GetComponentsInChildren<Renderer>(true);
        SetGhostVisible(false);
    }

    private void SetGhostVisible(bool visible)
    {
        if (ghostRenderers == null)
            return;

        foreach (Renderer ghostRenderer in ghostRenderers)
        {
            if (ghostRenderer != null)
            {
                ghostRenderer.SetPropertyBlock(null);
                ghostRenderer.enabled = visible;
            }
        }
    }

    private Material[] CreateMaterialArray(int length)
    {
        int safeLength = Mathf.Max(1, length);
        Material[] materials = new Material[safeLength];

        for (int i = 0; i < safeLength; i++)
            materials[i] = ghostMaterial;

        return materials;
    }

    private static Material CreateGhostMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        Color ghostColor = GameInteractionManager.Instance != null
            ? GameInteractionManager.Instance.PlacementGhostColor
            : new Color(1f, 0.85f, 0.2f, 0.35f);

        material.name = "PlacementGhost_Runtime";
        material.SetColor("_BaseColor", ghostColor);
        material.SetColor("_Color", ghostColor);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        return material;
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

    private static void CopyRelativeTransform(Transform root, Transform source, Transform target)
    {
        Matrix4x4 relativeMatrix = root.worldToLocalMatrix * source.localToWorldMatrix;
        target.localPosition = relativeMatrix.GetColumn(3);
        target.localRotation = Quaternion.LookRotation(relativeMatrix.GetColumn(2), relativeMatrix.GetColumn(1));
        target.localScale = new Vector3(
            relativeMatrix.GetColumn(0).magnitude,
            relativeMatrix.GetColumn(1).magnitude,
            relativeMatrix.GetColumn(2).magnitude
        );
    }
}

using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Objeto))]
public class BlenderCup : MonoBehaviour, IInteractable, ItemHoldStateReceiver
{
    [Header("Conteudo")]
    [SerializeField] private int maxItems = 1;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Vector3 contentLocalOffset;
    [SerializeField] private float ingredientVisualScale = 0.18f;
    [SerializeField] private float blendedVisualScale = 0.05f;
    [SerializeField] private bool centerVisualBoundsOnSpawn = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> contents = new List<ItemData>();
    private readonly List<GameObject> visuals = new List<GameObject>();
    private Blender attachedBase;
    private Objeto objeto;
    private Rigidbody rb;
    private Collider[] colliders;

    public IReadOnlyList<ItemData> Contents => contents;
    public bool IsAttached => attachedBase != null;
    public bool HasSingleOutput => contents.Count == 1;

    private void Awake()
    {
        objeto = GetComponent<Objeto>();
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();

        if (contentRoot == null)
            contentRoot = FindContentRoot(transform);
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        ItemHolder holder = player.ItemHolder;

        if (holder.IsEmpty())
        {
            if (TryTakeLastItem(holder))
                return;

            if (attachedBase != null)
                DetachToHolder(holder);
            else
                holder.TryPickUp(objeto);

            return;
        }

        PlateContainer heldPlate = holder.CurrentObjeto != null
            ? holder.CurrentObjeto.GetComponent<PlateContainer>()
            : null;

        if (heldPlate != null && TryMoveOutputToPlate(heldPlate))
            return;

        TryStoreHeldObject(holder);
    }

    public void AttachTo(Blender blenderBase, Transform anchor)
    {
        attachedBase = blenderBase;

        if (anchor != null)
        {
            transform.SetParent(anchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        SetPhysicsEnabled(false);
    }

    public bool DetachToHolder(ItemHolder holder)
    {
        if (holder == null || !holder.IsEmpty())
            return false;

        Blender previousBase = attachedBase;

        if (!holder.TryPickUp(objeto))
            return false;

        attachedBase = null;

        previousBase?.NotifyCupPickedUp(this);
        return true;
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (plate == null || contents.Count != 1)
            return false;

        if (!plate.TryAddItem(contents[0]))
            return false;

        ClearContents();
        return true;
    }

    public bool TryTakeOutput(ItemHolder holder)
    {
        if (holder == null || !holder.IsEmpty() || contents.Count != 1)
            return false;

        if (!ObjetoDeliveryUtility.TryDeliverToHolder(contents[0], holder))
            return false;

        ClearContents();
        return true;
    }

    public bool TryTakeLastItem(ItemHolder holder)
    {
        if (holder == null || !holder.IsEmpty() || contents.Count == 0)
            return false;

        int lastIndex = contents.Count - 1;
        ItemData item = contents[lastIndex];

        if (!ObjetoDeliveryUtility.TryDeliverToHolder(item, holder))
            return false;

        contents.RemoveAt(lastIndex);
        RemoveLastVisual();
        LogContents();
        return true;
    }

    public void ReplaceContentsWithResult(ItemData result)
    {
        contents.Clear();

        if (result != null)
            contents.Add(result);

        RefreshVisuals(blendedVisualScale);
    }

    public void ClearContents()
    {
        contents.Clear();
        RefreshVisuals(ingredientVisualScale);
    }

    public void UpdateBlendVisuals(
        bool isCooking,
        float spinSpeed
    )
    {
        if (!isCooking || contentRoot == null)
            return;

        UpdateTransformBlendVisuals(spinSpeed);
    }

    public void OnPickedUp()
    {
        if (attachedBase != null)
            attachedBase.NotifyCupPickedUp(this);

        attachedBase = null;
    }

    public void OnDropped()
    {
    }

    public void OnThrown()
    {
    }

    private bool TryStoreHeldObject(ItemHolder holder)
    {
        if (!IsAttached)
            return false;

        if (contents.Count >= maxItems)
            return false;

        Objeto held = holder.CurrentObjeto;
        if (held == null || held.Data == null || held.GetComponent<BlenderCup>() != null)
            return false;

        Objeto removed = holder.RemoveObjeto();
        if (removed == null)
            return false;

        contents.Add(removed.Data);
        Destroy(removed.gameObject);
        CreateVisual(removed.Data, ingredientVisualScale);
        LogContents();
        return true;
    }

    private void UpdateTransformBlendVisuals(float spinSpeed)
    {
        for (int i = 0; i < visuals.Count; i++)
        {
            GameObject visual = visuals[i];
            if (visual == null)
                continue;

            visual.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void RefreshVisuals(float scale)
    {
        ClearVisuals();

        if (contentRoot == null)
            return;

        for (int i = 0; i < contents.Count; i++)
            CreateVisual(contents[i], scale);
    }

    private void CreateVisual(ItemData data, float scale)
    {
        if (data == null || data.Prefab == null || contentRoot == null)
            return;

        GameObject visual = Instantiate(data.Prefab, contentRoot);
        visual.name = $"BlenderCupVisual_{data.DisplayName}";
        visual.transform.localPosition = contentLocalOffset;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * scale;
        CenterVisualOnContentPoint(visual, contentLocalOffset);
        RecipeVisualUtility.DisableGameplayComponents(visual);
        visuals.Add(visual);
    }

    private void CenterVisualOnContentPoint(GameObject visual, Vector3 targetLocalPosition)
    {
        if (!centerVisualBoundsOnSpawn || visual == null || contentRoot == null)
            return;

        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 targetWorldPosition = contentRoot.TransformPoint(targetLocalPosition);
        Vector3 correction = targetWorldPosition - bounds.center;
        visual.transform.position += correction;
    }

    private void ClearVisuals()
    {
        for (int i = visuals.Count - 1; i >= 0; i--)
            if (visuals[i] != null)
                Destroy(visuals[i]);

        visuals.Clear();
    }

    private void RemoveLastVisual()
    {
        int lastVisualIndex = visuals.Count - 1;
        if (lastVisualIndex < 0)
            return;

        GameObject visual = visuals[lastVisualIndex];
        visuals.RemoveAt(lastVisualIndex);

        if (visual != null)
            Destroy(visual);
    }

    private void SetPhysicsEnabled(bool enabled)
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
            rb.detectCollisions = true;
        }

        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider>();

        foreach (Collider targetCollider in colliders)
            if (targetCollider != null)
                targetCollider.enabled = true;
    }

    private static Transform FindContentRoot(Transform root)
    {
        if (root == null)
            return null;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            string lowerName = child.name.ToLowerInvariant();
            if (lowerName == "cupcontentroot" || lowerName == "contentroot" || lowerName == "conteudo")
                return child;
        }

        return root;
    }

    private void LogContents()
    {
        if (!showDebugLogs)
            return;

        StringBuilder text = new StringBuilder();
        for (int i = 0; i < contents.Count; i++)
        {
            text.Append(contents[i] != null ? contents[i].DisplayName : "Dado nulo");
            if (i < contents.Count - 1)
                text.Append(", ");
        }

        Debug.Log($"{name}: {text}");
    }

    private void OnValidate()
    {
        maxItems = 1;
    }
}

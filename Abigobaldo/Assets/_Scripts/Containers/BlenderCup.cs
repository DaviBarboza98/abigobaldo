using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(HoldableObject))]
public class BlenderCup : MonoBehaviour, IInteractable, HoldStateReceiver
{
    [Header("Contents")]
    [SerializeField] private int maxObjects = 1;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private float ingredientVisualScale = 0.18f;
    [SerializeField] private float blendedVisualScale = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ObjectData> contents = new List<ObjectData>();
    private readonly List<GameObject> visuals = new List<GameObject>();
    private Blender attachedBase;
    private HoldableObject holdableObject;
    private Rigidbody rb;
    private Collider[] colliders;

    public IReadOnlyList<ObjectData> Contents => contents;
    public int ContentCount => contents.Count;
    public bool IsAttached => attachedBase != null;
    public bool HasSingleOutput => contents.Count == 1;

    private void Awake()
    {
        holdableObject = GetComponent<HoldableObject>();
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();

        if (contentRoot == null)
            contentRoot = FindContentRoot(transform);
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.Holder == null)
            return;

        Holder holder = player.Holder;

        if (holder.IsEmpty())
        {
            if (TryTakeLastObject(holder))
                return;

            if (attachedBase != null)
                DetachToHolder(holder);
            else
                holder.TryPickUp(holdableObject);

            return;
        }

        PlateContainer heldPlate = holder.CurrentObject != null
            ? holder.CurrentObject.GetComponent<PlateContainer>()
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

    public bool DetachToHolder(Holder holder)
    {
        if (holder == null || !holder.IsEmpty())
            return false;

        Blender previousBase = attachedBase;

        if (!holder.TryPickUp(holdableObject))
            return false;

        attachedBase = null;

        previousBase?.NotifyCupPickedUp(this);
        return true;
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (plate == null || contents.Count != 1)
            return false;

        if (!plate.TryAddObject(contents[0]))
            return false;

        ClearContents();
        return true;
    }

    public bool TryTakeOutput(Holder holder)
    {
        if (holder == null || !holder.IsEmpty() || contents.Count != 1)
            return false;

        if (!ObjectDeliveryUtility.TryDeliverToHolder(contents[0], holder))
            return false;

        ClearContents();
        return true;
    }

    public bool TryTakeLastObject(Holder holder)
    {
        if (holder == null || !holder.IsEmpty() || contents.Count == 0)
            return false;

        int lastIndex = contents.Count - 1;
        ObjectData targetObject = contents[lastIndex];

        if (!ObjectDeliveryUtility.TryDeliverToHolder(targetObject, holder))
            return false;

        contents.RemoveAt(lastIndex);
        RemoveLastVisual();
        LogContents();
        return true;
    }

    public void ReplaceContentsWithResult(ObjectData result)
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

    private bool TryStoreHeldObject(Holder holder)
    {
        if (!IsAttached)
            return false;

        if (contents.Count >= maxObjects)
            return false;

        HoldableObject held = holder.CurrentObject;
        if (held == null || held.Data == null || held.GetComponent<BlenderCup>() != null)
            return false;

        HoldableObject removed = holder.RemoveObject();
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

            visual.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
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

    private void CreateVisual(ObjectData data, float scale)
    {
        if (data == null || data.Prefab == null || contentRoot == null)
            return;

        GameObject visual = Instantiate(data.Prefab, contentRoot);
        visual.name = $"BlenderCupVisual_{data.DisplayName}";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * scale;
        RecipeVisualUtility.DisableGameplayComponents(visual);
        visuals.Add(visual);
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
            if (lowerName == "cupcontentroot" || lowerName == "contentroot")
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
            text.Append(contents[i] != null ? contents[i].DisplayName : "Null data");
            if (i < contents.Count - 1)
                text.Append(", ");
        }

        Debug.Log($"{name}: {text}");
    }

    private void OnValidate()
    {
        maxObjects = 1;
    }
}



using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Objeto))]
public class BlenderCup : MonoBehaviour, IInteractable, ItemHoldStateReceiver
{
    [Header("Conteudo")]
    [SerializeField] private int maxItems = 5;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private Vector3 contentLocalOffset;
    [SerializeField] private float ingredientVisualScale = 0.18f;
    [SerializeField] private float blendedVisualScale = 0.05f;

    [Header("Fisica visual")]
    [SerializeField] private bool usePhysicalIngredientVisuals = true;
    [SerializeField] private float visualMass = 0.04f;
    [SerializeField] private float randomForce = 2.6f;
    [SerializeField] private float randomTorque = 8f;
    [SerializeField] private float forceInterval = 0.08f;
    [SerializeField] private float fallbackBoundaryRadius = 0.22f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> contents = new List<ItemData>();
    private readonly List<GameObject> visuals = new List<GameObject>();
    private readonly List<Rigidbody> visualBodies = new List<Rigidbody>();
    private Blender attachedBase;
    private Objeto objeto;
    private Rigidbody rb;
    private Collider[] colliders;
    private float nextForceTime;

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
        attachedBase = null;
        transform.SetParent(null, true);
        SetPhysicsEnabled(true);

        if (!holder.TryPickUp(objeto))
        {
            AttachTo(previousBase, previousBase != null ? previousBase.transform : null);
            return false;
        }

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

    public void ReplaceContentsWithResult(ItemData result)
    {
        contents.Clear();

        if (result != null)
            contents.Add(result);

        RefreshVisuals(blendedVisualScale, false);
    }

    public void ClearContents()
    {
        contents.Clear();
        RefreshVisuals(ingredientVisualScale, false);
    }

    public void UpdateBlendVisuals(
        bool isCooking,
        float spinSpeed,
        float shakeRadius,
        float morphStartTime,
        float morphDuration,
        CookingProcess cookingProcess,
        RecipeData activeRecipe,
        ref bool morphedToResult
    )
    {
        if (!isCooking || contentRoot == null)
            return;

        if (usePhysicalIngredientVisuals)
            UpdatePhysicalBlendVisuals();
        else
            UpdateTransformBlendVisuals(spinSpeed, shakeRadius);

        TryMorphToResult(morphStartTime, morphDuration, cookingProcess, activeRecipe, ref morphedToResult);
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
        RefreshVisuals(ingredientVisualScale, usePhysicalIngredientVisuals);
        LogContents();
        return true;
    }

    private void UpdateTransformBlendVisuals(float spinSpeed, float shakeRadius)
    {
        for (int i = 0; i < visuals.Count; i++)
        {
            GameObject visual = visuals[i];
            if (visual == null)
                continue;

            float phase = Time.time * 18f + i;
            Vector3 shake = new Vector3(Mathf.Sin(phase), Mathf.Cos(phase * 1.3f), Mathf.Sin(phase * 0.7f));
            visual.transform.localPosition = GetVisualPosition(i, visuals.Count) + shake * shakeRadius;
            visual.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void UpdatePhysicalBlendVisuals()
    {
        if (Time.time < nextForceTime)
            return;

        nextForceTime = Time.time + forceInterval;

        for (int i = visualBodies.Count - 1; i >= 0; i--)
        {
            Rigidbody body = visualBodies[i];
            if (body == null)
            {
                visualBodies.RemoveAt(i);
                continue;
            }

            body.AddForce(Random.onUnitSphere * randomForce, ForceMode.Acceleration);
            body.AddTorque(Random.onUnitSphere * randomTorque, ForceMode.Acceleration);

            Vector3 localPosition = contentRoot.InverseTransformPoint(body.position) - contentLocalOffset;
            if (localPosition.magnitude <= fallbackBoundaryRadius)
                continue;

            Vector3 clampedLocal = contentLocalOffset + localPosition.normalized * fallbackBoundaryRadius;
            Vector3 targetWorld = contentRoot.TransformPoint(clampedLocal);
            body.AddForce((targetWorld - body.position) * randomForce * 2f, ForceMode.Acceleration);
        }
    }

    private void TryMorphToResult(
        float morphStartTime,
        float morphDuration,
        CookingProcess cookingProcess,
        RecipeData activeRecipe,
        ref bool morphedToResult
    )
    {
        if (morphedToResult || activeRecipe == null || cookingProcess == null || cookingProcess.Timer < morphStartTime)
            return;

        float transition = morphDuration <= 0f
            ? 1f
            : Mathf.InverseLerp(morphStartTime, morphStartTime + morphDuration, cookingProcess.Timer);

        float visualScale = Mathf.Lerp(ingredientVisualScale, blendedVisualScale, transition);
        foreach (GameObject visual in visuals)
            if (visual != null)
                visual.transform.localScale = Vector3.one * visualScale;

        if (transition < 1f)
            return;

        morphedToResult = true;
        ClearVisuals();
        CreateVisual(activeRecipe.ResultItem, 0, 1, blendedVisualScale, false);
    }

    private void RefreshVisuals(float scale, bool physical)
    {
        ClearVisuals();

        if (contentRoot == null)
            return;

        for (int i = 0; i < contents.Count; i++)
            CreateVisual(contents[i], i, contents.Count, scale, physical);
    }

    private void CreateVisual(ItemData data, int index, int count, float scale, bool physical)
    {
        if (data == null || data.Prefab == null || contentRoot == null)
            return;

        GameObject visual = Instantiate(data.Prefab, contentRoot);
        visual.name = $"BlenderCupVisual_{data.DisplayName}";
        visual.transform.localPosition = GetVisualPosition(index, count);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * scale;
        RecipeVisualUtility.DisableGameplayComponents(visual, !physical);
        ConfigureVisualPhysics(visual, physical);
        visuals.Add(visual);
    }

    private void ConfigureVisualPhysics(GameObject visual, bool physical)
    {
        if (!physical)
            return;

        Rigidbody body = visual.GetComponent<Rigidbody>();
        if (body == null)
            body = visual.AddComponent<Rigidbody>();

        body.mass = visualMass;
        body.useGravity = false;
        body.isKinematic = false;
        body.detectCollisions = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        visualBodies.Add(body);
    }

    private Vector3 GetVisualPosition(int index, int count)
    {
        if (count <= 1)
            return contentLocalOffset;

        float angle = Mathf.PI * 2f * index / count;
        return contentLocalOffset + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.08f;
    }

    private void ClearVisuals()
    {
        for (int i = visuals.Count - 1; i >= 0; i--)
            if (visuals[i] != null)
                Destroy(visuals[i]);

        visuals.Clear();
        visualBodies.Clear();
    }

    private void SetPhysicsEnabled(bool enabled)
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = !enabled;
            rb.useGravity = enabled;
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
        maxItems = Mathf.Max(1, maxItems);
        visualMass = Mathf.Max(0.001f, visualMass);
        randomForce = Mathf.Max(0f, randomForce);
        randomTorque = Mathf.Max(0f, randomTorque);
        forceInterval = Mathf.Max(0.02f, forceInterval);
        fallbackBoundaryRadius = Mathf.Max(0.01f, fallbackBoundaryRadius);
    }
}

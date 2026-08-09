using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlateContainer : MonoBehaviour, IInteractable
{
    [SerializeField] private int maxObjects = 1;
    [SerializeField] private Transform contentVisualRoot;
    [SerializeField] private Vector3 contentVisualLocalOffset = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private float contentVisualScale = 0.25f;
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ObjectData> platedObjects = new List<ObjectData>();
    private readonly List<Color?> platedTints = new List<Color?>();
    private readonly List<Material> platedMaterials = new List<Material>();
    private readonly List<GameObject> contentVisuals = new List<GameObject>();

    public IReadOnlyList<ObjectData> PlatedObjects => platedObjects;
    public int ObjectCount => platedObjects.Count;
    public bool IsFull => platedObjects.Count >= maxObjects;
    public bool IsEmpty => platedObjects.Count == 0;

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.Holder == null)
            return;

        Holder holder = player.Holder;

        if (holder.IsEmpty())
        {
            LogContents();
            return;
        }

        HoldableObject heldObject = holder.CurrentObject;

        if (heldObject == null || heldObject.gameObject == gameObject)
        {
            LogContents();
            return;
        }

        TryAddHeldObject(holder);
    }

    public bool TryAddHeldObject(Holder holder)
    {
        if (holder == null || holder.IsEmpty())
            return false;

        HoldableObject heldObject = holder.CurrentObject;

        if (heldObject == null || heldObject.Data == null)
            return false;

        if (heldObject.GetComponent<PlateContainer>() != null)
            return false;

        Color? tint = heldObject.HasRuntimeTint ? heldObject.RuntimeTint : null;
        if (!TryAddObject(heldObject.Data, tint, heldObject.RuntimeMaterial))
            return false;

        HoldableObject removedObject = holder.RemoveObject();

        if (removedObject != null)
            Destroy(removedObject.gameObject);

        return true;
    }

    public bool TryAddLooseObject(HoldableObject looseObject)
    {
        if (looseObject == null || looseObject.Data == null)
            return false;

        if (looseObject.GetComponent<PlateContainer>() != null)
            return false;

        Color? tint = looseObject.HasRuntimeTint ? looseObject.RuntimeTint : null;
        if (!TryAddObject(looseObject.Data, tint, looseObject.RuntimeMaterial))
            return false;

        Destroy(looseObject.gameObject);
        return true;
    }

    public bool TryAddObject(ObjectData objectData)
    {
        return TryAddObject(objectData, null);
    }

    public bool TryAddObject(ObjectData objectData, Color? tint)
    {
        return TryAddObject(objectData, tint, null);
    }

    public bool TryAddObject(ObjectData objectData, Color? tint, Material material)
    {
        if (objectData == null)
            return false;

        if (IsFull)
        {
            Log($"{name}: plate is full.");
            return false;
        }

        platedObjects.Add(objectData);
        platedTints.Add(tint);
        platedMaterials.Add(material);
        Log($"{objectData.DisplayName} was plated. Total: {platedObjects.Count}");
        RefreshContentVisuals();

        return true;
    }

    public bool ContainsObject(ObjectData objectData)
    {
        return objectData != null && platedObjects.Contains(objectData);
    }

    public List<ObjectData> GetContentsCopy()
    {
        return new List<ObjectData>(platedObjects);
    }

    public void ClearPlate()
    {
        platedObjects.Clear();
        platedTints.Clear();
        platedMaterials.Clear();
        RefreshContentVisuals();
    }

    private void RefreshContentVisuals()
    {
        for (int i = contentVisuals.Count - 1; i >= 0; i--)
        {
            if (contentVisuals[i] != null)
                Destroy(contentVisuals[i]);
        }

        contentVisuals.Clear();

        if (contentVisualRoot == null)
            contentVisualRoot = transform;

        for (int i = 0; i < platedObjects.Count; i++)
        {
            ObjectData platedObject = platedObjects[i];

            if (platedObject == null || platedObject.Prefab == null)
                continue;

            GameObject visual = Instantiate(platedObject.Prefab, contentVisualRoot);
            visual.name = $"Plate_{platedObject.DisplayName}";
            visual.transform.localPosition = contentVisualLocalOffset;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * contentVisualScale;
            RecipeVisualUtility.DisableGameplayComponents(visual);
            ApplyMaterial(visual, i < platedMaterials.Count ? platedMaterials[i] : null);
            ApplyTint(visual, i < platedTints.Count ? platedTints[i] : null);
            contentVisuals.Add(visual);
        }
    }

    private static void ApplyMaterial(GameObject visual, Material material)
    {
        if (material == null || visual == null)
            return;

        foreach (Renderer targetRenderer in visual.GetComponentsInChildren<Renderer>())
            targetRenderer.material = material;
    }

    private static void ApplyTint(GameObject visual, Color? tint)
    {
        if (!tint.HasValue || visual == null)
            return;

        foreach (Renderer targetRenderer in visual.GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in targetRenderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", tint.Value);
                else if (material.HasProperty("_Color"))
                    material.color = tint.Value;
            }
        }
    }

    private void LogContents()
    {
        if (!showDebugLogs)
            return;

        if (platedObjects.Count == 0)
        {
            Debug.Log($"{name}: plate is empty.");
            return;
        }

        StringBuilder contents = new StringBuilder();

        for (int i = 0; i < platedObjects.Count; i++)
        {
            ObjectData platedObject = platedObjects[i];
            contents.Append(platedObject != null ? platedObject.DisplayName : "Null object");

            if (i < platedObjects.Count - 1)
                contents.Append(", ");
        }

        Debug.Log($"{name} contains: {contents}");
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log(message);
    }

    private void OnValidate()
    {
        maxObjects = Mathf.Max(1, maxObjects);
    }
}


using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlateContainer : MonoBehaviour, IInteractable
{
    [SerializeField] private int maxItems = 1;
    [SerializeField] private Transform contentVisualRoot;
    [SerializeField] private Vector3 contentVisualLocalOffset = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private float contentVisualScale = 0.25f;
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> platedItems = new List<ItemData>();
    private readonly List<Color?> platedTints = new List<Color?>();
    private readonly List<Material> platedMaterials = new List<Material>();
    private readonly List<GameObject> contentVisuals = new List<GameObject>();

    public IReadOnlyList<ItemData> PlatedItems => platedItems;
    public int ItemCount => platedItems.Count;
    public bool IsFull => platedItems.Count >= maxItems;
    public bool IsEmpty => platedItems.Count == 0;

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        ItemHolder holder = player.ItemHolder;

        if (holder.IsEmpty())
        {
            LogContents();
            return;
        }

        Objeto heldItem = holder.CurrentObjeto;

        if (heldItem == null || heldItem.gameObject == gameObject)
        {
            LogContents();
            return;
        }

        TryAddHeldItem(holder);
    }

    public bool TryAddHeldItem(ItemHolder holder)
    {
        if (holder == null || holder.IsEmpty())
            return false;

        Objeto heldItem = holder.CurrentObjeto;

        if (heldItem == null || heldItem.Data == null)
            return false;

        if (heldItem.GetComponent<PlateContainer>() != null)
            return false;

        Color? tint = heldItem.HasRuntimeTint ? heldItem.RuntimeTint : null;
        if (!TryAddItem(heldItem.Data, tint, heldItem.RuntimeMaterial))
            return false;

        Objeto removedItem = holder.RemoveObjeto();

        if (removedItem != null)
            Destroy(removedItem.gameObject);

        return true;
    }

    public bool TryAddLooseItem(Objeto item)
    {
        if (item == null || item.Data == null)
            return false;

        if (item.GetComponent<PlateContainer>() != null)
            return false;

        Color? tint = item.HasRuntimeTint ? item.RuntimeTint : null;
        if (!TryAddItem(item.Data, tint, item.RuntimeMaterial))
            return false;

        Destroy(item.gameObject);
        return true;
    }

    public bool TryAddItem(ItemData itemData)
    {
        return TryAddItem(itemData, null);
    }

    public bool TryAddItem(ItemData itemData, Color? tint)
    {
        return TryAddItem(itemData, tint, null);
    }

    public bool TryAddItem(ItemData itemData, Color? tint, Material material)
    {
        if (itemData == null)
            return false;

        if (IsFull)
        {
            Log($"{name}: o prato esta cheio.");
            return false;
        }

        platedItems.Add(itemData);
        platedTints.Add(tint);
        platedMaterials.Add(material);
        Log($"{itemData.DisplayName} foi colocado no prato. Total: {platedItems.Count}");
        RefreshContentVisuals();

        return true;
    }

    public bool ContainsItem(ItemData itemData)
    {
        return itemData != null && platedItems.Contains(itemData);
    }

    public List<ItemData> GetContentsCopy()
    {
        return new List<ItemData>(platedItems);
    }

    public void ClearPlate()
    {
        platedItems.Clear();
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

        for (int i = 0; i < platedItems.Count; i++)
        {
            ItemData item = platedItems[i];

            if (item == null || item.Prefab == null)
                continue;

            GameObject visual = Instantiate(item.Prefab, contentVisualRoot);
            visual.name = $"Prato_{item.DisplayName}";
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

        if (platedItems.Count == 0)
        {
            Debug.Log($"{name}: o prato esta vazio.");
            return;
        }

        StringBuilder contents = new StringBuilder();

        for (int i = 0; i < platedItems.Count; i++)
        {
            ItemData item = platedItems[i];
            contents.Append(item != null ? item.DisplayName : "Item nulo");

            if (i < platedItems.Count - 1)
                contents.Append(", ");
        }

        Debug.Log($"{name} contem: {contents}");
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log(message);
    }

    private void OnValidate()
    {
        maxItems = Mathf.Max(1, maxItems);
    }
}

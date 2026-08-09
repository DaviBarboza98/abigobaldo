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

        if (!TryAddItem(heldItem.Data))
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

        if (!TryAddItem(item.Data))
            return false;

        Destroy(item.gameObject);
        return true;
    }

    public bool TryAddItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        if (IsFull)
        {
            Log($"{name}: o prato esta cheio.");
            return false;
        }

        platedItems.Add(itemData);
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
            DisableGameplayComponents(visual);
            contentVisuals.Add(visual);
        }
    }

    private static void DisableGameplayComponents(GameObject visual)
    {
        foreach (Objeto item in visual.GetComponentsInChildren<Objeto>())
            item.enabled = false;

        foreach (RecipeContainer container in visual.GetComponentsInChildren<RecipeContainer>())
            container.enabled = false;

        foreach (PlateContainer plate in visual.GetComponentsInChildren<PlateContainer>())
            plate.enabled = false;

        foreach (Collider collider in visual.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>())
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
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

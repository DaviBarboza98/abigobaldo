using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : MonoBehaviour, IInteractable
{
    [Header("Container")]
    [SerializeField] private ContainerType containerType;

    [Header("Capacidade")]
    [SerializeField] private int maxItems = 5;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> storedItems = new();

    public ContainerType Type => containerType;

    public IReadOnlyList<ItemData> StoredItems =>
        storedItems;

    public int ItemCount => storedItems.Count;

    public bool IsEmpty =>
        storedItems.Count == 0;

    public bool IsFull =>
        storedItems.Count >= maxItems;

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;

        ItemHolder holder = player.ItemHolder;

        if (holder == null)
            return;

        if (holder.IsEmpty())
        {
            LogContents();
            return;
        }

        TryStoreHeldItem(holder);
    }

    public bool TryStoreHeldItem(ItemHolder holder)
    {
        if (holder == null)
            return false;

        if (holder.IsEmpty())
            return false;

        if (IsFull)
        {
            if (showDebugLogs)
            {
                Debug.Log(
                    $"{name}: o container está cheio."
                );
            }

            return false;
        }

        Item heldItem = holder.CurrentItem;

        if (heldItem == null)
            return false;

        if (heldItem.Data == null)
        {
            Debug.LogWarning(
                $"{heldItem.name} não possui ItemData."
            );

            return false;
        }

        Item removedItem = holder.RemoveItem();

        if (removedItem == null)
            return false;

        ItemData storedData = removedItem.Data;

        storedItems.Add(storedData);

        if (showDebugLogs)
        {
            Debug.Log(
                $"{storedData.DisplayName} foi colocado em " +
                $"{containerType}. Total: {storedItems.Count}"
            );
        }

        Destroy(removedItem.gameObject);

        OnContentsChanged();

        return true;
    }

    private void OnContentsChanged()
    {
        // Mais tarde, esse método chamará
        // o sistema de receitas.

        // Exemplo futuro:
        // recipeProcessor.CheckRecipe(this);
    }

    public bool ContainsItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        return storedItems.Contains(itemData);
    }

    public int CountItem(ItemData itemData)
    {
        if (itemData == null)
            return 0;

        int amount = 0;

        foreach (ItemData storedItem in storedItems)
        {
            if (storedItem == itemData)
                amount++;
        }

        return amount;
    }

    public bool RemoveItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        bool removed = storedItems.Remove(itemData);

        if (removed)
            OnContentsChanged();

        return removed;
    }

    public void ClearContainer()
    {
        storedItems.Clear();

        OnContentsChanged();

        if (showDebugLogs)
        {
            Debug.Log(
                $"{name}: todos os itens foram removidos."
            );
        }
    }

    public List<ItemData> GetContentsCopy()
    {
        return new List<ItemData>(storedItems);
    }

    private void LogContents()
    {
        if (!showDebugLogs)
            return;

        if (storedItems.Count == 0)
        {
            Debug.Log(
                $"{name}: o container está vazio."
            );

            return;
        }

        string contents = "";

        for (int i = 0; i < storedItems.Count; i++)
        {
            ItemData item = storedItems[i];

            contents += item != null
                ? item.DisplayName
                : "Item nulo";

            if (i < storedItems.Count - 1)
                contents += ", ";
        }

        Debug.Log(
            $"{name} contém: {contents}"
        );
    }
}
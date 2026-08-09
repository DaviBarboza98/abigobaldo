using UnityEngine;

public static class ObjetoDeliveryUtility
{
    public static bool TryDeliverToHolder(ItemData itemData, ItemHolder holder)
    {
        return TryDeliverToHolder(itemData, holder, itemData != null ? itemData.CookState : ItemCookState.Cru, null, null);
    }

    public static bool TryDeliverToHolder(ItemData itemData, ItemHolder holder, ItemCookState cookState, Color? tint)
    {
        return TryDeliverToHolder(itemData, holder, cookState, tint, null);
    }

    public static bool TryDeliverToHolder(ItemData itemData, ItemHolder holder, ItemCookState cookState, Color? tint, Material material)
    {
        if (itemData == null || itemData.Prefab == null || holder == null || !holder.IsEmpty())
            return false;

        GameObject instance = Object.Instantiate(itemData.Prefab, holder.transform.position, holder.transform.rotation);
        Objeto objeto = instance.GetComponent<Objeto>();

        if (objeto != null)
        {
            objeto.Configure(itemData, objeto.CanBeHeld, objeto.CanBeThrown);
            objeto.SetRuntimeCookVisual(cookState, tint, material);
        }

        if (objeto != null && holder.TryPickUp(objeto))
            return true;

        Object.Destroy(instance);
        return false;
    }
}

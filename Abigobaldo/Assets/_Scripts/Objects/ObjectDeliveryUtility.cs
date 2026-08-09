using UnityEngine;

public static class ObjectDeliveryUtility
{
    public static bool TryDeliverToHolder(ObjectData itemData, Holder holder)
    {
        return TryDeliverToHolder(itemData, holder, itemData != null ? itemData.CookState : ObjectCookState.Raw, null, null);
    }

    public static bool TryDeliverToHolder(ObjectData itemData, Holder holder, ObjectCookState cookState, Color? tint)
    {
        return TryDeliverToHolder(itemData, holder, cookState, tint, null);
    }

    public static bool TryDeliverToHolder(ObjectData itemData, Holder holder, ObjectCookState cookState, Color? tint, Material material)
    {
        if (itemData == null || itemData.Prefab == null || holder == null || !holder.IsEmpty())
            return false;

        GameObject instance = Object.Instantiate(itemData.Prefab, holder.transform.position, holder.transform.rotation);
        HoldableObject objeto = instance.GetComponent<HoldableObject>();

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


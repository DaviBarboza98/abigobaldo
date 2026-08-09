using UnityEngine;

public static class ObjetoDeliveryUtility
{
    public static bool TryDeliverToHolder(ItemData itemData, ItemHolder holder)
    {
        if (itemData == null || itemData.Prefab == null || holder == null || !holder.IsEmpty())
            return false;

        GameObject instance = Object.Instantiate(itemData.Prefab, holder.transform.position, holder.transform.rotation);
        Objeto objeto = instance.GetComponent<Objeto>();

        if (objeto != null && holder.TryPickUp(objeto))
            return true;

        Object.Destroy(instance);
        return false;
    }
}

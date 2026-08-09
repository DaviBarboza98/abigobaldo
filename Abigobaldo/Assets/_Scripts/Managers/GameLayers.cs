using UnityEngine;

public static class GameLayers
{
    public const string Player = "Player";
    public const string Interactable = "Interactable";
    public const string HoldableObject = "HoldableObject";
    public const string Container = "Container";
    public const string Door = "Door";
    public const string Spawner = "Spawner";
    public const string HomeSlot = "HomeSlot";

    public static LayerMask InteractionMask => GetMaskOrEverything(
        Interactable,
        HoldableObject,
        Container,
        Door,
        Spawner,
        HomeSlot
    );

    public static LayerMask PhysicsObjectCollisionMask => GetMaskOrEverything(
        "Default",
        Interactable,
        HoldableObject,
        Container,
        Door,
        HomeSlot
    );

    public static void SetLayerRecursivelyIfDefault(GameObject target, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);

        if (target == null || layer < 0)
            return;

        SetLayerRecursivelyIfDefault(target.transform, layer);
    }

    private static void SetLayerRecursivelyIfDefault(Transform target, int layer)
    {
        if (target.gameObject.layer == 0)
            target.gameObject.layer = layer;

        foreach (Transform child in target)
            SetLayerRecursivelyIfDefault(child, layer);
    }

    private static LayerMask GetMaskOrEverything(params string[] layerNames)
    {
        int mask = LayerMask.GetMask(layerNames);
        return mask != 0 ? mask : ~0;
    }
}


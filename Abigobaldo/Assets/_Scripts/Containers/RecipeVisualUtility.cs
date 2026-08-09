using UnityEngine;

public static class RecipeVisualUtility
{
    public static void DisableGameplayComponents(GameObject visual)
    {
        DisableGameplayComponents(visual, true);
    }

    public static void DisableGameplayComponents(GameObject visual, bool disablePhysics)
    {
        foreach (HoldableObject objeto in visual.GetComponentsInChildren<HoldableObject>())
            objeto.enabled = false;

        foreach (MonoBehaviour behaviour in visual.GetComponentsInChildren<MonoBehaviour>())
        {
            if (behaviour is IRecipeStation || behaviour is PlateContainer || behaviour is BlenderCup)
                behaviour.enabled = false;
        }

        if (disablePhysics)
        {
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>())
                collider.enabled = false;
        }

        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>())
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = disablePhysics;
            body.detectCollisions = !disablePhysics;
        }
    }
}



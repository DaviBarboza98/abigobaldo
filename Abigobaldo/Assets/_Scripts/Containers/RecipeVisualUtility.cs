using UnityEngine;

public static class RecipeVisualUtility
{
    public static void DisableGameplayComponents(GameObject visual)
    {
        foreach (Objeto objeto in visual.GetComponentsInChildren<Objeto>())
            objeto.enabled = false;

        foreach (MonoBehaviour behaviour in visual.GetComponentsInChildren<MonoBehaviour>())
        {
            if (behaviour is IRecipeStation)
                behaviour.enabled = false;
        }

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
}

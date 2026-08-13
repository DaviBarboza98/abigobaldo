using UnityEngine;

namespace Abigobaldo.Game
{
    public sealed class RotationTransform : MonoBehaviour, IHeldRotationReceiver
    {
        [SerializeField] private HoldableObject outputPrefab;
        [SerializeField] private FoodState requiredState = FoodState.AlmostReady;
        [SerializeField] private float requiredRotationDegrees = 720f;

        private float accumulatedRotation;

        public HoldableObject AddRotation(float degrees)
        {
            RecipeProgress progress = GetComponent<RecipeProgress>();

            if (outputPrefab == null || progress == null || progress.State != requiredState)
                return null;

            accumulatedRotation += Mathf.Max(0f, degrees);
            return accumulatedRotation >= requiredRotationDegrees ? outputPrefab : null;
        }

        private void OnValidate()
        {
            requiredRotationDegrees = Mathf.Max(0f, requiredRotationDegrees);
        }
    }
}

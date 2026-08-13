using UnityEngine;

namespace Abigobaldo.Game
{
    public sealed class RotationTransform : MonoBehaviour, IHeldRotationReceiver
    {
        [SerializeField] private HoldableObject outputPrefab;
        [SerializeField] private FoodState requiredState = FoodState.AlmostReady;
        [Tooltip("Seconds of active mouse movement while R is held. Rotation speed does not accelerate the transformation.")]
        [SerializeField] private float requiredMixDuration = 5f;

        private float accumulatedMixTime;

        public float MixProgress => requiredMixDuration <= 0f
            ? 1f
            : Mathf.Clamp01(accumulatedMixTime / requiredMixDuration);

        public HoldableObject AddRotationTime(float activeDeltaTime)
        {
            RecipeProgress progress = GetComponent<RecipeProgress>();

            if (outputPrefab == null || progress == null || progress.State != requiredState)
            {
                accumulatedMixTime = 0f;
                return null;
            }

            accumulatedMixTime += Mathf.Max(0f, activeDeltaTime);
            return accumulatedMixTime >= requiredMixDuration ? outputPrefab : null;
        }

        private void OnValidate()
        {
            requiredMixDuration = Mathf.Max(0.1f, requiredMixDuration);
        }
    }
}

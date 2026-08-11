using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoCookableItem : MonoBehaviour
    {
        [SerializeField] private DemoFoodState state = DemoFoodState.Raw;
        [SerializeField] private float cookedTime;
        [SerializeField] private bool canBePlated = true;
        [SerializeField] private DemoHoldableObject handMixOutputPrefab;
        [SerializeField] private DemoFoodState handMixRequiredState = DemoFoodState.AlmostReady;
        [SerializeField] private float handMixRequiredIntensity = 80f;

        private GameObject visualOverride;
        private float handMixIntensity;

        public DemoFoodState State => state;
        public float CookedTime => cookedTime;
        public bool CanBePlated => canBePlated && state >= DemoFoodState.Ready;

        public bool AdvanceCooking(float deltaTime, DemoRecipeData recipe)
        {
            if (recipe == null || !recipe.UsesHeat || state == DemoFoodState.Carbonized)
                return false;

            cookedTime += Mathf.Max(0f, deltaTime);
            DemoFoodState previousState = state;
            state = GetStateForTime(cookedTime, recipe);

            if (state != previousState)
                ApplyRecipeVisual(recipe);

            return state == DemoFoodState.Carbonized && previousState != DemoFoodState.Carbonized;
        }

        public void ConfigureFromRecipe(DemoRecipeData recipe, bool resetProgress)
        {
            if (resetProgress)
            {
                cookedTime = 0f;
                state = DemoFoodState.Raw;
                handMixIntensity = 0f;
            }

            handMixOutputPrefab = recipe.HandMixOutputPrefab;
            handMixRequiredState = recipe.HandMixRequiredState;
            handMixRequiredIntensity = recipe.HandMixRequiredIntensity;
            ApplyRecipeVisual(recipe);
        }

        public DemoHoldableObject AddHandMix(float amount)
        {
            if (handMixOutputPrefab == null || state != handMixRequiredState)
                return null;

            handMixIntensity += Mathf.Max(0f, amount);
            return handMixIntensity >= handMixRequiredIntensity ? handMixOutputPrefab : null;
        }

        public void ApplyRecipeVisual(DemoRecipeData recipe)
        {
            if (recipe == null || !recipe.TryGetStateVisual(state, out DemoRecipeData.StateVisual visual))
                return;

            if (visual.material != null)
                ApplyMaterial(visual.material);

            if (visual.modelPrefab != null)
                ReplaceVisual(visual.modelPrefab);
        }

        private static DemoFoodState GetStateForTime(float time, DemoRecipeData recipe)
        {
            if (time >= recipe.CarbonizedTime)
                return recipe.CanBurn ? DemoFoodState.Carbonized : DemoFoodState.Ready;

            if (time >= recipe.BurnedTime)
                return recipe.CanBurn ? DemoFoodState.Burned : DemoFoodState.Ready;

            if (time >= recipe.OverdoneTime)
                return recipe.CanBurn ? DemoFoodState.Overdone : DemoFoodState.Ready;

            if (time >= recipe.ReadyTime)
                return DemoFoodState.Ready;

            if (time >= recipe.AlmostReadyTime)
                return DemoFoodState.AlmostReady;

            return DemoFoodState.Raw;
        }

        private void ApplyMaterial(Material material)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = material;
        }

        private void ReplaceVisual(GameObject prefab)
        {
            if (visualOverride != null)
                Destroy(visualOverride);

            visualOverride = Instantiate(prefab, transform);
            visualOverride.name = prefab.name;
            visualOverride.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}

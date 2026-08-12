using UnityEngine;

namespace Abigobaldo.Game
{
    public class CookableItem : MonoBehaviour
    {
        [SerializeField] private FoodState state = FoodState.Raw;
        [SerializeField] private float cookedTime;
        [SerializeField] private bool canBePlated = true;
        [SerializeField] private HoldableObject handMixOutputPrefab;
        [SerializeField] private FoodState handMixRequiredState = FoodState.AlmostReady;
        [SerializeField] private float handMixRequiredIntensity = 80f;

        private GameObject visualOverride;
        private float handMixIntensity;

        public FoodState State => state;
        public float CookedTime => cookedTime;
        public bool CanBePlated => canBePlated && state >= FoodState.Ready;

        public bool AdvanceCooking(float deltaTime, RecipeData recipe)
        {
            if (recipe == null || !recipe.UsesHeat || state == FoodState.Carbonized)
                return false;

            cookedTime += Mathf.Max(0f, deltaTime);
            FoodState previousState = state;
            state = GetStateForTime(cookedTime, recipe);

            if (state != previousState)
                ApplyRecipeVisual(recipe);

            return state == FoodState.Carbonized && previousState != FoodState.Carbonized;
        }

        public void ConfigureFromRecipe(RecipeData recipe, bool resetProgress)
        {
            if (resetProgress)
            {
                cookedTime = 0f;
                state = FoodState.Raw;
                handMixIntensity = 0f;
            }

            handMixOutputPrefab = recipe.HandMixOutputPrefab;
            handMixRequiredState = recipe.HandMixRequiredState;
            handMixRequiredIntensity = recipe.HandMixRequiredIntensity;
            ApplyRecipeVisual(recipe);
        }

        public HoldableObject AddHandMix(float amount)
        {
            if (handMixOutputPrefab == null || state != handMixRequiredState)
                return null;

            handMixIntensity += Mathf.Max(0f, amount);
            return handMixIntensity >= handMixRequiredIntensity ? handMixOutputPrefab : null;
        }

        public void ApplyRecipeVisual(RecipeData recipe)
        {
            if (recipe == null || !recipe.TryGetStateVisual(state, out RecipeData.StateVisual visual))
                return;

            if (visual.material != null)
                ApplyMaterial(visual.material);

            if (visual.modelPrefab != null)
                ReplaceVisual(visual.modelPrefab);
        }

        private static FoodState GetStateForTime(float time, RecipeData recipe)
        {
            if (time >= recipe.CarbonizedTime)
                return recipe.CanBurn ? FoodState.Carbonized : FoodState.Ready;

            if (time >= recipe.BurnedTime)
                return recipe.CanBurn ? FoodState.Burned : FoodState.Ready;

            if (time >= recipe.OverdoneTime)
                return recipe.CanBurn ? FoodState.Overdone : FoodState.Ready;

            if (time >= recipe.ReadyTime)
                return FoodState.Ready;

            if (time >= recipe.AlmostReadyTime)
                return FoodState.AlmostReady;

            return FoodState.Raw;
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

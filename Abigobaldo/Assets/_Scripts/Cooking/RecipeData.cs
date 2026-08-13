using System.Collections.Generic;
using UnityEngine;

namespace Abigobaldo.Game
{
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Abigobaldo/Recipe")]
    public sealed class RecipeData : ScriptableObject
    {
        [System.Serializable]
        public struct Ingredient
        {
            public ObjectDefinition definition;
            [Min(1)] public int amount;
        }

        [System.Serializable]
        public struct StateAppearance
        {
            [Tooltip("Optional material applied when the food reaches this state.")]
            public Material material;
            [Tooltip("Optional model used when the food reaches this state.")]
            public GameObject modelPrefab;
        }

        [Header("Match")]
        [SerializeField] private RecipeStationType requiredStation;
        [SerializeField] private Ingredient[] ingredients;

        [Header("Transformation")]
        [Tooltip("Optional object used while the recipe is running. If empty, a single input object is kept.")]
        [SerializeField] private GameObject inProgressPrefab;
        [Tooltip("Optional replacement created when the recipe becomes ready.")]
        [SerializeField] private GameObject resultPrefab;

        [Header("Process")]
        [SerializeField] private float processingTime = 5f;
        [Tooltip("Enables the six heated-food states. Disable for finite processes such as blending.")]
        [SerializeField] private bool usesHeat;

        [Header("Heated State Times")]
        [Tooltip("Elapsed time when Raw becomes Almost Ready.")]
        [SerializeField] private float almostReadyTime = 5f;
        [Tooltip("Elapsed time when Ready becomes Overdone.")]
        [SerializeField] private float overdoneTime = 15f;
        [Tooltip("Elapsed time when Overdone becomes Burned.")]
        [SerializeField] private float burnedTime = 20f;
        [Tooltip("Elapsed time when the object is obligatorily replaced by the Recipe Book charcoal prefab.")]
        [SerializeField] private float carbonizedTime = 25f;

        [Header("Heated State Appearances")]
        [SerializeField] private StateAppearance rawAppearance;
        [SerializeField] private StateAppearance almostReadyAppearance;
        [SerializeField] private StateAppearance readyAppearance;
        [SerializeField] private StateAppearance overdoneAppearance;
        [SerializeField] private StateAppearance burnedAppearance;

        [Header("Byproducts On Start")]
        [SerializeField] private GameObject[] byproducts;

        public RecipeStationType RequiredStation => requiredStation;
        public IReadOnlyList<Ingredient> Ingredients => ingredients;
        public GameObject InProgressPrefab => inProgressPrefab;
        public GameObject ResultPrefab => resultPrefab;
        public float ProcessingTime => processingTime;
        public bool UsesHeat => usesHeat;
        public IReadOnlyList<GameObject> Byproducts => byproducts;
        public int RequiredIngredientCount => GetRequiredIngredientCount();

        public FoodState EvaluateState(float elapsedTime)
        {
            if (!usesHeat)
                return elapsedTime >= processingTime ? FoodState.Ready : FoodState.Raw;

            if (elapsedTime >= carbonizedTime)
                return FoodState.Carbonized;

            if (elapsedTime >= burnedTime)
                return FoodState.Burned;

            if (elapsedTime >= overdoneTime)
                return FoodState.Overdone;

            if (elapsedTime >= processingTime)
                return FoodState.Ready;

            if (elapsedTime >= almostReadyTime)
                return FoodState.AlmostReady;

            return FoodState.Raw;
        }

        public bool TryGetAppearance(FoodState state, out StateAppearance appearance)
        {
            switch (state)
            {
                case FoodState.Raw:
                    appearance = rawAppearance;
                    return true;
                case FoodState.AlmostReady:
                    appearance = almostReadyAppearance;
                    return true;
                case FoodState.Ready:
                    appearance = readyAppearance;
                    return true;
                case FoodState.Overdone:
                    appearance = overdoneAppearance;
                    return true;
                case FoodState.Burned:
                    appearance = burnedAppearance;
                    return true;
                default:
                    appearance = default;
                    return false;
            }
        }

        public bool Matches(RecipeStationType station, IReadOnlyList<ObjectDefinition> contents)
        {
            return requiredStation == station
                && contents != null
                && contents.Count == RequiredIngredientCount
                && ContainsOnlyRequiredAmounts(contents);
        }

        public bool CanAccept(RecipeStationType station, IReadOnlyList<ObjectDefinition> contents)
        {
            return requiredStation == station
                && contents != null
                && contents.Count <= RequiredIngredientCount
                && ContainsOnlyRequiredAmounts(contents);
        }

        private bool ContainsOnlyRequiredAmounts(IReadOnlyList<ObjectDefinition> contents)
        {
            if (ingredients == null || ingredients.Length == 0)
                return false;

            foreach (ObjectDefinition content in contents)
            {
                if (content == null || Count(contents, content) > GetRequiredAmount(content))
                    return false;
            }

            return true;
        }

        private int GetRequiredIngredientCount()
        {
            if (ingredients == null)
                return 0;

            int total = 0;

            foreach (Ingredient ingredient in ingredients)
                total += Mathf.Max(1, ingredient.amount);

            return total;
        }

        private int GetRequiredAmount(ObjectDefinition definition)
        {
            if (ingredients == null)
                return 0;

            int total = 0;

            foreach (Ingredient ingredient in ingredients)
            {
                if (ingredient.definition == definition)
                    total += Mathf.Max(1, ingredient.amount);
            }

            return total;
        }

        private static int Count(IReadOnlyList<ObjectDefinition> contents, ObjectDefinition definition)
        {
            int count = 0;

            foreach (ObjectDefinition content in contents)
            {
                if (content == definition)
                    count++;
            }

            return count;
        }

        private void OnValidate()
        {
            processingTime = Mathf.Max(0f, processingTime);
            almostReadyTime = Mathf.Clamp(almostReadyTime, 0f, processingTime);
            overdoneTime = Mathf.Max(processingTime, overdoneTime);
            burnedTime = Mathf.Max(overdoneTime, burnedTime);
            carbonizedTime = Mathf.Max(burnedTime, carbonizedTime);

            if (ingredients == null)
                return;

            for (int i = 0; i < ingredients.Length; i++)
            {
                Ingredient ingredient = ingredients[i];
                ingredient.amount = Mathf.Max(1, ingredient.amount);
                ingredients[i] = ingredient;
            }
        }
    }
}

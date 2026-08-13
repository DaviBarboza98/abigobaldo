using UnityEngine;

namespace Abigobaldo.Game
{
    public sealed class RecipeProgress : MonoBehaviour
    {
        [SerializeField] private RecipeData recipe;
        [SerializeField] private float elapsedTime;
        [SerializeField] private FoodState state = FoodState.Raw;
        [SerializeField] private bool resultApplied;
        [SerializeField] private bool carbonizedOutputApplied;

        private GameObject modelOverride;
        private GameObject activeModelPrefab;
        private Renderer[] originalRenderers;
        private bool[] originalRendererStates;

        public RecipeData Recipe => recipe;
        public float ElapsedTime => elapsedTime;
        public FoodState State => state;
        public bool IsReady => state >= FoodState.Ready;
        public bool ResultApplied => resultApplied;
        public bool CarbonizedOutputApplied => carbonizedOutputApplied;
        public GameObject ActiveModelPrefab => activeModelPrefab;

        public void Configure(RecipeData newRecipe, bool resetProgress)
        {
            if (newRecipe == null)
                return;

            if (resetProgress || recipe != newRecipe)
            {
                elapsedTime = 0f;
                state = FoodState.Raw;
                resultApplied = false;
                carbonizedOutputApplied = false;
                ResetModelOverride();
            }

            recipe = newRecipe;
            state = recipe.EvaluateState(elapsedTime);
            ApplyOwnVisual();
        }

        public bool Advance(float deltaTime, out bool becameReady)
        {
            becameReady = false;

            if (recipe == null || state == FoodState.Carbonized)
                return false;

            FoodState previousState = state;
            elapsedTime += Mathf.Max(0f, deltaTime);
            state = recipe.EvaluateState(elapsedTime);
            becameReady = previousState < FoodState.Ready && state >= FoodState.Ready;

            if (state == previousState)
                return false;

            ApplyOwnVisual();
            return true;
        }

        public void CopyFrom(RecipeProgress source)
        {
            if (source == null)
                return;

            recipe = source.recipe;
            elapsedTime = source.elapsedTime;
            state = source.state;
            resultApplied = source.resultApplied;
            carbonizedOutputApplied = source.carbonizedOutputApplied;
            ResetModelOverride();
            ApplyOwnVisual();
        }

        public void MarkResultApplied()
        {
            resultApplied = true;
        }

        public void MarkCarbonizedOutputApplied()
        {
            carbonizedOutputApplied = true;
        }

        public void ApplyVisualTo(GameObject target)
        {
            if (target == null || recipe == null)
                return;

            if (!recipe.TryGetAppearance(state, out RecipeData.StateAppearance appearance))
                return;

            if (appearance.material == null)
                return;

            foreach (Renderer targetRenderer in target.GetComponentsInChildren<Renderer>(true))
                targetRenderer.sharedMaterial = appearance.material;
        }

        private void ApplyOwnVisual()
        {
            if (recipe == null)
                return;

            if (!recipe.TryGetAppearance(state, out RecipeData.StateAppearance appearance))
                return;

            if (appearance.modelPrefab != null)
                SetModelOverride(appearance.modelPrefab);

            ApplyVisualTo(modelOverride != null ? modelOverride : gameObject);
        }

        private void SetModelOverride(GameObject prefab)
        {
            if (prefab == null || prefab == activeModelPrefab)
                return;

            if (originalRenderers == null)
            {
                originalRenderers = GetComponentsInChildren<Renderer>(true);
                originalRendererStates = new bool[originalRenderers.Length];

                for (int i = 0; i < originalRenderers.Length; i++)
                {
                    Renderer targetRenderer = originalRenderers[i];
                    originalRendererStates[i] = targetRenderer != null && targetRenderer.enabled;

                    if (targetRenderer != null)
                        targetRenderer.enabled = false;
                }
            }

            if (modelOverride != null)
                Destroy(modelOverride);

            activeModelPrefab = prefab;
            modelOverride = Instantiate(prefab, transform);
            modelOverride.name = prefab.name;
            modelOverride.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            modelOverride.transform.localScale = Vector3.one;
            RemoveGameplayComponents(modelOverride);
        }

        private void ResetModelOverride()
        {
            if (modelOverride != null)
                Destroy(modelOverride);

            modelOverride = null;
            activeModelPrefab = null;

            if (originalRenderers != null)
            {
                for (int i = 0; i < originalRenderers.Length; i++)
                {
                    if (originalRenderers[i] != null)
                        originalRenderers[i].enabled = originalRendererStates[i];
                }
            }

            originalRenderers = null;
            originalRendererStates = null;
        }

        private static void RemoveGameplayComponents(GameObject target)
        {
            foreach (RotationTransform rotationTransform in target.GetComponentsInChildren<RotationTransform>(true))
                DestroyComponent(rotationTransform);

            foreach (HoldableObject holdable in target.GetComponentsInChildren<HoldableObject>(true))
                DestroyComponent(holdable);

            foreach (ObjectIdentity identity in target.GetComponentsInChildren<ObjectIdentity>(true))
                DestroyComponent(identity);

            foreach (RecipeProgress progress in target.GetComponentsInChildren<RecipeProgress>(true))
                DestroyComponent(progress);

            foreach (Collider targetCollider in target.GetComponentsInChildren<Collider>(true))
                DestroyComponent(targetCollider);

            foreach (Rigidbody body in target.GetComponentsInChildren<Rigidbody>(true))
                DestroyComponent(body);
        }

        private static void DestroyComponent(Component component)
        {
            if (component == null)
                return;

            DestroyImmediate(component);
        }
    }
}

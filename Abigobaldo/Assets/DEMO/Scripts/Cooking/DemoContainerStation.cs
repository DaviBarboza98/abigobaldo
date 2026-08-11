using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoContainerStation : MonoBehaviour, IDemoInteractable
    {
        [SerializeField] private DemoContainerKind containerKind;
        [SerializeField] private Transform itemAnchor;
        [SerializeField] private Transform sideEffectSpawnRoot;
        [SerializeField] private DemoRecipeData[] recipes;
        [SerializeField] private float sideEffectSpacing = 0.12f;

        private DemoHoldableObject containedObject;
        private DemoRecipeData activeRecipe;
        private GameObject containedVisual;
        private Renderer[] hiddenContainedRenderers;

        private void Update()
        {
            UpdateContainedObject();
        }

        public void Interact(DemoPlayerInteractor player)
        {
            if (player == null || player.Holder == null)
                return;

            if (containedObject != null)
            {
                if (TryPlateContainedObject(player))
                    return;

                TryTakeContainedObject(player);
                return;
            }

            TryInsertHeldObject(player);
        }

        private void UpdateContainedObject()
        {
            if (containedObject == null || activeRecipe == null)
                return;

            if (activeRecipe.SpinsInContainer)
            {
                Transform target = containedVisual != null ? containedVisual.transform : containedObject.transform;
                target.Rotate(0f, 0f, activeRecipe.SpinSpeed * Time.deltaTime, Space.Self);
            }

            DemoCookableItem cookable = containedObject.GetComponent<DemoCookableItem>();

            if (cookable == null)
                return;

            bool carbonizedNow = cookable.AdvanceCooking(Time.deltaTime, activeRecipe);

            if (carbonizedNow && activeRecipe.CarbonizedTurnsIntoCharcoal && activeRecipe.CharcoalPrefab != null)
                ReplaceContainedObject(activeRecipe.CharcoalPrefab, null);
            else if (cookable.State == DemoFoodState.Ready && activeRecipe.OutputWhenReadyPrefab != null)
                ReplaceContainedObject(activeRecipe.OutputWhenReadyPrefab, null);
        }

        private bool TryInsertHeldObject(DemoPlayerInteractor player)
        {
            if (!player.Holder.TryGetHeldIdentity(out DemoObjectIdentity identity))
                return false;

            DemoRecipeData recipe = FindRecipe(identity.Kind);

            if (recipe == null)
                return false;

            activeRecipe = recipe;

            bool createdOutputObject = recipe.OutputOnInsertPrefab != null && identity.Kind == recipe.InputKind;

            if (createdOutputObject)
            {
                player.Holder.ConsumeHeldObject();
                containedObject = Instantiate(recipe.OutputOnInsertPrefab, GetAnchorPosition(), GetAnchorRotation());
                containedObject.name = recipe.OutputOnInsertPrefab.name;
            }
            else
            {
                containedObject = player.Holder.ReleaseHeldObject();
            }

            if (containedObject == null)
                return false;

            containedObject.PlaceInContainer(GetAnchor());
            ConfigureContainedObject(recipe, createdOutputObject);
            SpawnContainedVisual(recipe);
            containedObject.GetComponent<DemoCookableItem>()?.ApplyRecipeVisual(recipe);
            SpawnSideEffects(recipe);
            return true;
        }

        private bool TryTakeContainedObject(DemoPlayerInteractor player)
        {
            if (!player.Holder.IsEmpty || containedObject == null)
                return false;

            ClearContainedVisual();
            DemoHoldableObject target = containedObject;
            containedObject = null;
            activeRecipe = null;
            target.RemoveFromContainer();
            return player.Holder.TryPickUp(target);
        }

        private bool TryPlateContainedObject(DemoPlayerInteractor player)
        {
            if (containedObject == null || !player.Holder.TryGetHeldComponent(out DemoPlate plate))
                return false;

            DemoCookableItem cookable = containedObject.GetComponent<DemoCookableItem>();

            if (cookable != null && !cookable.CanBePlated)
                return false;

            DemoObjectIdentity identity = containedObject.GetComponent<DemoObjectIdentity>();

            if (identity == null || !plate.TrySetFoodFromObject(containedObject))
                return false;

            Destroy(containedObject.gameObject);
            containedObject = null;
            activeRecipe = null;
            ClearContainedVisual();
            return true;
        }

        private void ConfigureContainedObject(DemoRecipeData recipe, bool resetProgress)
        {
            DemoCookableItem cookable = containedObject.GetComponent<DemoCookableItem>();

            if (cookable == null && recipe != null && recipe.UsesHeat)
                cookable = containedObject.gameObject.AddComponent<DemoCookableItem>();

            if (cookable != null)
                cookable.ConfigureFromRecipe(recipe, resetProgress);
        }

        private void ReplaceContainedObject(DemoHoldableObject prefab, DemoRecipeData recipeForNewObject)
        {
            if (prefab == null || containedObject == null)
                return;

            Destroy(containedObject.gameObject);
            containedObject = Instantiate(prefab, GetAnchorPosition(), GetAnchorRotation());
            containedObject.name = prefab.name;
            containedObject.PlaceInContainer(GetAnchor());
            activeRecipe = recipeForNewObject;
            ClearContainedVisual();

            if (recipeForNewObject != null)
                ConfigureContainedObject(recipeForNewObject, true);
        }

        private DemoRecipeData FindRecipe(DemoObjectKind inputKind)
        {
            if (recipes == null)
                return null;

            foreach (DemoRecipeData recipe in recipes)
            {
                if (recipe != null && recipe.Matches(containerKind, inputKind))
                    return recipe;
            }

            return null;
        }

        private void SpawnContainedVisual(DemoRecipeData recipe)
        {
            ClearContainedVisual();

            if (recipe == null || recipe.ContainedVisualPrefab == null)
                return;

            hiddenContainedRenderers = containedObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in hiddenContainedRenderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }

            containedVisual = Instantiate(recipe.ContainedVisualPrefab, containedObject.transform);
            containedVisual.name = recipe.ContainedVisualPrefab.name;
            containedVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void ClearContainedVisual()
        {
            if (hiddenContainedRenderers != null)
            {
                foreach (Renderer renderer in hiddenContainedRenderers)
                {
                    if (renderer != null)
                        renderer.enabled = true;
                }
            }

            hiddenContainedRenderers = null;

            if (containedVisual != null)
                Destroy(containedVisual);

            containedVisual = null;
        }

        private void SpawnSideEffects(DemoRecipeData recipe)
        {
            DemoHoldableObject[] prefabs = recipe.SpawnedOnInsertPrefabs;

            if (prefabs == null || prefabs.Length == 0)
                return;

            Transform root = sideEffectSpawnRoot != null ? sideEffectSpawnRoot : transform;

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] == null)
                    continue;

                Vector3 offset = root.right * ((i - (prefabs.Length - 1) * 0.5f) * sideEffectSpacing);
                DemoHoldableObject sideEffect = Instantiate(prefabs[i], root.position + offset, root.rotation);
                sideEffect.name = prefabs[i].name;
                sideEffect.Drop();
            }
        }

        private Transform GetAnchor()
        {
            return itemAnchor != null ? itemAnchor : transform;
        }

        private Vector3 GetAnchorPosition()
        {
            return GetAnchor().position;
        }

        private Quaternion GetAnchorRotation()
        {
            return GetAnchor().rotation;
        }

        private void OnValidate()
        {
            sideEffectSpacing = Mathf.Max(0f, sideEffectSpacing);
        }
    }
}

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
        [SerializeField] private bool showContainedObject = true;
        [SerializeField] private bool logCooking = true;
        [SerializeField] private DemoStationParticles particles;
        [SerializeField] private bool createFallbackParticles = true;

        protected DemoHoldableObject containedObject;
        protected DemoRecipeData activeRecipe;
        private GameObject containedVisual;
        private Renderer[] hiddenContainedRenderers;
        private DemoFoodState lastLoggedState;

        private void Awake()
        {
            if (particles == null)
                particles = GetComponentInChildren<DemoStationParticles>(true);

            if (particles == null && createFallbackParticles && containerKind != DemoContainerKind.Blender)
                particles = DemoStationParticles.CreateDefault(GetAnchor(), "Particles");
        }

        private void Update()
        {
            UpdateContainedObject();
        }

        public virtual void Interact(DemoPlayerInteractor player)
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

        public bool HasContainedObject => containedObject != null;
        public DemoContainerKind ContainerKind => containerKind;

        protected virtual bool CanInsertObject(DemoPlayerInteractor player, DemoObjectIdentity identity, DemoRecipeData recipe)
        {
            return true;
        }

        protected virtual void OnContainedObjectInserted(DemoHoldableObject insertedObject, DemoRecipeData recipe)
        {
        }

        protected virtual void OnContainedObjectRemoved(DemoHoldableObject removedObject)
        {
        }

        protected virtual void OnContainedObjectReplaced(DemoHoldableObject previousObject, DemoHoldableObject newObject)
        {
        }

        private void UpdateContainedObject()
        {
            if (containedObject == null || activeRecipe == null)
            {
                if (particles != null)
                    particles.SetState(false, DemoFoodState.Raw);

                return;
            }

            if (activeRecipe.SpinsInContainer)
            {
                Transform target = containedVisual != null ? containedVisual.transform : containedObject.transform;
                target.Rotate(0f, 0f, activeRecipe.SpinSpeed * Time.deltaTime, Space.Self);
            }

            DemoCookableItem cookable = containedObject.GetComponent<DemoCookableItem>();

            if (cookable == null)
                return;

            bool carbonizedNow = cookable.AdvanceCooking(Time.deltaTime, activeRecipe);
            UpdateParticles(cookable.State);
            LogStateIfChanged(cookable);

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
            {
                if (logCooking)
                    Debug.Log($"{name}: {identity.Kind} nao combina com este container.", this);

                return false;
            }

            if (!CanInsertObject(player, identity, recipe))
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
            ApplyContainedVisibility();
            containedObject.GetComponent<DemoCookableItem>()?.ApplyRecipeVisual(recipe);
            SpawnSideEffects(recipe);
            OnContainedObjectInserted(containedObject, recipe);
            LogInserted(identity.Kind, containedObject);
            return true;
        }

        private bool TryTakeContainedObject(DemoPlayerInteractor player)
        {
            if (!player.Holder.IsEmpty || containedObject == null)
            {
                if (logCooking && containedObject != null)
                    Debug.Log($"{name}: mao precisa estar vazia para tirar o item.", this);

                return false;
            }

            ClearContainedVisual();
            DemoHoldableObject target = containedObject;
            containedObject = null;
            activeRecipe = null;
            target.RemoveFromContainer();
            RestoreContainedVisibility(target);
            OnContainedObjectRemoved(target);
            particles?.SetState(false, DemoFoodState.Raw);
            return player.Holder.TryPickUp(target);
        }

        private bool TryPlateContainedObject(DemoPlayerInteractor player)
        {
            if (containedObject == null || !player.Holder.TryGetHeldComponent(out DemoPlate plate))
                return false;

            DemoCookableItem cookable = containedObject.GetComponent<DemoCookableItem>();

            if (cookable != null && !cookable.CanBePlated)
            {
                if (logCooking)
                    Debug.Log($"{name}: {containedObject.name} ainda nao esta pronto para empratar ({cookable.State}).", this);

                return false;
            }

            DemoObjectIdentity identity = containedObject.GetComponent<DemoObjectIdentity>();

            if (identity == null || !plate.TrySetFoodFromObject(containedObject))
                return false;

            Destroy(containedObject.gameObject);
            containedObject = null;
            activeRecipe = null;
            ClearContainedVisual();
            particles?.SetState(false, DemoFoodState.Raw);
            Debug.Log($"{name}: item empratado.", this);
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

            DemoHoldableObject previousObject = containedObject;
            Destroy(previousObject.gameObject);
            containedObject = Instantiate(prefab, GetAnchorPosition(), GetAnchorRotation());
            containedObject.name = prefab.name;
            containedObject.PlaceInContainer(GetAnchor());
            RestoreContainedVisibility(containedObject);
            activeRecipe = recipeForNewObject;
            ClearContainedVisual();

            if (recipeForNewObject != null)
                ConfigureContainedObject(recipeForNewObject, true);

            ApplyContainedVisibility();
            OnContainedObjectReplaced(previousObject, containedObject);
            Debug.Log($"{name}: virou {containedObject.name}.", this);
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

        private void ApplyContainedVisibility()
        {
            if (showContainedObject)
                return;

            hiddenContainedRenderers = containedObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in hiddenContainedRenderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }

        private void RestoreContainedVisibility(DemoHoldableObject target)
        {
            if (target == null)
                return;

            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                if (renderer != null)
                    renderer.enabled = true;
            }
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

        private void UpdateParticles(DemoFoodState state)
        {
            if (particles == null || activeRecipe == null)
                return;

            particles.SetState(activeRecipe.UsesHeat, state);
        }

        private void LogInserted(DemoObjectKind inputKind, DemoHoldableObject outputObject)
        {
            if (!logCooking)
                return;

            DemoCookableItem cookable = outputObject.GetComponent<DemoCookableItem>();
            lastLoggedState = cookable != null ? cookable.State : DemoFoodState.Raw;
            string stateText = cookable != null ? $" estado {cookable.State}" : " sem timer";
            Debug.Log($"{name}: recebeu {inputKind} -> {outputObject.name}{stateText}.", this);
        }

        private void LogStateIfChanged(DemoCookableItem cookable)
        {
            if (!logCooking || cookable.State == lastLoggedState)
                return;

            lastLoggedState = cookable.State;
            Debug.Log($"{name}: {containedObject.name} chegou em {cookable.State} ({cookable.CookedTime:0.0}s).", this);
        }
    }
}

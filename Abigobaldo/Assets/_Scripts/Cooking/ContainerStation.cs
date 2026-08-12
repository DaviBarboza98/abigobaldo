using UnityEngine;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HoldableObject))]
    public abstract class ContainerStation : MonoBehaviour, IInteractable, IPickupInteractable
    {
        [SerializeField] private Transform sideEffectSpawnRoot;
        [SerializeField] private DemoRecipeBook recipeBook;
        [SerializeField] private float sideEffectSpacing = 0.12f;
        [SerializeField] private bool showContainedObject = true;
        [SerializeField] private bool logCooking = true;
        [SerializeField] private StationParticles particles;
        [SerializeField] private bool createFallbackParticles = true;

        protected HoldableObject containedObject;
        protected RecipeData activeRecipe;
        private GameObject containedVisual;
        private Renderer[] hiddenContainedRenderers;
        private FoodState lastLoggedState;
        private HoldableObject holdableObject;

        public bool HasContainedObject => containedObject != null;
        public bool IsHeld => holdableObject != null && holdableObject.IsHeld;

        protected virtual ObjectVisualTarget VisualTarget => ObjectVisualTarget.Default;

        private void Awake()
        {
            holdableObject = GetComponent<HoldableObject>();

            if (particles == null)
                particles = GetComponentInChildren<StationParticles>(true);

            if (particles == null && createFallbackParticles && VisualTarget != ObjectVisualTarget.Blender)
                particles = StationParticles.CreateDefault(GetAnchor(), "Particles");
        }

        private void Update()
        {
            UpdateContainedObject();
        }

        public virtual void Interact(PlayerInteractor player)
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

        public virtual void PickInteract(PlayerInteractor player)
        {
            if (player == null || player.Holder == null)
                return;

            HoldableObject target = GetOrCreateHoldableObject();

            if (target == null || !target.CanBeHeld)
                return;

            if (!player.Holder.IsEmpty && player.Holder.CurrentObject != target)
                player.Holder.Drop();

            player.Holder.TryPickUp(target);
        }

        protected virtual bool CanInsertObject(PlayerInteractor player, ObjectIdentity identity, RecipeData recipe)
        {
            return true;
        }

        protected virtual void OnContainedObjectInserted(HoldableObject insertedObject, RecipeData recipe)
        {
        }

        protected virtual void OnContainedObjectRemoved(HoldableObject removedObject)
        {
        }

        protected virtual void OnContainedObjectReplaced(HoldableObject previousObject, HoldableObject newObject)
        {
        }

        protected virtual bool CanUpdateContainedObject()
        {
            return !IsHeld;
        }

        private void UpdateContainedObject()
        {
            if (containedObject == null || activeRecipe == null || !CanUpdateContainedObject())
            {
                if (particles != null)
                    particles.SetState(false, FoodState.Raw);

                return;
            }

            if (activeRecipe.SpinsInContainer)
            {
                Transform target = containedVisual != null ? containedVisual.transform : containedObject.transform;
                target.Rotate(0f, 0f, activeRecipe.SpinSpeed * Time.deltaTime, Space.Self);
            }

            CookableItem cookable = containedObject.GetComponent<CookableItem>();

            if (cookable == null)
                return;

            bool carbonizedNow = cookable.AdvanceCooking(Time.deltaTime, activeRecipe);
            UpdateParticles(cookable.State);
            LogStateIfChanged(cookable);

            if (carbonizedNow && activeRecipe.CarbonizedTurnsIntoCharcoal && activeRecipe.CharcoalPrefab != null)
                ReplaceContainedObject(activeRecipe.CharcoalPrefab, null);
            else if (cookable.State == FoodState.Ready && activeRecipe.OutputWhenReadyPrefab != null)
                ReplaceContainedObject(activeRecipe.OutputWhenReadyPrefab, null);
        }

        private bool TryInsertHeldObject(PlayerInteractor player)
        {
            if (!player.Holder.TryGetHeldIdentity(out ObjectIdentity identity))
                return false;

            RecipeData recipe = FindRecipe(identity.Kind);

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
            containedObject.GetComponent<CookableItem>()?.ApplyRecipeVisual(recipe);
            SpawnSideEffects(recipe);
            OnContainedObjectInserted(containedObject, recipe);
            LogInserted(identity.Kind, containedObject);
            return true;
        }

        private bool TryTakeContainedObject(PlayerInteractor player)
        {
            if (!player.Holder.IsEmpty || containedObject == null)
            {
                if (logCooking && containedObject != null)
                    Debug.Log($"{name}: mao precisa estar vazia para tirar o item.", this);

                return false;
            }

            ClearContainedVisual();
            HoldableObject target = containedObject;
            containedObject = null;
            activeRecipe = null;
            target.RemoveFromContainer();
            RestoreContainedVisibility(target);
            OnContainedObjectRemoved(target);
            particles?.SetState(false, FoodState.Raw);
            return player.Holder.TryPickUp(target);
        }

        private bool TryPlateContainedObject(PlayerInteractor player)
        {
            if (containedObject == null || !player.Holder.TryGetHeldComponent(out Plate plate))
                return false;

            CookableItem cookable = containedObject.GetComponent<CookableItem>();

            if (cookable != null && !cookable.CanBePlated)
            {
                if (logCooking)
                    Debug.Log($"{name}: {containedObject.name} ainda nao esta pronto para empratar ({cookable.State}).", this);

                return false;
            }

            ObjectIdentity identity = containedObject.GetComponent<ObjectIdentity>();

            if (identity == null || !plate.TrySetFoodFromObject(containedObject))
                return false;

            Destroy(containedObject.gameObject);
            containedObject = null;
            activeRecipe = null;
            ClearContainedVisual();
            particles?.SetState(false, FoodState.Raw);
            Debug.Log($"{name}: item empratado.", this);
            return true;
        }

        private void ConfigureContainedObject(RecipeData recipe, bool resetProgress)
        {
            CookableItem cookable = containedObject.GetComponent<CookableItem>();

            if (cookable == null && recipe != null && recipe.UsesHeat)
                cookable = containedObject.gameObject.AddComponent<CookableItem>();

            if (cookable != null)
                cookable.ConfigureFromRecipe(recipe, resetProgress);
        }

        private void ReplaceContainedObject(HoldableObject prefab, RecipeData recipeForNewObject)
        {
            if (prefab == null || containedObject == null)
                return;

            HoldableObject previousObject = containedObject;
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

        private RecipeData FindRecipe(ObjectKind inputKind)
        {
            return recipeBook != null ? FindRecipe(recipeBook, inputKind) : null;
        }

        protected abstract RecipeData FindRecipe(DemoRecipeBook book, ObjectKind inputKind);

        private void SpawnContainedVisual(RecipeData recipe)
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

            containedVisual = ObjectVisualPreset.InstantiateFor(recipe.ContainedVisualPrefab, VisualTarget, containedObject.transform);
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

        private void RestoreContainedVisibility(HoldableObject target)
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

        private void SpawnSideEffects(RecipeData recipe)
        {
            HoldableObject[] prefabs = recipe.SpawnedOnInsertPrefabs;

            if (prefabs == null || prefabs.Length == 0)
                return;

            Transform root = sideEffectSpawnRoot != null ? sideEffectSpawnRoot : transform;

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] == null)
                    continue;

                Vector3 offset = root.right * ((i - (prefabs.Length - 1) * 0.5f) * sideEffectSpacing);
                HoldableObject sideEffect = Instantiate(prefabs[i], root.position + offset, root.rotation);
                sideEffect.name = prefabs[i].name;
                sideEffect.Drop();
            }
        }

        protected virtual Transform GetAnchor()
        {
            return transform;
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

        private void UpdateParticles(FoodState state)
        {
            if (particles == null || activeRecipe == null)
                return;

            particles.SetState(activeRecipe.UsesHeat, state);
        }

        private void LogInserted(ObjectKind inputKind, HoldableObject outputObject)
        {
            if (!logCooking)
                return;

            CookableItem cookable = outputObject.GetComponent<CookableItem>();
            lastLoggedState = cookable != null ? cookable.State : FoodState.Raw;
            string stateText = cookable != null ? $" estado {cookable.State}" : " sem timer";
            Debug.Log($"{name}: recebeu {inputKind} -> {outputObject.name}{stateText}.", this);
        }

        private void LogStateIfChanged(CookableItem cookable)
        {
            if (!logCooking || cookable.State == lastLoggedState)
                return;

            lastLoggedState = cookable.State;
            Debug.Log($"{name}: {containedObject.name} chegou em {cookable.State} ({cookable.CookedTime:0.0}s).", this);
        }

        private HoldableObject GetOrCreateHoldableObject()
        {
            if (holdableObject != null)
                return holdableObject;

            Rigidbody body = GetComponent<Rigidbody>();

            if (body == null)
                body = gameObject.AddComponent<Rigidbody>();

            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            holdableObject = gameObject.AddComponent<HoldableObject>();
            return holdableObject;
        }
    }
}

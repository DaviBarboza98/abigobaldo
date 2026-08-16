using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Abigobaldo.Game
{
    public abstract class ContainerStation : MonoBehaviour, IInteractable, IPickupInteractable, IObjectContainer
    {
        [Header("Recipes")]
        [SerializeField] private RecipeBook recipeBook;

        [Header("Content")]
        [FormerlySerializedAs("sideEffectSpawnRoot")]
        [SerializeField] private Transform sideEffectAnchor;
        [SerializeField] private float sideEffectSpacing = 0.12f;
        [FormerlySerializedAs("showContainedObject")]
        [SerializeField] private bool showContents = true;
        [SerializeField] private float multipleContentSpacing = 0.12f;

        [Header("Debug")]
        [SerializeField] private bool logCooking = true;

        private readonly List<HoldableObject> contents = new List<HoldableObject>();
        private readonly List<GameObject> contentVisuals = new List<GameObject>();
        private readonly Dictionary<Renderer, bool> hiddenRendererStates = new Dictionary<Renderer, bool>();

        protected RecipeData activeRecipe;
        private FoodState lastLoggedState;
        private HoldableObject stationHoldable;

        public bool HasContent => contents.Count > 0;
        public bool HasActiveRecipe => activeRecipe != null;
        public bool IsHeld => stationHoldable != null && stationHoldable.IsHeld;
        public HoldableObject Holdable => stationHoldable;
        public virtual bool IsDirectInteractionTarget => true;
        protected HoldableObject ProcessedObject => contents.Count == 1 ? contents[0] : null;
        protected Transform ContentMotionTarget => contentVisuals.Count > 0 && contentVisuals[0] != null
            ? contentVisuals[0].transform
            : ProcessedObject != null ? ProcessedObject.transform : null;
        protected abstract RecipeStationType StationType { get; }
        protected virtual ObjectVisualTarget VisualTarget => ObjectVisualTarget.Default;
        protected virtual bool ShouldShowContents => showContents;

        protected virtual void Awake()
        {
            stationHoldable = GetComponent<HoldableObject>();
        }

        private void Update()
        {
            UpdateRecipe();
        }

        public virtual void Interact(PlayerInteractor player)
        {
            if (player == null || player.Holder == null)
                return;

            if (HasContent && player.Holder.TryGetHeldComponent(out Plate plate))
            {
                TryPlateInto(plate);
                return;
            }

            if (!player.Holder.IsEmpty)
            {
                TryInsertHeldObject(player);
                return;
            }

            TryTakeLastObject(player);
        }

        public virtual void PickInteract(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || stationHoldable == null || !stationHoldable.CanBeHeld)
                return;

            if (!player.Holder.IsEmpty && player.Holder.CurrentObject != stationHoldable)
                player.Holder.Drop();

            player.Holder.TryPickUp(stationHoldable);
        }

        protected virtual bool CanInsertObject(PlayerInteractor player, ObjectIdentity identity, RecipeData recipe)
        {
            return true;
        }

        protected virtual bool CanUpdateRecipe()
        {
            return !IsHeld;
        }

        protected virtual void AnimateContent(float deltaTime)
        {
        }

        protected virtual void OnContentChanged()
        {
        }

        protected virtual void OnRecipeBecameReady()
        {
        }

        protected virtual Transform GetContentAnchor()
        {
            return transform;
        }

        private void UpdateRecipe()
        {
            if (activeRecipe == null || ProcessedObject == null || !CanUpdateRecipe())
                return;

            RecipeProgress progress = ProcessedObject.GetComponent<RecipeProgress>();

            if (progress == null)
                return;

            AnimateContent(Time.deltaTime);
            bool stateChanged = progress.Advance(Time.deltaTime, out bool becameReady);

            if (progress.State == FoodState.Carbonized)
            {
                TryApplyCarbonizedOutput(progress);
                return;
            }

            if (stateChanged)
            {
                RefreshContentVisuals();
                LogStateIfChanged(progress);
            }

            if (becameReady || (progress.IsReady && !progress.ResultApplied))
                TryApplyRecipeResult(progress);
        }

        public bool TryInsertHeldObject(PlayerInteractor player)
        {
            if (!player.Holder.TryGetHeldIdentity(out ObjectIdentity identity) || identity.Definition == null)
                return false;

            HoldableObject heldObject = player.Holder.CurrentObject;
            return TryInsertObjectFromHolder(heldObject, player, identity);
        }

        public bool TryInsertObject(HoldableObject item, PlayerInteractor player)
        {
            if (item == null)
                return false;

            ObjectIdentity identity = item.GetComponent<ObjectIdentity>();

            if (identity == null || identity.Definition == null)
                return false;

            if (player != null && player.Holder != null && player.Holder.CurrentObject == item)
                return TryInsertObjectFromHolder(item, player, identity);

            return TryInsertLooseObject(item, player, identity);
        }

        private bool TryInsertObjectFromHolder(HoldableObject heldObject, PlayerInteractor player, ObjectIdentity identity)
        {
            RecipeProgress heldProgress = heldObject.GetComponent<RecipeProgress>();

            if (!CanShowHeldObjectInThisContainer(heldObject, identity))
                return false;

            if (contents.Count == 0
                && heldProgress != null
                && heldProgress.Recipe != null
                && heldProgress.Recipe.RequiredStation == StationType)
            {
                RecipeData resumeRecipe = heldProgress.Recipe;

                if (!CanInsertObject(player, identity, resumeRecipe))
                    return false;

                HoldableObject resumedObject = player.Holder.ReleaseHeldObject();
                contents.Add(resumedObject);
                activeRecipe = resumeRecipe;
                resumedObject.PlaceInContainer(GetContentAnchor());
                lastLoggedState = heldProgress.State;
                RefreshContentVisuals();
                OnContentChanged();
                Log($"{name}: retomou {identity.DisplayName} em {heldProgress.State} ({heldProgress.ElapsedTime:0.0}s).", this);
                return true;
            }

            if (activeRecipe != null)
            {
                Log($"{name}: a receita atual precisa ser retirada antes de adicionar outro objeto.", this);
                return false;
            }

            List<ObjectDefinition> candidateContents = GetContentDefinitions();
            candidateContents.Add(identity.Definition);
            RecipeData matchingRecipe = recipeBook != null
                ? recipeBook.FindExact(StationType, candidateContents)
                : null;

            if (contents.Count > 0 && (recipeBook == null || !recipeBook.CanAccept(StationType, candidateContents)))
            {
                Log($"{name}: {identity.DisplayName} nao completa uma receita aqui.", this);
                return false;
            }

            if (!CanInsertObject(player, identity, matchingRecipe))
                return false;

            HoldableObject insertedObject = player.Holder.ReleaseHeldObject();

            if (insertedObject == null)
                return false;

            contents.Add(insertedObject);
            insertedObject.PlaceInContainer(GetContentAnchor());

            if (matchingRecipe != null)
                StartRecipe(matchingRecipe);
            else
                RefreshContentVisuals();

            OnContentChanged();
            return true;
        }

        private bool TryInsertLooseObject(HoldableObject item, PlayerInteractor player, ObjectIdentity identity)
        {
            if (!CanShowHeldObjectInThisContainer(item, identity))
                return false;

            RecipeProgress progress = item.GetComponent<RecipeProgress>();

            if (contents.Count == 0
                && progress != null
                && progress.Recipe != null
                && progress.Recipe.RequiredStation == StationType)
            {
                if (!CanInsertObject(player, identity, progress.Recipe))
                    return false;

                contents.Add(item);
                activeRecipe = progress.Recipe;
                item.PlaceInContainer(GetContentAnchor());
                lastLoggedState = progress.State;
                RefreshContentVisuals();
                OnContentChanged();
                return true;
            }

            if (activeRecipe != null)
                return false;

            List<ObjectDefinition> candidateContents = GetContentDefinitions();
            candidateContents.Add(identity.Definition);
            RecipeData matchingRecipe = recipeBook != null
                ? recipeBook.FindExact(StationType, candidateContents)
                : null;

            if (contents.Count > 0 && (recipeBook == null || !recipeBook.CanAccept(StationType, candidateContents)))
                return false;

            if (!CanInsertObject(player, identity, matchingRecipe))
                return false;

            contents.Add(item);
            item.PlaceInContainer(GetContentAnchor());

            if (matchingRecipe != null)
                StartRecipe(matchingRecipe);
            else
                RefreshContentVisuals();

            OnContentChanged();
            return true;
        }

        private bool CanShowHeldObjectInThisContainer(HoldableObject heldObject, ObjectIdentity identity)
        {
            if (heldObject == null)
                return false;

            if (heldObject.GetComponent<IObjectContainer>() != null)
                return false;

            if (ObjectVisualPreset.HasPlacementFor(heldObject.gameObject, VisualTarget))
                return true;

            Log($"{name}: {identity.DisplayName} nao tem ObjectVisualPreset para {VisualTarget}.", this);
            return false;
        }

        private void StartRecipe(RecipeData recipe)
        {
            if (recipe == null || contents.Count == 0)
                return;

            HoldableObject processObject = null;

            if (recipe.InProgressPrefab != null)
            {
                ClearContentVisuals();

                foreach (HoldableObject input in contents)
                {
                    if (input != null)
                        Destroy(input.gameObject);
                }

                processObject = InstantiateHoldable(recipe.InProgressPrefab, GetContentAnchor());

                if (processObject == null)
                    return;
            }
            else if (contents.Count == 1)
            {
                processObject = contents[0];
            }
            else
            {
                Debug.LogError($"{recipe.name}: multiple ingredients require an In Progress Prefab.", recipe);
                return;
            }

            contents.Clear();
            contents.Add(processObject);
            processObject.PlaceInContainer(GetContentAnchor());

            RecipeProgress progress = processObject.GetComponent<RecipeProgress>();

            if (progress == null)
                progress = processObject.gameObject.AddComponent<RecipeProgress>();

            progress.Configure(recipe, true);
            activeRecipe = recipe;
            lastLoggedState = progress.State;
            SpawnByproducts(recipe);
            RefreshContentVisuals();
            Log($"{name}: iniciou {recipe.name} com {recipe.RequiredIngredientCount} ingrediente(s).", this);
        }

        public bool TryTakeLastObject(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || !player.Holder.IsEmpty || contents.Count == 0)
                return false;

            ClearContentVisuals();
            int lastIndex = contents.Count - 1;
            HoldableObject target = contents[lastIndex];
            contents.RemoveAt(lastIndex);

            if (activeRecipe != null)
                activeRecipe = null;

            target.RemoveFromContainer();
            bool pickedUp = player.Holder.TryPickUp(target);
            RefreshContentVisuals();
            OnContentChanged();
            return pickedUp;
        }

        public bool TryMoveLastObjectTo(IObjectContainer target, PlayerInteractor player)
        {
            if (target == null || ReferenceEquals(target, this) || contents.Count == 0)
                return false;

            ClearContentVisuals();
            int lastIndex = contents.Count - 1;
            HoldableObject item = contents[lastIndex];
            contents.RemoveAt(lastIndex);

            if (activeRecipe != null)
                activeRecipe = null;

            item.RemoveFromContainer();

            if (target.TryInsertObject(item, player))
            {
                RefreshContentVisuals();
                OnContentChanged();
                return true;
            }

            contents.Add(item);
            item.PlaceInContainer(GetContentAnchor());
            RefreshContentVisuals();
            OnContentChanged();
            return false;
        }

        public bool TryPlateInto(Plate plate)
        {
            if (plate == null || ProcessedObject == null)
                return false;

            RecipeProgress progress = ProcessedObject.GetComponent<RecipeProgress>();

            if (progress != null && !progress.IsReady)
            {
                Log($"{name}: {ProcessedObject.name} ainda esta em {progress.State}.", this);
                return false;
            }

            return TryMoveLastObjectTo(plate, null);
        }

        private void TryApplyRecipeResult(RecipeProgress progress)
        {
            if (progress == null || progress.ResultApplied)
                return;

            GameObject resultPrefab = activeRecipe.ResultPrefab;

            if (resultPrefab != null && !HasSameDefinition(ProcessedObject, resultPrefab))
            {
                if (!ReplaceProcessedObject(resultPrefab, true))
                    return;

                RecipeProgress replacementProgress = ProcessedObject != null
                    ? ProcessedObject.GetComponent<RecipeProgress>()
                    : null;
                replacementProgress?.MarkResultApplied();
            }
            else
            {
                progress.MarkResultApplied();
            }

            OnRecipeBecameReady();
        }

        private void TryApplyCarbonizedOutput(RecipeProgress progress)
        {
            if (progress == null || progress.CarbonizedOutputApplied)
                return;

            GameObject carbonizedPrefab = recipeBook != null ? recipeBook.CharcoalPrefab : null;

            if (carbonizedPrefab == null)
            {
                Debug.LogError($"{name}: RecipeBook requires a Charcoal Prefab for carbonized food.", this);
                return;
            }

            if (ReplaceProcessedObject(carbonizedPrefab, false))
            {
                progress.MarkCarbonizedOutputApplied();
                activeRecipe = null;
            }
        }

        private bool ReplaceProcessedObject(GameObject prefab, bool preserveProgress)
        {
            if (prefab == null || ProcessedObject == null)
                return false;

            HoldableObject previousObject = ProcessedObject;
            int contentIndex = contents.IndexOf(previousObject);

            if (contentIndex < 0)
                return false;

            RecipeProgress previousProgress = previousObject.GetComponent<RecipeProgress>();
            HoldableObject replacement = InstantiateHoldable(prefab, GetContentAnchor());

            if (replacement == null)
                return false;

            ClearContentVisuals();
            previousObject.transform.SetParent(null);
            previousObject.gameObject.SetActive(false);
            replacement.PlaceInContainer(GetContentAnchor());

            if (preserveProgress && previousProgress != null)
            {
                RecipeProgress replacementProgress = replacement.GetComponent<RecipeProgress>();

                if (replacementProgress == null)
                    replacementProgress = replacement.gameObject.AddComponent<RecipeProgress>();

                replacementProgress.CopyFrom(previousProgress);
            }

            contents[contentIndex] = replacement;
            Destroy(previousObject.gameObject);
            RefreshContentVisuals();
            OnContentChanged();
            Log($"{name}: resultado virou {replacement.name}.", this);
            return true;
        }

        private void RefreshContentVisuals()
        {
            ClearContentVisuals();
            Transform anchor = GetContentAnchor();

            for (int i = 0; i < contents.Count; i++)
            {
                HoldableObject content = contents[i];

                if (content == null)
                    continue;

                Vector3 offset = anchor.right * ((i - (contents.Count - 1) * 0.5f) * multipleContentSpacing);
                content.PlaceInContainer(anchor);
                content.transform.position += offset;

                if (!ShouldShowContents)
                {
                    SetRenderersVisible(content.gameObject, false);
                    continue;
                }

                RecipeProgress progress = content.GetComponent<RecipeProgress>();
                GameObject visual = ObjectVisualPreset.InstantiateFromObject(content, VisualTarget, anchor);
                SetRenderersVisible(content.gameObject, false);

                if (visual == null)
                    continue;

                visual.transform.position += offset;
                progress?.ApplyVisualTo(visual);
                contentVisuals.Add(visual);
            }
        }

        private void ClearContentVisuals()
        {
            foreach (GameObject visual in contentVisuals)
            {
                if (visual != null)
                    Destroy(visual);
            }

            contentVisuals.Clear();

            foreach (KeyValuePair<Renderer, bool> entry in hiddenRendererStates)
            {
                if (entry.Key != null)
                    entry.Key.enabled = entry.Value;
            }

            hiddenRendererStates.Clear();
        }

        private void SetRenderersVisible(GameObject target, bool visible)
        {
            foreach (Renderer targetRenderer in target.GetComponentsInChildren<Renderer>(true))
            {
                if (!hiddenRendererStates.ContainsKey(targetRenderer))
                    hiddenRendererStates[targetRenderer] = targetRenderer.enabled;

                targetRenderer.enabled = visible;
            }
        }

        private List<ObjectDefinition> GetContentDefinitions()
        {
            List<ObjectDefinition> definitions = new List<ObjectDefinition>(contents.Count);

            foreach (HoldableObject content in contents)
            {
                ObjectIdentity identity = content != null ? content.GetComponent<ObjectIdentity>() : null;

                if (identity != null && identity.Definition != null)
                    definitions.Add(identity.Definition);
            }

            return definitions;
        }

        private void SpawnByproducts(RecipeData recipe)
        {
            IReadOnlyList<GameObject> prefabs = recipe.Byproducts;

            if (prefabs == null || prefabs.Count == 0)
                return;

            Transform root = sideEffectAnchor != null ? sideEffectAnchor : transform;

            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[i];

                if (prefab == null)
                    continue;

                Vector3 offset = root.right * ((i - (prefabs.Count - 1) * 0.5f) * sideEffectSpacing);
                HoldableObject byproduct = InstantiateHoldable(prefab, root.position + offset, root.rotation);

                if (byproduct == null)
                    continue;

                byproduct.Drop();
            }
        }

        private void LogStateIfChanged(RecipeProgress progress)
        {
            if (progress == null || progress.State == lastLoggedState)
                return;

            lastLoggedState = progress.State;
            Log($"{name}: {ProcessedObject.name} chegou em {progress.State} ({progress.ElapsedTime:0.0}s).", this);
        }

        private void Log(string message, Object context)
        {
            if (logCooking)
                Debug.Log(message, context);
        }

        private static bool HasSameDefinition(HoldableObject first, GameObject second)
        {
            ObjectIdentity firstIdentity = first != null ? first.GetComponent<ObjectIdentity>() : null;
            ObjectIdentity secondIdentity = second != null ? second.GetComponent<ObjectIdentity>() : null;
            return firstIdentity != null
                && secondIdentity != null
                && firstIdentity.Definition != null
                && firstIdentity.Definition == secondIdentity.Definition;
        }

        private static HoldableObject InstantiateHoldable(GameObject prefab, Transform anchor)
        {
            return anchor == null
                ? null
                : InstantiateHoldable(prefab, anchor.position, anchor.rotation);
        }

        private static HoldableObject InstantiateHoldable(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                return null;

            GameObject instance = Instantiate(prefab, position, rotation);
            instance.name = prefab.name;
            HoldableObject holdable = instance.GetComponent<HoldableObject>();

            if (holdable != null)
                return holdable;

            Debug.LogError($"{prefab.name}: recipe prefabs must have a HoldableObject component.", prefab);
            Destroy(instance);
            return null;
        }

        private void OnValidate()
        {
            sideEffectSpacing = Mathf.Max(0f, sideEffectSpacing);
            multipleContentSpacing = Mathf.Max(0f, multipleContentSpacing);
        }
    }
}

using UnityEngine;
using UnityEngine.Serialization;

namespace Abigobaldo.Game
{
    public class Plate : MonoBehaviour, IInteractable, IObjectContainer
    {
        [FormerlySerializedAs("foodRoot")]
        [SerializeField] private Transform contentAnchor;

        private HoldableObject contentObject;
        private ObjectDefinition contentDefinition;
        private GameObject contentVisual;
        private HoldableObject holdableObject;

        public bool IsEmpty => contentObject == null;
        public bool HasContent => contentObject != null;
        public HoldableObject Holdable => holdableObject;
        public ObjectDefinition ContentDefinition => contentDefinition;

        private void Awake()
        {
            holdableObject = GetComponent<HoldableObject>();
        }

        public void Interact(PlayerInteractor player)
        {
            if (player == null || player.Holder == null)
                return;

            if (!player.Holder.IsEmpty)
            {
                TryInsertObject(player.Holder.CurrentObject, player);
                return;
            }

            TryTakeLastObject(player);
        }

        public bool TryInsertObject(HoldableObject source, PlayerInteractor player)
        {
            if (source == null || !IsEmpty)
                return false;

            if (IsContainer(source))
                return false;

            ObjectIdentity identity = source.GetComponent<ObjectIdentity>();

            if (identity == null || identity.Definition == null)
                return false;

            if (!ObjectVisualPreset.HasPlacementFor(source.gameObject, ObjectVisualTarget.Plate))
                return false;

            HoldableObject insertedObject = source;

            if (player != null && player.Holder != null && player.Holder.CurrentObject == source)
                insertedObject = player.Holder.ReleaseHeldObject();

            if (insertedObject == null)
                return false;

            contentDefinition = identity.Definition;
            contentObject = insertedObject;
            contentObject.PlaceInContainer(GetContentAnchor());
            RefreshVisual();
            return true;
        }

        public bool TrySetFoodFromObject(HoldableObject source)
        {
            return TryInsertObject(source, null);
        }

        public bool TryTakeLastObject(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || !player.Holder.IsEmpty || contentObject == null)
                return false;

            HoldableObject target = ExtractContent();
            return player.Holder.TryPickUp(target);
        }

        public bool TryMoveLastObjectTo(IObjectContainer target, PlayerInteractor player)
        {
            if (target == null || ReferenceEquals(target, this) || contentObject == null)
                return false;

            HoldableObject item = ExtractContent();

            if (target.TryInsertObject(item, player))
                return true;

            TryInsertObject(item, player);
            return false;
        }

        public void Clear()
        {
            contentDefinition = null;
            contentObject = null;

            if (contentVisual != null)
                Destroy(contentVisual);

            contentVisual = null;
        }

        public bool TryPlateHeldObject(PlayerInteractor player)
        {
            return player != null && player.Holder != null && TryInsertObject(player.Holder.CurrentObject, player);
        }

        private static bool IsContainer(HoldableObject source)
        {
            return source.GetComponent<IObjectContainer>() != null
                || source.GetComponent<BlenderCupContent>() != null;
        }

        private Transform GetContentAnchor()
        {
            return contentAnchor != null ? contentAnchor : transform;
        }

        private HoldableObject ExtractContent()
        {
            HoldableObject item = contentObject;
            ClearVisualOnly();
            contentObject = null;
            contentDefinition = null;
            item.RemoveFromContainer();
            return item;
        }

        private void RefreshVisual()
        {
            ClearVisualOnly();

            if (contentObject == null)
                return;

            contentVisual = ObjectVisualPreset.InstantiateFromObject(contentObject, ObjectVisualTarget.Plate, GetContentAnchor());
            SetRenderersVisible(contentObject.gameObject, false);

            RecipeProgress progress = contentObject.GetComponent<RecipeProgress>();
            progress?.ApplyVisualTo(contentVisual);
        }

        private void ClearVisualOnly()
        {
            if (contentVisual != null)
                Destroy(contentVisual);

            contentVisual = null;

            if (contentObject != null)
                SetRenderersVisible(contentObject.gameObject, true);
        }

        private static void SetRenderersVisible(GameObject target, bool visible)
        {
            foreach (Renderer targetRenderer in target.GetComponentsInChildren<Renderer>(true))
                targetRenderer.enabled = visible;
        }

        private void OnValidate()
        {
            if (contentAnchor == null)
                contentAnchor = transform;
        }
    }
}

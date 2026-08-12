using UnityEngine;

namespace Abigobaldo.Game
{
    public class BlenderStation : ContainerStation
    {
        [SerializeField] private BlenderCupContent cup;

        protected override ObjectVisualTarget VisualTarget => ObjectVisualTarget.Blender;

        protected override RecipeData FindRecipe(DemoRecipeBook book, ObjectKind inputKind)
        {
            return book.FindBlenderRecipe(inputKind);
        }

        private void OnEnable()
        {
            CacheCup();
            RefreshCupLock();
        }

        private void LateUpdate()
        {
            RefreshCupLock();
        }

        public override void Interact(PlayerInteractor player)
        {
            CacheCup();

            if (TryAttachHeldCup(player))
                return;

            base.Interact(player);
        }

        public override void PickInteract(PlayerInteractor player)
        {
            // The blender base is fixed. The removable object is the BlenderCup.
        }

        protected override bool CanInsertObject(PlayerInteractor player, ObjectIdentity identity, RecipeData recipe)
        {
            CacheCup();

            if (cup == null || cup.IsAttached)
                return true;

            Debug.Log($"{name}: encaixe o copo antes de colocar ingrediente.", this);
            return false;
        }

        protected override void OnContainedObjectInserted(HoldableObject insertedObject, RecipeData recipe)
        {
            RefreshCupLock();
        }

        protected override void OnContainedObjectRemoved(HoldableObject removedObject)
        {
            RefreshCupLock();
        }

        protected override void OnContainedObjectReplaced(HoldableObject previousObject, HoldableObject newObject)
        {
            RefreshCupLock();
        }

        protected override bool CanUpdateContainedObject()
        {
            CacheCup();
            return base.CanUpdateContainedObject() && cup != null && cup.IsAttached;
        }

        protected override Transform GetAnchor()
        {
            CacheCup();
            return cup != null ? cup.ContentRoot : transform;
        }

        private bool TryAttachHeldCup(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || !player.Holder.TryGetHeldComponent(out BlenderCupContent heldCup))
                return false;

            HoldableObject releasedCup = player.Holder.ReleaseHeldObject();

            if (releasedCup == null || !heldCup.TryAttachHome())
                return false;

            cup = heldCup;
            RefreshCupLock();
            Debug.Log($"{name}: copo encaixado.", this);
            return true;
        }

        private void RefreshCupLock()
        {
            CacheCup();
            cup?.HoldableObject?.SetPickupLocked(false);
        }

        private void CacheCup()
        {
            if (cup == null)
                cup = GetComponentInChildren<BlenderCupContent>(true);
        }
    }
}

using UnityEngine;

namespace Abigobaldo.Game
{
    public class BlenderStation : ContainerStation
    {
        [SerializeField] private BlenderCupContent cup;

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

        private bool TryAttachHeldCup(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || !player.Holder.TryGetHeldComponent(out BlenderCupContent heldCup))
                return false;

            if (HasContainedObject)
            {
                Debug.Log($"{name}: tire o conteudo antes de encaixar/desencaixar o copo.", this);
                return true;
            }

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
            cup?.HoldableObject?.SetPickupLocked(HasContainedObject);
        }

        private void CacheCup()
        {
            if (cup == null)
                cup = GetComponentInChildren<BlenderCupContent>(true);
        }
    }
}

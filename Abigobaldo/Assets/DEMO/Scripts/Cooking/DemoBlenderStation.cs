using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoBlenderStation : DemoContainerStation
    {
        [SerializeField] private DemoBlenderCupContent cup;

        private void OnEnable()
        {
            CacheCup();
            RefreshCupLock();
        }

        private void LateUpdate()
        {
            RefreshCupLock();
        }

        public override void Interact(DemoPlayerInteractor player)
        {
            CacheCup();

            if (TryAttachHeldCup(player))
                return;

            base.Interact(player);
        }

        protected override bool CanInsertObject(DemoPlayerInteractor player, DemoObjectIdentity identity, DemoRecipeData recipe)
        {
            CacheCup();

            if (cup == null || cup.IsAttached)
                return true;

            Debug.Log($"{name}: encaixe o copo antes de colocar ingrediente.", this);
            return false;
        }

        protected override void OnContainedObjectInserted(DemoHoldableObject insertedObject, DemoRecipeData recipe)
        {
            RefreshCupLock();
        }

        protected override void OnContainedObjectRemoved(DemoHoldableObject removedObject)
        {
            RefreshCupLock();
        }

        protected override void OnContainedObjectReplaced(DemoHoldableObject previousObject, DemoHoldableObject newObject)
        {
            RefreshCupLock();
        }

        private bool TryAttachHeldCup(DemoPlayerInteractor player)
        {
            if (player == null || player.Holder == null || !player.Holder.TryGetHeldComponent(out DemoBlenderCupContent heldCup))
                return false;

            if (HasContainedObject)
            {
                Debug.Log($"{name}: tire o conteudo antes de encaixar/desencaixar o copo.", this);
                return true;
            }

            DemoHoldableObject releasedCup = player.Holder.ReleaseHeldObject();

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
                cup = GetComponentInChildren<DemoBlenderCupContent>(true);
        }
    }
}

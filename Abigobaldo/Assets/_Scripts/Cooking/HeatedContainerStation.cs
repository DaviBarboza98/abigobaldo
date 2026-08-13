using UnityEngine;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(HoldableObject))]
    public abstract class HeatedContainerStation : ContainerStation, IHoldableLifecycle
    {
        private HoldableObject dockedHoldable;
        private CooktopSlot currentCooktop;

        public bool IsDocked => currentCooktop != null;
        public CooktopSlot CurrentCooktop => currentCooktop;

        protected override void Awake()
        {
            base.Awake();
            dockedHoldable = GetComponent<HoldableObject>();
        }

        public bool TryDock(CooktopSlot cooktop)
        {
            if (cooktop == null || !cooktop.TryClaim(this))
                return false;

            if (currentCooktop != null && currentCooktop != cooktop)
                currentCooktop.Release(this);

            currentCooktop = cooktop;
            dockedHoldable ??= GetComponent<HoldableObject>();
            dockedHoldable.PlaceOnDock(cooktop.ContainerAnchor);
            return true;
        }

        public void Undock()
        {
            CooktopSlot previousCooktop = currentCooktop;
            currentCooktop = null;
            previousCooktop?.Release(this);
        }

        public void OnPickedUp()
        {
            Undock();
        }

        public void OnDropped()
        {
        }

        protected override bool CanUpdateRecipe()
        {
            return base.CanUpdateRecipe() && IsDocked;
        }
    }
}

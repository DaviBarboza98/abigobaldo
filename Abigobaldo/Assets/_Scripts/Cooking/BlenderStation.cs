using UnityEngine;

namespace Abigobaldo.Game
{
    public class BlenderStation : ContainerStation
    {
        private enum SpinAxis
        {
            X,
            Y,
            Z
        }

        [SerializeField] private BlenderCupContent cup;
        [SerializeField] private float spinSpeed = 720f;
        [SerializeField] private SpinAxis spinAxis = SpinAxis.Z;

        private bool isRunning;

        protected override RecipeStationType StationType => RecipeStationType.Blender;
        protected override ObjectVisualTarget VisualTarget => ObjectVisualTarget.Blender;

        private void OnEnable()
        {
            CacheCup();
            LockBasePickup();
        }

        public override void Interact(PlayerInteractor player)
        {
            CacheCup();

            if (!TryAttachHeldCup(player))
                Debug.Log($"{name}: interaja com o copo para colocar ou retirar ingredientes.", this);
        }

        public void InteractWithCup(PlayerInteractor player)
        {
            CacheCup();
            base.Interact(player);
        }

        public override void PickInteract(PlayerInteractor player)
        {
            CacheCup();

            if (TryAttachHeldCup(player))
                return;

            if (!HasActiveRecipe || !HasContent || cup == null || !cup.IsAttached)
            {
                isRunning = false;
                Debug.Log($"{name}: nao ha uma receita pronta para processar.", this);
                return;
            }

            isRunning = !isRunning;
            Debug.Log($"{name}: liquidificador {(isRunning ? "ligado" : "desligado")}.", this);
        }

        protected override bool CanInsertObject(PlayerInteractor player, ObjectIdentity identity, RecipeData recipe)
        {
            CacheCup();

            if (cup != null && cup.IsAttached)
                return true;

            Debug.Log($"{name}: encaixe o copo antes de colocar ingredientes.", this);
            return false;
        }

        protected override bool CanUpdateRecipe()
        {
            CacheCup();
            return base.CanUpdateRecipe() && isRunning && cup != null && cup.IsAttached;
        }

        protected override void AnimateContent(float deltaTime)
        {
            if (ContentMotionTarget != null)
                ContentMotionTarget.Rotate(GetSpinEuler(spinSpeed * deltaTime), Space.Self);
        }

        protected override void OnContentChanged()
        {
            if (!HasContent)
                isRunning = false;
        }

        protected override void OnRecipeBecameReady()
        {
            isRunning = false;
            Debug.Log($"{name}: receita pronta; liquidificador desligado automaticamente.", this);
        }

        protected override Transform GetContentAnchor()
        {
            CacheCup();
            return cup != null ? cup.ContentRoot : transform;
        }

        private bool TryAttachHeldCup(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || !player.Holder.TryGetHeldComponent(out BlenderCupContent heldCup))
                return false;

            HoldableObject releasedCup = player.Holder.ReleaseHeldObject();

            if (releasedCup == null)
                return false;

            if (!heldCup.TryAttachHome())
            {
                player.Holder.TryPickUp(releasedCup);
                return false;
            }

            cup = heldCup;
            Debug.Log($"{name}: copo encaixado.", this);
            return true;
        }

        private void CacheCup()
        {
            if (cup == null)
                cup = GetComponentInChildren<BlenderCupContent>(true);
        }

        private void LockBasePickup()
        {
            HoldableObject baseHoldable = GetComponent<HoldableObject>();
            baseHoldable?.SetPickupLocked(true);

            Rigidbody body = GetComponent<Rigidbody>();

            if (body == null)
                return;

            if (!body.isKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.useGravity = false;
            body.isKinematic = true;
        }

        private Vector3 GetSpinEuler(float angle)
        {
            return spinAxis switch
            {
                SpinAxis.X => new Vector3(angle, 0f, 0f),
                SpinAxis.Y => new Vector3(0f, angle, 0f),
                _ => new Vector3(0f, 0f, angle)
            };
        }

        private void OnValidate()
        {
            spinSpeed = Mathf.Max(0f, spinSpeed);
        }
    }
}

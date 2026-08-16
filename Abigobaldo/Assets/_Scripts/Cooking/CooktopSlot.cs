using UnityEngine;

namespace Abigobaldo.Game
{
    public sealed class CooktopSlot : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform containerAnchor;
        [SerializeField] private HeatedContainerStation startingContainer;

        private HeatedContainerStation occupant;

        public Transform ContainerAnchor => containerAnchor != null ? containerAnchor : transform;
        public HeatedContainerStation Occupant => occupant;
        public bool IsOccupied => occupant != null;

        private void Start()
        {
            if (startingContainer != null)
                startingContainer.TryDock(this);
        }

        public void Interact(PlayerInteractor player)
        {
            if (player == null || player.Holder == null)
                return;

            if (!player.Holder.TryGetHeldComponent(out HeatedContainerStation container))
            {
                Debug.Log($"{name}: segure uma frigideira, cuscuzeira ou outro recipiente aquecivel.", this);
                return;
            }

            if (IsOccupied && occupant != container)
            {
                Debug.Log($"{name}: esta boca ja esta ocupada.", this);
                return;
            }

            HoldableObject releasedObject = player.Holder.ReleaseHeldObject();

            if (releasedObject == null)
                return;

            if (!container.TryDock(this))
            {
                player.Holder.TryPickUp(releasedObject);
                return;
            }

            Debug.Log($"{container.name}: encaixado em {name}; aquecimento retomado.", container);
        }

        public bool TryClaim(HeatedContainerStation container)
        {
            if (container == null || (occupant != null && occupant != container))
                return false;

            occupant = container;
            return true;
        }

        public void Release(HeatedContainerStation container)
        {
            if (occupant == container)
                occupant = null;
        }

        private void OnValidate()
        {
            if (containerAnchor == null)
                containerAnchor = transform;
        }
    }
}

using UnityEngine;

namespace Abigobaldo.Game
{
    public class ObjectSpawner : MonoBehaviour, IInteractable, IPickupInteractable
    {
        [SerializeField] private HoldableObject prefab;
        [UnityEngine.Serialization.FormerlySerializedAs("spawnPoint")]
        [SerializeField] private Transform spawnAnchor;
        [SerializeField] private bool giveDirectlyToHolder = true;
        [SerializeField] private bool replaceHeldObject = true;
        [SerializeField] private bool alignToSpawnPoint = true;

        public HoldableObject Prefab => prefab;

        public void Interact(PlayerInteractor player)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{name} has no object prefab configured.", this);
                return;
            }

            if (player == null || player.Holder == null || player.Holder.IsEmpty)
                return;

            if (TrySpawnIntoHeldContainer(player))
                return;

            HoldableObject spawnedContainer = CreateInstance();

            if (!TrySpawnContainerWithHeldObject(player, spawnedContainer))
                Destroy(spawnedContainer.gameObject);
        }

        public void PickInteract(PlayerInteractor player)
        {
            SpawnForPickup(player);
        }

        private void SpawnForPickup(PlayerInteractor player)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{name} has no object prefab configured.", this);
                return;
            }

            HoldableObject instance = CreateInstance();

            if (!giveDirectlyToHolder || player == null || player.Holder == null)
            {
                if (!alignToSpawnPoint)
                    instance.transform.rotation = prefab.transform.rotation;

                instance.Drop();
                return;
            }

            if (!player.Holder.IsEmpty)
            {
                if (!replaceHeldObject)
                {
                    Destroy(instance.gameObject);
                    return;
                }

                player.Holder.Drop();
            }

            if (!player.Holder.TryPickUp(instance))
                instance.Drop();
        }

        public HoldableObject CreateInstance()
        {
            Transform targetSpawnPoint = spawnAnchor != null ? spawnAnchor : transform;
            HoldableObject instance = Instantiate(prefab, targetSpawnPoint.position, targetSpawnPoint.rotation);
            instance.name = prefab.name;
            return instance;
        }

        private bool TrySpawnIntoHeldContainer(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || player.Holder.IsEmpty)
                return false;

            IObjectContainer heldContainer = player.Holder.CurrentObject.GetComponent<IObjectContainer>();

            if (heldContainer == null)
                return false;

            HoldableObject spawnedObject = CreateInstance();

            if (heldContainer.TryInsertObject(spawnedObject, player))
                return true;

            Destroy(spawnedObject.gameObject);
            return true;
        }

        private bool TrySpawnContainerWithHeldObject(PlayerInteractor player, HoldableObject spawnedObject)
        {
            if (player == null || player.Holder == null || player.Holder.IsEmpty || spawnedObject == null)
                return false;

            IObjectContainer spawnedContainer = spawnedObject.GetComponent<IObjectContainer>();

            if (spawnedContainer == null)
                spawnedContainer = spawnedObject.GetComponentInChildren<IObjectContainer>(true);

            if (spawnedContainer == null)
                return false;

            HoldableObject heldObject = player.Holder.CurrentObject;

            if (!spawnedContainer.TryInsertObject(heldObject, player))
            {
                Destroy(spawnedObject.gameObject);
                return true;
            }

            if (!giveDirectlyToHolder || !player.Holder.TryPickUp(spawnedObject))
                spawnedObject.Drop();

            return true;
        }

        private void OnValidate()
        {
            if (spawnAnchor == null)
                spawnAnchor = transform;
        }
    }
}

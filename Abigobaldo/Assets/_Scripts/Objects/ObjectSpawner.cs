using UnityEngine;

namespace Abigobaldo.Game
{
    public class ObjectSpawner : MonoBehaviour, IInteractable, IPickupInteractable
    {
        [SerializeField] private HoldableObject prefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool giveDirectlyToHolder = true;
        [SerializeField] private bool replaceHeldObject = true;
        [SerializeField] private bool alignToSpawnPoint = true;

        public void Interact(PlayerInteractor player)
        {
            Spawn(player);
        }

        public void PickInteract(PlayerInteractor player)
        {
            Spawn(player);
        }

        private void Spawn(PlayerInteractor player)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{name} has no object prefab configured.", this);
                return;
            }

            Transform targetSpawnPoint = spawnPoint != null ? spawnPoint : transform;
            HoldableObject instance = Instantiate(prefab, targetSpawnPoint.position, targetSpawnPoint.rotation);
            instance.name = prefab.name;

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

        private void OnValidate()
        {
            if (spawnPoint == null)
                spawnPoint = transform;
        }
    }
}

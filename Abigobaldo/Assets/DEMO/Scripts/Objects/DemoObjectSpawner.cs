using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoObjectSpawner : MonoBehaviour, IDemoInteractable, IDemoPickupInteractable
    {
        [SerializeField] private DemoHoldableObject prefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool giveDirectlyToHolder = true;
        [SerializeField] private bool replaceHeldObject = true;
        [SerializeField] private bool alignToSpawnPoint = true;

        public void Interact(DemoPlayerInteractor player)
        {
            Spawn(player);
        }

        public void PickInteract(DemoPlayerInteractor player)
        {
            Spawn(player);
        }

        private void Spawn(DemoPlayerInteractor player)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{name} has no demo object prefab configured.", this);
                return;
            }

            Transform targetSpawnPoint = spawnPoint != null ? spawnPoint : transform;
            DemoHoldableObject instance = Instantiate(prefab, targetSpawnPoint.position, targetSpawnPoint.rotation);
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

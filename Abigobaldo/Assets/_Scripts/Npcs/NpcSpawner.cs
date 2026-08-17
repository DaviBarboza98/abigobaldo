using UnityEngine;

namespace Abigobaldo.Game
{
    public sealed class NpcSpawner : MonoBehaviour
    {
        [Header("Customer Prefabs")]
        [SerializeField] private GameObject marciaPrefab;
        [SerializeField] private GameObject ninoPrefab;
        [SerializeField] private GameObject seuZePrefab;
        [SerializeField] private Transform spawnPoint;

        private GameObject currentNpc;

        public CustomerNpc Spawn(string customerId, bool nameKnown)
        {
            DespawnCurrent();
            GameObject prefab = GetPrefab(customerId);
            if (prefab == null)
            {
                Debug.LogError("NpcSpawner has no prefab for " + customerId + ".", this);
                return null;
            }

            Transform point = spawnPoint != null ? spawnPoint : transform;
            // Keep the active customer under NpcSpawner in the hierarchy while
            // preserving the configured world position and rotation.
            currentNpc = Instantiate(prefab, point.position, point.rotation, transform);
            CustomerNpc npc = currentNpc.GetComponent<CustomerNpc>();
            if (npc == null)
                npc = currentNpc.AddComponent<CustomerNpc>();
            ConfigureCustomer(npc, customerId, nameKnown);
            return npc;
        }

        public void DespawnCurrent()
        {
            if (currentNpc != null)
                Destroy(currentNpc);
            currentNpc = null;
        }

        private GameObject GetPrefab(string customerId)
        {
            switch (customerId)
            {
                case "marcia": return marciaPrefab;
                case "nino": return ninoPrefab;
                case "seuze": return seuZePrefab;
                default: return null;
            }
        }

        private static void ConfigureCustomer(CustomerNpc npc, string customerId, bool nameKnown)
        {
            switch (customerId)
            {
                case "marcia": npc.Configure("marcia", "Marcia", CustomerTemperament.Warm, nameKnown); break;
                case "nino": npc.Configure("nino", "Nino", CustomerTemperament.Timid, nameKnown); break;
                case "seuze": npc.Configure("seuze", "Seu Zé", CustomerTemperament.Gruff, nameKnown); break;
            }
        }
    }
}

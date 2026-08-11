using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoBreakIntoFragments : MonoBehaviour, IDemoInteractable
    {
        [SerializeField] private DemoHoldableObject fragmentPrefab;
        [SerializeField] private int fragmentCount = 12;
        [SerializeField] private float spawnRadius = 0.12f;
        [SerializeField] private float impulse = 0.6f;
        [SerializeField] private bool destroyOriginal = true;

        public void Interact(DemoPlayerInteractor player)
        {
            Break();
        }

        public void Break()
        {
            if (fragmentPrefab == null)
            {
                Debug.LogWarning($"{name} has no fragment prefab configured.", this);
                return;
            }

            int count = Mathf.Max(1, fragmentCount);

            for (int i = 0; i < count; i++)
            {
                Vector3 offset = Random.insideUnitSphere * spawnRadius;
                offset.y = Mathf.Abs(offset.y) * 0.5f;

                DemoHoldableObject fragment = Instantiate(
                    fragmentPrefab,
                    transform.position + offset,
                    Random.rotation
                );

                fragment.name = fragmentPrefab.name;

                if (fragment.Rigidbody != null)
                    fragment.Rigidbody.AddForce((offset.normalized + Vector3.up * 0.25f) * impulse, ForceMode.Impulse);
            }

            if (destroyOriginal)
                Destroy(gameObject);
        }

        private void OnValidate()
        {
            fragmentCount = Mathf.Max(1, fragmentCount);
            spawnRadius = Mathf.Max(0f, spawnRadius);
            impulse = Mathf.Max(0f, impulse);
        }
    }
}

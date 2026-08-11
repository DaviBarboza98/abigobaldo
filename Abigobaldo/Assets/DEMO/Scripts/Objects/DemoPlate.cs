using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoPlate : MonoBehaviour, IDemoInteractable
    {
        [System.Serializable]
        private struct PlatedFoodVisual
        {
            public DemoObjectKind kind;
            public GameObject visualPrefab;
        }

        [SerializeField] private Transform foodRoot;
        [SerializeField] private PlatedFoodVisual[] platedFoodVisuals;

        private DemoObjectKind contentKind = DemoObjectKind.None;
        private GameObject contentVisual;

        public bool IsEmpty => contentKind == DemoObjectKind.None;
        public DemoObjectKind ContentKind => contentKind;

        public void Interact(DemoPlayerInteractor player)
        {
            TryPlateHeldReadyFood(player);
        }

        public bool TrySetFood(DemoObjectKind kind, GameObject visualPrefab)
        {
            if (!IsEmpty || kind == DemoObjectKind.None)
                return false;

            contentKind = kind;
            visualPrefab ??= GetPlatedVisual(kind);

            if (visualPrefab != null)
            {
                Transform root = foodRoot != null ? foodRoot : transform;
                contentVisual = Instantiate(visualPrefab, root.position, root.rotation, root);
                contentVisual.name = visualPrefab.name;
                contentVisual.transform.localPosition = Vector3.zero;
                contentVisual.transform.localRotation = Quaternion.identity;
            }

            return true;
        }

        public void Clear()
        {
            contentKind = DemoObjectKind.None;

            if (contentVisual != null)
                Destroy(contentVisual);
        }

        private bool TryPlateHeldReadyFood(DemoPlayerInteractor player)
        {
            if (player == null || player.Holder == null || !IsEmpty)
                return false;

            if (!player.Holder.TryGetHeldIdentity(out DemoObjectIdentity heldIdentity))
                return false;

            DemoCookableItem cookable = player.Holder.CurrentObject != null ? player.Holder.CurrentObject.GetComponent<DemoCookableItem>() : null;

            if (cookable != null && !cookable.CanBePlated)
                return false;

            if (cookable == null && !IsPlateableReadyFood(heldIdentity.Kind))
                return false;

            if (!TrySetFoodFromObject(player.Holder.CurrentObject))
                return false;

            player.Holder.ConsumeHeldObject();
            return true;
        }

        public bool TrySetFoodFromObject(DemoHoldableObject source)
        {
            if (source == null)
                return false;

            DemoObjectIdentity identity = source.GetComponent<DemoObjectIdentity>();

            if (identity == null)
                return false;

            GameObject visualPrefab = GetPlatedVisual(identity.Kind);

            if (visualPrefab != null)
                return TrySetFood(identity.Kind, visualPrefab);

            if (!IsEmpty)
                return false;

            contentKind = identity.Kind;
            Transform root = foodRoot != null ? foodRoot : transform;
            contentVisual = Instantiate(source.gameObject, root.position, root.rotation, root);
            contentVisual.name = $"Visual_{source.name}";
            contentVisual.transform.localPosition = Vector3.zero;
            contentVisual.transform.localRotation = Quaternion.identity;
            StripRuntimeComponents(contentVisual);
            return true;
        }

        private GameObject GetPlatedVisual(DemoObjectKind kind)
        {
            if (platedFoodVisuals == null)
                return null;

            foreach (PlatedFoodVisual entry in platedFoodVisuals)
            {
                if (entry.kind == kind)
                    return entry.visualPrefab;
            }

            return null;
        }

        private static bool IsPlateableReadyFood(DemoObjectKind kind)
        {
            return kind == DemoObjectKind.FriedEgg
                || kind == DemoObjectKind.Omelet
                || kind == DemoObjectKind.Cuscuz
                || kind == DemoObjectKind.RoastedCorn
                || kind == DemoObjectKind.Charcoal;
        }

        private static void StripRuntimeComponents(GameObject target)
        {
            foreach (Rigidbody rigidbody in target.GetComponentsInChildren<Rigidbody>())
                Destroy(rigidbody);

            foreach (Collider collider in target.GetComponentsInChildren<Collider>())
                Destroy(collider);

            foreach (DemoHoldableObject holdableObject in target.GetComponentsInChildren<DemoHoldableObject>())
                Destroy(holdableObject);

            foreach (DemoCookableItem cookableItem in target.GetComponentsInChildren<DemoCookableItem>())
                Destroy(cookableItem);

            foreach (DemoObjectIdentity identity in target.GetComponentsInChildren<DemoObjectIdentity>())
                Destroy(identity);
        }
    }
}

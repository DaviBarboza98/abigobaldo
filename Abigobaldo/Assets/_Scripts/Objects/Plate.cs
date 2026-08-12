using UnityEngine;

namespace Abigobaldo.Game
{
    public class Plate : MonoBehaviour, IInteractable
    {
        [System.Serializable]
        private struct PlatedFoodVisual
        {
            public ObjectKind kind;
            public GameObject visualPrefab;
        }

        [SerializeField] private Transform foodRoot;
        [SerializeField] private PlatedFoodVisual[] platedFoodVisuals;

        private ObjectKind contentKind = ObjectKind.None;
        private GameObject contentVisual;

        public bool IsEmpty => contentKind == ObjectKind.None;
        public ObjectKind ContentKind => contentKind;

        public void Interact(PlayerInteractor player)
        {
            TryPlateHeldReadyFood(player);
        }

        public bool TrySetFood(ObjectKind kind, GameObject visualPrefab)
        {
            if (!IsEmpty || kind == ObjectKind.None)
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
            contentKind = ObjectKind.None;

            if (contentVisual != null)
                Destroy(contentVisual);
        }

        private bool TryPlateHeldReadyFood(PlayerInteractor player)
        {
            if (player == null || player.Holder == null || !IsEmpty)
                return false;

            if (!player.Holder.TryGetHeldIdentity(out ObjectIdentity heldIdentity))
                return false;

            CookableItem cookable = player.Holder.CurrentObject != null ? player.Holder.CurrentObject.GetComponent<CookableItem>() : null;

            if (cookable != null && !cookable.CanBePlated)
                return false;

            if (cookable == null && !IsPlateableReadyFood(heldIdentity.Kind))
                return false;

            if (!TrySetFoodFromObject(player.Holder.CurrentObject))
                return false;

            player.Holder.ConsumeHeldObject();
            return true;
        }

        public bool TrySetFoodFromObject(HoldableObject source)
        {
            if (source == null)
                return false;

            ObjectIdentity identity = source.GetComponent<ObjectIdentity>();

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

        private GameObject GetPlatedVisual(ObjectKind kind)
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

        private static bool IsPlateableReadyFood(ObjectKind kind)
        {
            return kind == ObjectKind.FriedEgg
                || kind == ObjectKind.Omelet
                || kind == ObjectKind.Cuscuz
                || kind == ObjectKind.RoastedCorn
                || kind == ObjectKind.Charcoal;
        }

        private static void StripRuntimeComponents(GameObject target)
        {
            foreach (Rigidbody rigidbody in target.GetComponentsInChildren<Rigidbody>())
                Destroy(rigidbody);

            foreach (Collider collider in target.GetComponentsInChildren<Collider>())
                Destroy(collider);

            foreach (HoldableObject holdableObject in target.GetComponentsInChildren<HoldableObject>())
                Destroy(holdableObject);

            foreach (CookableItem cookableItem in target.GetComponentsInChildren<CookableItem>())
                Destroy(cookableItem);

            foreach (ObjectIdentity identity in target.GetComponentsInChildren<ObjectIdentity>())
                Destroy(identity);
        }
    }
}

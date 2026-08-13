using UnityEngine;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Object Container Visual")]
    public class ObjectVisualPreset : MonoBehaviour
    {
        [System.Serializable]
        private struct Placement
        {
            [Tooltip("Where this object visual is being displayed.")]
            public ObjectVisualTarget target;
            [Tooltip("Local position from the container/item anchor.")]
            public Vector3 localPosition;
            [Tooltip("Local rotation from the container/item anchor.")]
            public Vector3 localEulerAngles;
            [Tooltip("Local scale used only for this visual copy.")]
            public Vector3 localScale;
        }

        [Tooltip("Per-container visual placement. An object can only enter containers listed here.")]
        [SerializeField] private Placement[] placements;

        public bool HasPlacement(ObjectVisualTarget target)
        {
            if (placements == null)
                return false;

            foreach (Placement entry in placements)
            {
                if (entry.target == target)
                    return true;
            }

            return false;
        }

        public static bool HasPlacementFor(GameObject source, ObjectVisualTarget target)
        {
            if (source == null)
                return false;

            ObjectVisualPreset preset = source.GetComponent<ObjectVisualPreset>();
            return preset != null && preset.HasPlacement(target);
        }

        public static GameObject InstantiateFromObject(HoldableObject source, ObjectVisualTarget target, Transform parent)
        {
            if (source == null || parent == null)
                return null;

            GameObject instance = Instantiate(source.gameObject, parent);
            instance.name = $"Visual_{source.name}";
            ApplyPlacement(instance.transform, source.gameObject, target);
            StripRuntimeComponents(instance);
            return instance;
        }

        private static void StripRuntimeComponents(GameObject target)
        {
            foreach (PlateableObject plateable in target.GetComponentsInChildren<PlateableObject>(true))
                DestroyComponent(plateable);

            foreach (RotationTransform rotationTransform in target.GetComponentsInChildren<RotationTransform>(true))
                DestroyComponent(rotationTransform);

            foreach (RecipeProgress progress in target.GetComponentsInChildren<RecipeProgress>(true))
                DestroyComponent(progress);

            foreach (ObjectIdentity identity in target.GetComponentsInChildren<ObjectIdentity>(true))
                DestroyComponent(identity);

            foreach (HoldableObject holdable in target.GetComponentsInChildren<HoldableObject>(true))
                DestroyComponent(holdable);

            foreach (Collider targetCollider in target.GetComponentsInChildren<Collider>(true))
                DestroyComponent(targetCollider);

            foreach (Rigidbody body in target.GetComponentsInChildren<Rigidbody>(true))
                DestroyComponent(body);
        }

        private static void DestroyComponent(Component component)
        {
            if (component == null)
                return;

            DestroyImmediate(component);
        }

        private static void ApplyPlacement(Transform instance, GameObject sourcePresetPrefab, ObjectVisualTarget target)
        {
            instance.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.localScale = Vector3.one;

            ObjectVisualPreset preset = sourcePresetPrefab.GetComponent<ObjectVisualPreset>();

            if (preset == null || !preset.TryGetPlacement(target, out Placement placement))
                return;

            instance.localPosition = placement.localPosition;
            instance.localRotation = Quaternion.Euler(placement.localEulerAngles);
            instance.localScale = SanitizeScale(placement.localScale);
        }

        private bool TryGetPlacement(ObjectVisualTarget target, out Placement placement)
        {
            if (placements != null)
            {
                foreach (Placement entry in placements)
                {
                    if (entry.target == target)
                    {
                        placement = entry;
                        return true;
                    }
                }

            }

            placement = default;
            return false;
        }

        private static Vector3 SanitizeScale(Vector3 scale)
        {
            if (scale == Vector3.zero)
                return Vector3.one;

            return scale;
        }
    }
}

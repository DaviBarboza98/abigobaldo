using UnityEngine;

namespace Abigobaldo.Game
{
    public class ObjectVisualPreset : MonoBehaviour
    {
        [System.Serializable]
        private struct Placement
        {
            public ObjectVisualTarget target;
            public GameObject prefabOverride;
            public Vector3 localPosition;
            public Vector3 localEulerAngles;
            public Vector3 localScale;
        }

        [SerializeField] private Placement[] placements;

        public static GameObject InstantiateFor(GameObject visualPrefab, ObjectVisualTarget target, Transform parent)
        {
            if (visualPrefab == null || parent == null)
                return null;

            GameObject prefabToSpawn = ResolvePrefab(visualPrefab, target);
            GameObject instance = Instantiate(prefabToSpawn, parent);
            instance.name = prefabToSpawn.name;
            ApplyPlacement(instance.transform, visualPrefab, target);
            return instance;
        }

        private static GameObject ResolvePrefab(GameObject visualPrefab, ObjectVisualTarget target)
        {
            ObjectVisualPreset preset = visualPrefab.GetComponent<ObjectVisualPreset>();

            if (preset != null && preset.TryGetPlacement(target, out Placement placement) && placement.prefabOverride != null)
                return placement.prefabOverride;

            return visualPrefab;
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

                foreach (Placement entry in placements)
                {
                    if (entry.target == ObjectVisualTarget.Default)
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

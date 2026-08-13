using UnityEngine;

namespace Abigobaldo.Game
{
    [ExecuteAlways]
    [AddComponentMenu("Abigobaldo/Lighting/Atmosphere")]
    [DisallowMultipleComponent]
    public sealed class LightingAtmosphere : MonoBehaviour
    {
        [SerializeField] private bool settingsEnabled = true;
        [SerializeField] private FogMode mode = FogMode.ExponentialSquared;
        [SerializeField] private Color color = new Color(0.62f, 0.72f, 0.82f, 1f);
        [Tooltip("Fog thickness for Exponential modes. Keep this low for a light cartoon haze.")]
        [Range(0f, 0.1f)]
        [SerializeField] private float density = 0.008f;
        [Tooltip("Distance where Linear fog begins.")]
        [Min(0f)]
        [SerializeField] private float startDistance = 25f;
        [Tooltip("Distance where Linear fog becomes fully dense.")]
        [Min(0.01f)]
        [SerializeField] private float endDistance = 120f;

        public bool SettingsEnabled => settingsEnabled && isActiveAndEnabled;

        internal void Apply()
        {
            if (!SettingsEnabled)
                return;

            RenderSettings.fog = true;
            RenderSettings.fogMode = mode;
            RenderSettings.fogColor = color;
            RenderSettings.fogDensity = density;
            RenderSettings.fogStartDistance = startDistance;
            RenderSettings.fogEndDistance = endDistance;
        }

        private void OnEnable()
        {
            NotifyManager();
        }

        private void OnDisable()
        {
            NotifyManager();
        }

        private void OnValidate()
        {
            density = Mathf.Clamp(density, 0f, 0.1f);
            startDistance = Mathf.Max(0f, startDistance);
            endDistance = Mathf.Max(startDistance + 0.01f, endDistance);
            NotifyManager();
        }

        private void NotifyManager()
        {
            GetComponentInParent<LightingManager>()?.RequestApply();
        }
    }
}

using UnityEngine;

namespace Abigobaldo.Game
{
    [ExecuteAlways]
    [AddComponentMenu("Abigobaldo/Lighting/Sky")]
    [DisallowMultipleComponent]
    public sealed class LightingSky : MonoBehaviour
    {
        [SerializeField] private bool settingsEnabled = true;
        [Tooltip("Use a URP-compatible Skybox material. Its shader defines whether it uses a cubemap or six textures.")]
        [SerializeField] private Material skyboxMaterial;

        public bool SettingsEnabled => settingsEnabled && isActiveAndEnabled;

        internal void Apply()
        {
            if (SettingsEnabled && skyboxMaterial != null)
                RenderSettings.skybox = skyboxMaterial;
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
            NotifyManager();
        }

        private void NotifyManager()
        {
            GetComponentInParent<LightingManager>()?.RequestApply();
        }
    }
}

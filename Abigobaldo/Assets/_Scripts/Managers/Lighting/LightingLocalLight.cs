using UnityEngine;

namespace Abigobaldo.Game
{
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public abstract class LightingLocalLight : MonoBehaviour
    {
        [SerializeField] private bool lightEnabled = true;
        [SerializeField] private Color color = Color.white;
        [Min(0f)]
        [SerializeField] private float brightness = 1f;
        [Min(0f)]
        [SerializeField] private float range = 10f;
        [Tooltip("Baked by default. Mixed keeps a realtime direct-light component and costs more.")]
        [SerializeField] private bool useMixedLighting;
        [SerializeField] private bool shadows;
        [Range(0f, 1f)]
        [SerializeField] private float shadowStrength = 1f;

        private Light targetLight;

        protected abstract LightType ExpectedLightType { get; }
        protected virtual bool SupportsMixedLighting => true;

        internal void Apply(LightingManager.LightingTechnology technology)
        {
            ResolveLight();

            if (targetLight == null)
                return;

            targetLight.enabled = lightEnabled && isActiveAndEnabled;
            targetLight.type = ExpectedLightType;
            targetLight.color = color;
            targetLight.intensity = brightness;
            targetLight.range = range;
#if UNITY_WEBGL
            // WebGL has no Light.lightmapBakeType API. Keep shadow behavior
            // deterministic and let the target platform decide the backend.
            bool bakedShadow = shadows;
#else
            targetLight.lightmapBakeType = useMixedLighting && SupportsMixedLighting
                ? LightmapBakeType.Mixed
                : LightmapBakeType.Baked;
            bool bakedShadow = targetLight.lightmapBakeType == LightmapBakeType.Baked && shadows;
#endif
            bool realtimeShadow = technology == LightingManager.LightingTechnology.Future && shadows;
            targetLight.shadows = technology != LightingManager.LightingTechnology.Voxel && (bakedShadow || realtimeShadow)
                ? LightShadows.Soft
                : LightShadows.None;
            targetLight.shadowStrength = shadowStrength;
            ApplySpecificSettings(targetLight);
        }

        protected virtual void ApplySpecificSettings(Light lightComponent)
        {
        }

        protected virtual void OnEnable()
        {
            NotifyManager();
        }

        protected virtual void OnDisable()
        {
            ResolveLight();

            if (targetLight != null)
                targetLight.enabled = false;

            NotifyManager();
        }

        protected virtual void OnValidate()
        {
            brightness = Mathf.Max(0f, brightness);
            range = Mathf.Max(0f, range);
            shadowStrength = Mathf.Clamp01(shadowStrength);
            NotifyManager();
        }

        private void ResolveLight()
        {
            if (targetLight == null)
                targetLight = GetComponent<Light>();
        }

        private void NotifyManager()
        {
            LightingManager manager = GetComponentInParent<LightingManager>();

            if (manager != null)
                manager.RequestApply();
            else
                Apply(LightingManager.LightingTechnology.ShadowMap);
        }
    }
}

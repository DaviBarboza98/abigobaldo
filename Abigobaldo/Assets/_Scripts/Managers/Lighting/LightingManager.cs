using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Abigobaldo.Game
{
    [ExecuteAlways]
    [DefaultExecutionOrder(100)]
    [AddComponentMenu("Abigobaldo/Managers/Lighting Manager")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Volume))]
    public sealed class LightingManager : MonoBehaviour
    {
        public enum LightingTechnology
        {
            Voxel,
            ShadowMap,
            Future
        }

        [Header("Technology")]
        [Tooltip("Voxel disables realtime shadows. ShadowMap keeps sun shadows. Future also permits local realtime shadows.")]
        [SerializeField] private LightingTechnology technology = LightingTechnology.ShadowMap;

        [Header("Time And Sun")]
        [SerializeField] private Light sun;
        [Range(0f, 24f)]
        [SerializeField] private float clockTime = 15.15f;
        [Range(-90f, 90f)]
        [SerializeField] private float geographicLatitude = -8f;
        [Range(-180f, 180f)]
        [SerializeField] private float sunAzimuth = -150f;
        [SerializeField] private bool rotateSunFromClock = true;
        [SerializeField] private Color sunColor = new Color(1f, 0.82f, 0.52f, 1f);
        [Min(0f)]
        [SerializeField] private float brightness = 1.35f;
        [SerializeField] private bool sunUsesMixedLighting = false;
        [SerializeField] private LightShadows sunShadows = LightShadows.Hard;
        [Range(0f, 1f)]
        [SerializeField] private float sunShadowStrength = 0.58f;

        [Header("Ambient")]
        [SerializeField] private Color ambient = new Color(0.38f, 0.42f, 0.5f, 1f);
        [SerializeField] private Color outdoorAmbient = new Color(0.62f, 0.72f, 0.9f, 1f);
        [SerializeField] private Color colorShiftTop = Color.black;
        [SerializeField] private Color colorShiftBottom = new Color(0.18f, 0.08f, 0f, 1f);
        [SerializeField] private Color shadowColor = new Color(0.42f, 0.478f, 0.627f, 1f);
        [Range(0f, 2f)]
        [SerializeField] private float environmentDiffuseScale = 1f;
        [Range(0f, 2f)]
        [SerializeField] private float environmentSpecularScale = 0.55f;

        [Header("Rendering")]
        [Min(0f)]
        [SerializeField] private float shadowDistance = 24f;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float postProcessingPriority = 100f;

        [Header("Kitchen warmth")]
        [Tooltip("Adds a cheap warm fill light to each LightBulb prop in the active scene.")]
        [SerializeField] private bool lightKitchenBulbs = true;
        [SerializeField] private Color kitchenBulbColor = new Color(1f, 0.62f, 0.28f, 1f);
        [Min(0f)] [SerializeField] private float kitchenBulbIntensity = 1.8f;
        [Min(0f)] [SerializeField] private float kitchenBulbRange = 8f;

        private Volume globalVolume;
        private VolumeProfile runtimeProfile;
        private bool isApplying;

        public LightingTechnology Technology => technology;

        /// <summary>Lets gameplay advance the same lighting setup through the day.</summary>
        public void SetClockTime(float value)
        {
            clockTime = Mathf.Repeat(value, 24f);
            ApplyLighting();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplyLighting();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(ApplyAfterSceneSettings());
        }

        private IEnumerator ApplyAfterSceneSettings()
        {
            yield return null;
            ApplyLighting();
        }

        private void Start()
        {
            ApplyLighting();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (globalVolume != null && globalVolume.sharedProfile == runtimeProfile)
            {
                globalVolume.sharedProfile = null;
                globalVolume.enabled = false;
            }

            DestroyRuntimeProfile();
        }

        private void OnTransformChildrenChanged()
        {
            ApplyLighting();
        }

        [ContextMenu("Apply Lighting")]
        public void ApplyLighting()
        {
            if (isApplying)
                return;

            isApplying = true;

            try
            {
                ResolveReferences();
                ApplyAmbientLighting();
                ApplySunLighting();
                ApplySkyAndAtmosphere();
                ApplyLocalLights();
                ApplyKitchenBulbLights();
                ApplyPostProcessing();
                ApplyPipelineSettings();
            }
            finally
            {
                isApplying = false;
            }
        }

        internal void RequestApply()
        {
            if (isActiveAndEnabled)
                ApplyLighting();
        }

        private void ResolveReferences()
        {
            if (sun == null)
            {
                foreach (Light childLight in GetComponentsInChildren<Light>(true))
                {
                    if (childLight != null && childLight.type == LightType.Directional)
                    {
                        sun = childLight;
                        break;
                    }
                }
            }

            if (sun == null && RenderSettings.sun != null)
                sun = RenderSettings.sun;

            if (targetCamera == null)
            {
                // The player camera is intentionally not tagged MainCamera. In a
                // build, FindObjectOfType could therefore select DialogueCamera,
                // leaving post-processing disabled on the actual game camera.
                PlayerInteractor player = FindObjectOfType<PlayerInteractor>();
                targetCamera = player != null && player.PlayerCamera != null
                    ? player.PlayerCamera
                    : Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            }

            if (globalVolume == null)
                globalVolume = GetComponent<Volume>();
        }

        private void ApplyAmbientLighting()
        {
            Color skyColor = AddColors(outdoorAmbient, colorShiftTop);
            Color groundColor = AddColors(ambient, colorShiftBottom);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = skyColor;
            RenderSettings.ambientEquatorColor = Color.Lerp(groundColor, skyColor, 0.5f);
            RenderSettings.ambientGroundColor = groundColor;
            RenderSettings.ambientIntensity = environmentDiffuseScale;
            RenderSettings.reflectionIntensity = environmentSpecularScale;
            RenderSettings.subtractiveShadowColor = shadowColor;
        }

        private void ApplySunLighting()
        {
            if (sun == null)
                return;

            if (rotateSunFromClock)
            {
                float pitch = (clockTime - 6f) * 15f;
                sun.transform.rotation = Quaternion.Euler(pitch, sunAzimuth, geographicLatitude);
            }

            sun.type = LightType.Directional;
            sun.color = sunColor;
            sun.intensity = brightness;
            // Keep direct lighting independent from any baked-lightmap setup.
            sun.shadows = technology == LightingTechnology.Voxel ? LightShadows.None : sunShadows;
            sun.shadowStrength = sunShadowStrength;
            RenderSettings.sun = sun;
        }

        private void ApplySkyAndAtmosphere()
        {
            LightingSky[] skies = GetComponentsInChildren<LightingSky>(true);

            foreach (LightingSky sky in skies)
            {
                if (sky != null && sky.SettingsEnabled)
                {
                    sky.Apply();
                    break;
                }
            }

            LightingAtmosphere[] atmospheres = GetComponentsInChildren<LightingAtmosphere>(true);

            if (atmospheres.Length == 0)
                return;

            RenderSettings.fog = false;

            foreach (LightingAtmosphere atmosphere in atmospheres)
            {
                if (atmosphere != null && atmosphere.SettingsEnabled)
                {
                    atmosphere.Apply();
                    break;
                }
            }
        }

        private void ApplyLocalLights()
        {
            foreach (LightingLocalLight localLight in GetComponentsInChildren<LightingLocalLight>(true))
            {
                if (localLight != null)
                    localLight.Apply(technology);
            }
        }

        // The light-bulb meshes were only decorative before. Using them as anchors
        // gives the kitchen the warm practical-light highlights from the reference
        // without adding costly realtime shadows to the WebGL build.
        private void ApplyKitchenBulbLights()
        {
            if (!lightKitchenBulbs || !gameObject.scene.IsValid())
                return;

            foreach (Transform transform in FindObjectsOfType<Transform>(true))
            {
                if (transform == null || transform.gameObject.scene != gameObject.scene || transform.name != "LightBulb")
                    continue;

                Light bulbLight = transform.GetComponent<Light>();
                if (bulbLight == null)
                {
                    bulbLight = transform.gameObject.AddComponent<Light>();
                    bulbLight.name = "Kitchen Bulb Light";
                }

                bulbLight.enabled = true;
                bulbLight.type = LightType.Point;
                bulbLight.color = kitchenBulbColor;
                bulbLight.intensity = kitchenBulbIntensity;
                bulbLight.range = kitchenBulbRange;
                bulbLight.renderMode = LightRenderMode.ForceVertex;
                bulbLight.shadows = LightShadows.None;
            }
        }

        private void ApplyPostProcessing()
        {
            LightingPostEffect[] effects = GetComponentsInChildren<LightingPostEffect>(true);
            bool hasActiveEffect = false;
            bool requiresHdr = false;

            RebuildRuntimeProfile();

            foreach (LightingPostEffect effect in effects)
            {
                if (effect == null || !effect.SettingsEnabled)
                    continue;

                effect.Apply(runtimeProfile);
                hasActiveEffect = true;
                requiresHdr |= effect.RequiresHdr;
            }

            globalVolume.isGlobal = true;
            globalVolume.priority = postProcessingPriority;
            globalVolume.weight = 1f;
            globalVolume.sharedProfile = runtimeProfile;
            globalVolume.enabled = hasActiveEffect;

            if (!hasActiveEffect)
                return;

            // The menu transition can change camera discovery order. Apply the
            // MainGame volume to every active gameplay camera so the player view
            // never loses its color correction when it was opened from the menu.
            foreach (Camera camera in FindObjectsOfType<Camera>(true))
            {
                if (camera == null || !camera.gameObject.scene.IsValid()) continue;
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = true;
                if (requiresHdr) camera.allowHDR = true;
            }
        }

        private void ApplyPipelineSettings()
        {
            UniversalRenderPipelineAsset pipelineAsset = UniversalRenderPipeline.asset;

            if (pipelineAsset != null)
                pipelineAsset.shadowDistance = technology == LightingTechnology.Voxel ? 0f : shadowDistance;
        }

        private void RebuildRuntimeProfile()
        {
            if (runtimeProfile == null)
            {
                runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                runtimeProfile.name = "Lighting Runtime Profile";
                runtimeProfile.hideFlags = HideFlags.HideAndDontSave;
                return;
            }

            for (int i = runtimeProfile.components.Count - 1; i >= 0; i--)
            {
                VolumeComponent component = runtimeProfile.components[i];

                if (component == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(component);
                else
                    DestroyImmediate(component);
            }

            runtimeProfile.components.Clear();
        }

        private void DestroyRuntimeProfile()
        {
            if (runtimeProfile == null)
                return;

            if (Application.isPlaying)
                Destroy(runtimeProfile);
            else
                DestroyImmediate(runtimeProfile);

            runtimeProfile = null;
        }

        private static Color AddColors(Color first, Color second)
        {
            return new Color(
                Mathf.Clamp01(first.r + second.r),
                Mathf.Clamp01(first.g + second.g),
                Mathf.Clamp01(first.b + second.b),
                1f);
        }

#if UNITY_EDITOR
        [ContextMenu("Bake Lighting Now")]
        private void BakeLightingNow()
        {
            ApplyLighting();
            UnityEditor.Lightmapping.BakeAsync();
        }
#endif

        private void OnValidate()
        {
            clockTime = Mathf.Repeat(clockTime, 24f);
            geographicLatitude = Mathf.Clamp(geographicLatitude, -90f, 90f);
            sunAzimuth = Mathf.Clamp(sunAzimuth, -180f, 180f);
            brightness = Mathf.Max(0f, brightness);
            sunShadowStrength = Mathf.Clamp01(sunShadowStrength);
            environmentDiffuseScale = Mathf.Clamp(environmentDiffuseScale, 0f, 2f);
            environmentSpecularScale = Mathf.Clamp(environmentSpecularScale, 0f, 2f);
            shadowDistance = Mathf.Max(0f, shadowDistance);
            ApplyLighting();
        }
    }
}

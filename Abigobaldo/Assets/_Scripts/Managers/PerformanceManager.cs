using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Managers/Performance Manager")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(200)]
    public sealed class PerformanceManager : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private float fixedDeltaTime = 0.02f;
        [SerializeField] private float shadowDistance = 24f;
        [SerializeField] private int pixelLightCount = 1;
        [SerializeField] private bool reusePhysicsCollisionCallbacks = true;

        [Header("Camera")]
        [SerializeField] private bool optimizeCameras = true;
        [SerializeField] private float cameraFarClip = 120f;
        [SerializeField] private bool forceOcclusionCulling = true;
        [SerializeField] private bool disableCameraHdr = true;

        [Header("Visibility Culling")]
        [SerializeField] private bool enableVisibilityCulling = true;
        [SerializeField] private float visibilityCheckInterval = 0.12f;
        [SerializeField] private float rendererRefreshInterval = 5f;
        [SerializeField] private float cullingBoundsPadding = 0.35f;
        [SerializeField] private float alwaysVisibleDistance = 4f;
        [SerializeField] private float maxVisibleDistance = 120f;

        [Header("WebGL Profile")]
        [SerializeField] private bool applyWebGlProfile = true;
        [SerializeField] private int webGlTargetFrameRate = 60;
        [SerializeField] private float webGlShadowDistance = 12f;
        [SerializeField] private float webGlCameraFarClip = 75f;
        [SerializeField, Range(0.5f, 1f)] private float webGlRenderScale = 0.8f;
        [SerializeField] private int webGlMsaaSamples = 1;
        [SerializeField] private float webGlVisibilityCheckInterval = 0.22f;
        [SerializeField] private float webGlRendererRefreshInterval = 8f;
        [SerializeField] private float webGlMaxVisibleDistance = 75f;

        private UniversalRenderPipelineAsset runtimePipelineAsset;
        private RenderPipelineAsset originalQualityPipelineAsset;

        private void Awake()
        {
            bool useWebGlProfile = applyWebGlProfile && Application.platform == RuntimePlatform.WebGLPlayer;
            int effectiveTargetFrameRate = useWebGlProfile ? webGlTargetFrameRate : targetFrameRate;
            float effectiveShadowDistance = useWebGlProfile ? webGlShadowDistance : shadowDistance;
            float effectiveCameraFarClip = useWebGlProfile ? webGlCameraFarClip : cameraFarClip;
            float effectiveVisibilityCheckInterval = useWebGlProfile
                ? webGlVisibilityCheckInterval
                : visibilityCheckInterval;
            float effectiveRendererRefreshInterval = useWebGlProfile
                ? webGlRendererRefreshInterval
                : rendererRefreshInterval;
            float effectiveMaxVisibleDistance = useWebGlProfile
                ? webGlMaxVisibleDistance
                : maxVisibleDistance;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = effectiveTargetFrameRate;
            Time.fixedDeltaTime = fixedDeltaTime;
            QualitySettings.shadowDistance = effectiveShadowDistance;
            QualitySettings.pixelLightCount = pixelLightCount;
            Physics.reuseCollisionCallbacks = reusePhysicsCollisionCallbacks;

            if (useWebGlProfile)
            {
                QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
                QualitySettings.realtimeReflectionProbes = false;
                ApplyWebGlUrpProfile(effectiveShadowDistance);
            }

            if (optimizeCameras)
                OptimizeCameras(effectiveCameraFarClip);

            if (enableVisibilityCulling)
            {
                EnsureVisibilityCuller(
                    effectiveVisibilityCheckInterval,
                    effectiveRendererRefreshInterval,
                    effectiveMaxVisibleDistance);
            }
        }

        private void Start()
        {
            if (runtimePipelineAsset == null)
                return;

            runtimePipelineAsset.renderScale = webGlRenderScale;
            runtimePipelineAsset.shadowDistance = webGlShadowDistance;
            runtimePipelineAsset.msaaSampleCount = NormalizeMsaaSamples(webGlMsaaSamples);
        }

        private void OptimizeCameras(float effectiveFarClip)
        {
            foreach (Camera targetCamera in FindObjectsOfType<Camera>(true))
            {
                if (targetCamera == null)
                    continue;

                if (effectiveFarClip > targetCamera.nearClipPlane)
                    targetCamera.farClipPlane = effectiveFarClip;

                if (forceOcclusionCulling)
                    targetCamera.useOcclusionCulling = true;

                // Bloom and the color volume are part of the game's look. HDR is
                // kept in WebGL so the browser build matches the Editor.
                if (disableCameraHdr && Application.platform != RuntimePlatform.WebGLPlayer)
                    targetCamera.allowHDR = false;
            }
        }

        private void EnsureVisibilityCuller(
            float effectiveCheckInterval,
            float effectiveRefreshInterval,
            float effectiveMaxVisibleDistance)
        {
            RuntimeVisibilityCuller culler = FindObjectOfType<RuntimeVisibilityCuller>();

            if (culler == null)
                culler = gameObject.AddComponent<RuntimeVisibilityCuller>();

            culler.Configure(
                ResolvePlayerCamera(),
                effectiveCheckInterval,
                effectiveRefreshInterval,
                cullingBoundsPadding,
                alwaysVisibleDistance,
                effectiveMaxVisibleDistance);
        }

        private void ApplyWebGlUrpProfile(float effectiveShadowDistance)
        {
            UniversalRenderPipelineAsset sourceAsset = UniversalRenderPipeline.asset;

            if (sourceAsset == null)
                return;

            originalQualityPipelineAsset = QualitySettings.renderPipeline;
            runtimePipelineAsset = Instantiate(sourceAsset);
            runtimePipelineAsset.name = $"{sourceAsset.name} (WebGL Runtime)";
            runtimePipelineAsset.renderScale = webGlRenderScale;
            runtimePipelineAsset.shadowDistance = effectiveShadowDistance;
            runtimePipelineAsset.msaaSampleCount = NormalizeMsaaSamples(webGlMsaaSamples);
            QualitySettings.renderPipeline = runtimePipelineAsset;
        }

        private static int NormalizeMsaaSamples(int samples)
        {
            if (samples >= 8)
                return 8;

            if (samples >= 4)
                return 4;

            return samples >= 2 ? 2 : 1;
        }

        private static Camera ResolvePlayerCamera()
        {
            PlayerInteractor player = FindObjectOfType<PlayerInteractor>();

            if (player != null && player.PlayerCamera != null)
                return player.PlayerCamera;

            if (Camera.main != null)
                return Camera.main;

            return FindObjectOfType<Camera>();
        }

        private void OnDestroy()
        {
            if (runtimePipelineAsset == null)
                return;

            if (QualitySettings.renderPipeline == runtimePipelineAsset)
                QualitySettings.renderPipeline = originalQualityPipelineAsset;

            Destroy(runtimePipelineAsset);
        }

        private void OnValidate()
        {
            targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 144);
            fixedDeltaTime = Mathf.Clamp(fixedDeltaTime, 0.01f, 0.05f);
            shadowDistance = Mathf.Max(0f, shadowDistance);
            pixelLightCount = Mathf.Clamp(pixelLightCount, 0, 8);
            cameraFarClip = Mathf.Max(1f, cameraFarClip);
            visibilityCheckInterval = Mathf.Max(0.03f, visibilityCheckInterval);
            rendererRefreshInterval = Mathf.Max(0.25f, rendererRefreshInterval);
            cullingBoundsPadding = Mathf.Max(0f, cullingBoundsPadding);
            alwaysVisibleDistance = Mathf.Max(0f, alwaysVisibleDistance);
            maxVisibleDistance = Mathf.Max(0f, maxVisibleDistance);
            webGlTargetFrameRate = Mathf.Clamp(webGlTargetFrameRate, 30, 144);
            webGlShadowDistance = Mathf.Max(0f, webGlShadowDistance);
            webGlCameraFarClip = Mathf.Max(1f, webGlCameraFarClip);
            webGlRenderScale = Mathf.Clamp(webGlRenderScale, 0.5f, 1f);
            webGlMsaaSamples = NormalizeMsaaSamples(webGlMsaaSamples);
            webGlVisibilityCheckInterval = Mathf.Max(0.03f, webGlVisibilityCheckInterval);
            webGlRendererRefreshInterval = Mathf.Max(0.25f, webGlRendererRefreshInterval);
            webGlMaxVisibleDistance = Mathf.Max(0f, webGlMaxVisibleDistance);
        }
    }
}

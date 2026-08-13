using UnityEngine;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Managers/Performance Manager")]
    [DisallowMultipleComponent]
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
        [SerializeField] private float rendererRefreshInterval = 1.5f;
        [SerializeField] private float cullingBoundsPadding = 0.35f;
        [SerializeField] private float alwaysVisibleDistance = 4f;
        [SerializeField] private float maxVisibleDistance = 120f;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
            Time.fixedDeltaTime = fixedDeltaTime;
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.pixelLightCount = pixelLightCount;
            Physics.reuseCollisionCallbacks = reusePhysicsCollisionCallbacks;

            if (optimizeCameras)
                OptimizeCameras();

            if (enableVisibilityCulling)
                EnsureVisibilityCuller();
        }

        private void OptimizeCameras()
        {
            foreach (Camera targetCamera in FindObjectsOfType<Camera>(true))
            {
                if (targetCamera == null)
                    continue;

                if (cameraFarClip > targetCamera.nearClipPlane)
                    targetCamera.farClipPlane = cameraFarClip;

                if (forceOcclusionCulling)
                    targetCamera.useOcclusionCulling = true;

                if (disableCameraHdr)
                    targetCamera.allowHDR = false;
            }
        }

        private void EnsureVisibilityCuller()
        {
            RuntimeVisibilityCuller culler = FindObjectOfType<RuntimeVisibilityCuller>();

            if (culler == null)
                culler = gameObject.AddComponent<RuntimeVisibilityCuller>();

            culler.Configure(
                ResolvePlayerCamera(),
                visibilityCheckInterval,
                rendererRefreshInterval,
                cullingBoundsPadding,
                alwaysVisibleDistance,
                maxVisibleDistance);
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
        }
    }
}

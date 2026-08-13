using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abigobaldo.Game
{
    public class RuntimeVisibilityCuller : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask cullingLayers = ~0;
        [SerializeField] private float checkInterval = 0.12f;
        [SerializeField] private float rendererRefreshInterval = 1.5f;
        [SerializeField] private float boundsPadding = 0.35f;
        [SerializeField] private float alwaysVisibleDistance = 4f;
        [SerializeField] private float maxVisibleDistance = 120f;
        [SerializeField] private bool skipHoldableObjects = true;
        [SerializeField] private bool disableMotionVectors = true;

        private readonly List<Renderer> renderers = new List<Renderer>(256);
        private readonly HashSet<Renderer> managedRenderers = new HashSet<Renderer>();
        private readonly HashSet<Renderer> disabledByCuller = new HashSet<Renderer>();
        private readonly Plane[] frustumPlanes = new Plane[6];

        private float nextCheckTime;
        private float nextRefreshTime;

        public void Configure(
            Camera camera,
            float newCheckInterval,
            float newRefreshInterval,
            float newBoundsPadding,
            float newAlwaysVisibleDistance,
            float newMaxVisibleDistance)
        {
            targetCamera = camera;
            checkInterval = Mathf.Max(0.03f, newCheckInterval);
            rendererRefreshInterval = Mathf.Max(0.25f, newRefreshInterval);
            boundsPadding = Mathf.Max(0f, newBoundsPadding);
            alwaysVisibleDistance = Mathf.Max(0f, newAlwaysVisibleDistance);
            maxVisibleDistance = Mathf.Max(0f, newMaxVisibleDistance);
            RefreshRendererList();
        }

        private void Start()
        {
            ResolveCamera();
            RefreshRendererList();
            UpdateVisibility(true);
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextRefreshTime)
                RefreshRendererList();

            if (Time.unscaledTime < nextCheckTime)
                return;

            UpdateVisibility(false);
        }

        private void OnDisable()
        {
            RestoreRenderers();
        }

        private void ResolveCamera()
        {
            if (targetCamera != null)
                return;

            targetCamera = Camera.main;

            if (targetCamera != null)
                return;

            PlayerInteractor player = FindObjectOfType<PlayerInteractor>();

            if (player != null && player.PlayerCamera != null)
            {
                targetCamera = player.PlayerCamera;
                return;
            }

            targetCamera = FindObjectOfType<Camera>();
        }

        private void RefreshRendererList()
        {
            nextRefreshTime = Time.unscaledTime + rendererRefreshInterval;
            CleanupDeadReferences();
            ResolveCamera();

            Renderer[] sceneRenderers = FindObjectsOfType<Renderer>(true);

            foreach (Renderer targetRenderer in sceneRenderers)
            {
                if (!CanManage(targetRenderer) || !managedRenderers.Add(targetRenderer))
                    continue;

                if (disableMotionVectors)
                    targetRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

                targetRenderer.allowOcclusionWhenDynamic = true;
                renderers.Add(targetRenderer);
            }
        }

        private bool CanManage(Renderer targetRenderer)
        {
            if (targetRenderer == null || !targetRenderer.gameObject.activeInHierarchy)
                return false;

            if ((cullingLayers.value & (1 << targetRenderer.gameObject.layer)) == 0)
                return false;

            if (targetRenderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly)
                return false;

            VisibilityCullable marker = targetRenderer.GetComponentInParent<VisibilityCullable>();

            if (marker != null && marker.NeverCull)
                return false;

            if (targetRenderer.GetComponentInParent<PlayerInteractor>() != null)
                return false;

            if (skipHoldableObjects && targetRenderer.GetComponentInParent<HoldableObject>() != null)
                return false;

            return true;
        }

        private void UpdateVisibility(bool force)
        {
            if (targetCamera == null)
            {
                ResolveCamera();

                if (targetCamera == null)
                    return;
            }

            nextCheckTime = Time.unscaledTime + checkInterval;
            GeometryUtility.CalculateFrustumPlanes(targetCamera, frustumPlanes);
            Vector3 cameraPosition = targetCamera.transform.position;

            foreach (Renderer targetRenderer in renderers)
            {
                if (targetRenderer == null)
                    continue;

                if (!targetRenderer.gameObject.activeInHierarchy)
                    continue;

                bool shouldBeVisible = ShouldBeVisible(targetRenderer, cameraPosition);

                if (shouldBeVisible)
                {
                    if (disabledByCuller.Remove(targetRenderer))
                        targetRenderer.enabled = true;

                    continue;
                }

                if ((force || targetRenderer.enabled) && !disabledByCuller.Contains(targetRenderer))
                {
                    targetRenderer.enabled = false;
                    disabledByCuller.Add(targetRenderer);
                }
            }
        }

        private bool ShouldBeVisible(Renderer targetRenderer, Vector3 cameraPosition)
        {
            Bounds bounds = targetRenderer.bounds;
            VisibilityCullable marker = targetRenderer.GetComponentInParent<VisibilityCullable>();
            float padding = boundsPadding + (marker != null ? marker.ExtraBoundsPadding : 0f);

            if (padding > 0f)
                bounds.Expand(padding);

            float alwaysDistance = marker != null && marker.AlwaysVisibleDistanceOverride >= 0f
                ? marker.AlwaysVisibleDistanceOverride
                : alwaysVisibleDistance;

            float sqrDistance = bounds.SqrDistance(cameraPosition);

            if (sqrDistance <= alwaysDistance * alwaysDistance)
                return true;

            if (maxVisibleDistance > 0f && sqrDistance > maxVisibleDistance * maxVisibleDistance)
                return false;

            return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        }

        private void CleanupDeadReferences()
        {
            for (int i = renderers.Count - 1; i >= 0; i--)
            {
                Renderer targetRenderer = renderers[i];

                if (targetRenderer != null)
                    continue;

                renderers.RemoveAt(i);
                managedRenderers.Remove(targetRenderer);
            }

            disabledByCuller.RemoveWhere(targetRenderer => targetRenderer == null);
        }

        private void RestoreRenderers()
        {
            foreach (Renderer targetRenderer in disabledByCuller)
            {
                if (targetRenderer != null)
                    targetRenderer.enabled = true;
            }

            disabledByCuller.Clear();
        }

        private void OnValidate()
        {
            checkInterval = Mathf.Max(0.03f, checkInterval);
            rendererRefreshInterval = Mathf.Max(0.25f, rendererRefreshInterval);
            boundsPadding = Mathf.Max(0f, boundsPadding);
            alwaysVisibleDistance = Mathf.Max(0f, alwaysVisibleDistance);
            maxVisibleDistance = Mathf.Max(0f, maxVisibleDistance);
        }
    }
}

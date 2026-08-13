using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private string[] hiddenFirstPersonParts = { "Head" };

        [SerializeField] private float sensitivity = 0.2f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float defaultFov = 60f;
        [SerializeField] private float runningFov = 75f;
        [SerializeField] private float fovSmoothSpeed = 8f;

        private PlayerInput input;
        private PlayerMovement movement;
        private float pitch;
        private readonly List<Renderer> hiddenRenderers = new List<Renderer>();

        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            movement = GetComponent<PlayerMovement>();

            if (cameraPivot == null)
                cameraPivot = FindDeepChild(transform, "CameraPivot");

            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            if (modelRoot == null)
                modelRoot = FindDeepChild(transform, "Model");

            CacheHiddenRenderers();
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        }

        private void Update()
        {
            Look();
            UpdateFov();
        }

        private void CacheHiddenRenderers()
        {
            hiddenRenderers.Clear();

            if (modelRoot == null || hiddenFirstPersonParts == null)
                return;

            foreach (string partName in hiddenFirstPersonParts)
            {
                Transform part = FindDeepChild(modelRoot, partName);

                if (part == null)
                    continue;

                foreach (Renderer targetRenderer in part.GetComponentsInChildren<Renderer>())
                {
                    if (targetRenderer != null)
                        hiddenRenderers.Add(targetRenderer);
                }
            }
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (hiddenRenderers.Count == 0)
                return;

            // Oculta a cabeça (ShadowsOnly) apenas para a câmera do jogador (Game View).
            // Na câmera da janela Scene (SceneView), a cabeça permanece 100% visível!
            bool isPlayerCamera = renderingCamera == playerCamera;

            foreach (Renderer r in hiddenRenderers)
            {
                if (r != null)
                    r.shadowCastingMode = isPlayerCamera ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            }
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (hiddenRenderers.Count == 0)
                return;

            foreach (Renderer r in hiddenRenderers)
            {
                if (r != null)
                    r.shadowCastingMode = ShadowCastingMode.On;
            }
        }

        private void Look()
        {
            if (Cursor.lockState != CursorLockMode.Locked || input.RotateHeld)
                return;

            transform.rotation *= Quaternion.Euler(0f, input.Look.x * sensitivity, 0f);
            pitch = Mathf.Clamp(pitch - input.Look.y * sensitivity, minPitch, maxPitch);

            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void UpdateFov()
        {
            if (playerCamera == null)
                return;

            float targetFov = movement.IsRunning ? runningFov : defaultFov;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, fovSmoothSpeed * Time.deltaTime);
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
                return null;

            if (root.name == childName)
                return root;

            foreach (Transform child in root)
            {
                Transform result = FindDeepChild(child, childName);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}

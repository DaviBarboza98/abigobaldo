using UnityEngine;

namespace Abigobaldo.Game
{
    [DefaultExecutionOrder(-200)]
    [AddComponentMenu("Abigobaldo/Managers/Gameplay Manager")]
    [DisallowMultipleComponent]
    public sealed class GameplayManager : MonoBehaviour
    {
        [Header("Held Object Rotation")]
        [Tooltip("Degrees applied for each pixel of mouse movement while R is held.")]
        [SerializeField] private float heldRotationSensitivity = 0.18f;
        [Tooltip("Smooths noisy mouse input without making the object feel delayed.")]
        [SerializeField] private float heldRotationSmoothTime = 0.045f;
        [Tooltip("Limits a single mouse input spike so the held object never jumps abruptly.")]
        [SerializeField] private float heldRotationMaximumMouseDelta = 24f;
        [Tooltip("Mouse movement below this value does not count as active mixing.")]
        [SerializeField] private float heldRotationMovementDeadZone = 0.35f;
        [Tooltip("Maximum visual rotation speed of a held object.")]
        [SerializeField] private float heldRotationMaximumDegreesPerSecond = 360f;

        public static GameplayManager Instance { get; private set; }

        public float HeldRotationSensitivity => heldRotationSensitivity;
        public float HeldRotationSmoothTime => heldRotationSmoothTime;
        public float HeldRotationMaximumMouseDelta => heldRotationMaximumMouseDelta;
        public float HeldRotationMovementDeadZone => heldRotationMovementDeadZone;
        public float HeldRotationMaximumDegreesPerSecond => heldRotationMaximumDegreesPerSecond;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Only one GameplayManager should exist in a scene.", this);
                enabled = false;
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnValidate()
        {
            heldRotationSensitivity = Mathf.Max(0f, heldRotationSensitivity);
            heldRotationSmoothTime = Mathf.Clamp(heldRotationSmoothTime, 0f, 0.25f);
            heldRotationMaximumMouseDelta = Mathf.Max(1f, heldRotationMaximumMouseDelta);
            heldRotationMovementDeadZone = Mathf.Max(0f, heldRotationMovementDeadZone);
            heldRotationMaximumDegreesPerSecond = Mathf.Max(1f, heldRotationMaximumDegreesPerSecond);
        }
    }
}

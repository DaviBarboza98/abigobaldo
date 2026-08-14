using UnityEngine;

namespace Abigobaldo.Game
{
    public class OpenableDoor : MonoBehaviour, IHoldInteractable, IBodyPushable
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }

        [SerializeField] private Transform pivot;
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;
        [SerializeField] private float maxOpenAngle = 90f;
        [SerializeField] private float followSpeed = 30f;
        [SerializeField] private float mouseSensitivity = 0.25f;
        [SerializeField] private bool invertDirection;
        [SerializeField] private bool startOpen;
        [SerializeField] private bool useMouseDelta = true;
        [SerializeField] private bool logAngleOnRelease;
        [SerializeField] private bool logHoldEvents;

        [Header("Body Push")]
        [SerializeField] private bool bodyPushEnabled = true;
        [SerializeField] private float bodyPushDegreesPerMeter = 45f;
        [SerializeField] private float minimumBodyPushTorque = 0.08f;

        private Quaternion closedLocalRotation;
        private float currentAngle;
        private float holdStartYaw;
        private float holdStartAngle;
        private bool holding;

        private Transform Pivot => pivot != null ? pivot : transform;

        private void Awake()
        {
            closedLocalRotation = Pivot.localRotation;

            if (startOpen)
            {
                currentAngle = maxOpenAngle;
                ApplyAngle(currentAngle);
            }
        }

        public void BeginHold(PlayerInteractor player)
        {
            holding = true;
            holdStartAngle = currentAngle;
            holdStartYaw = player != null && player.PlayerCamera != null
                ? player.PlayerCamera.transform.eulerAngles.y
                : transform.eulerAngles.y;

            if (logHoldEvents)
                Debug.Log($"{name}: began door hold.", this);
        }

        public void UpdateHold(PlayerInteractor player)
        {
            if (!holding || player == null || player.PlayerCamera == null)
                return;

            float direction = invertDirection ? 1f : -1f;
            float targetAngle;

            if (useMouseDelta)
            {
                PlayerInput input = player.GetComponent<PlayerInput>();
                float mouseDelta = input != null ? input.Look.x : 0f;
                targetAngle = Mathf.Clamp(currentAngle + mouseDelta * mouseSensitivity * direction, 0f, maxOpenAngle);
            }
            else
            {
                float yawDelta = Mathf.DeltaAngle(holdStartYaw, player.PlayerCamera.transform.eulerAngles.y);
                targetAngle = Mathf.Clamp(holdStartAngle + yawDelta * direction, 0f, maxOpenAngle);
            }

            currentAngle = Mathf.Lerp(currentAngle, targetAngle, followSpeed * Time.deltaTime);
            ApplyAngle(currentAngle);
        }

        public void EndHold(PlayerInteractor player)
        {
            holding = false;

            if (logAngleOnRelease)
                Debug.Log($"{name}: {currentAngle:0} degrees open.", this);
        }

        public void PushFromBody(Vector3 contactPoint, Vector3 pushDirection, float moveDistance)
        {
            if (!bodyPushEnabled || holding || moveDistance <= 0f || pushDirection.sqrMagnitude <= 0.0001f)
                return;

            Vector3 worldAxis = GetWorldRotationAxis();
            Vector3 radialDirection = Vector3.ProjectOnPlane(contactPoint - Pivot.position, worldAxis);
            Vector3 planarPush = Vector3.ProjectOnPlane(pushDirection, worldAxis);

            if (radialDirection.sqrMagnitude <= 0.0001f || planarPush.sqrMagnitude <= 0.0001f)
                return;

            float torque = Vector3.Dot(
                Vector3.Cross(radialDirection.normalized, planarPush.normalized),
                worldAxis);

            if (Mathf.Abs(torque) < minimumBodyPushTorque)
                return;

            float configuredDirection = invertDirection ? -1f : 1f;
            float angleDelta = torque * configuredDirection * bodyPushDegreesPerMeter * Mathf.Min(moveDistance, 0.25f);
            float nextAngle = Mathf.Clamp(currentAngle + angleDelta, 0f, maxOpenAngle);

            if (Mathf.Approximately(nextAngle, currentAngle))
                return;

            currentAngle = nextAngle;
            ApplyAngle(currentAngle);
        }

        private void ApplyAngle(float angle)
        {
            float signedAngle = invertDirection ? -angle : angle;
            Pivot.localRotation = closedLocalRotation * Quaternion.Euler(GetAxisEuler(signedAngle));
        }

        private Vector3 GetAxisEuler(float angle)
        {
            return rotationAxis switch
            {
                RotationAxis.X => new Vector3(angle, 0f, 0f),
                RotationAxis.Y => new Vector3(0f, angle, 0f),
                _ => new Vector3(0f, 0f, angle)
            };
        }

        private Vector3 GetWorldRotationAxis()
        {
            Vector3 localAxis = rotationAxis switch
            {
                RotationAxis.X => Vector3.right,
                RotationAxis.Y => Vector3.up,
                _ => Vector3.forward
            };

            return Pivot.TransformDirection(localAxis).normalized;
        }

        private void OnValidate()
        {
            maxOpenAngle = Mathf.Clamp(maxOpenAngle, 0f, 180f);
            followSpeed = Mathf.Max(0.01f, followSpeed);
            mouseSensitivity = Mathf.Max(0f, mouseSensitivity);
            bodyPushDegreesPerMeter = Mathf.Max(0f, bodyPushDegreesPerMeter);
            minimumBodyPushTorque = Mathf.Clamp01(minimumBodyPushTorque);
        }
    }
}

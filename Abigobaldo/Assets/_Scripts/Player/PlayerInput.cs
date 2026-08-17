using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Abigobaldo.Game
{
    [DefaultExecutionOrder(-100)]
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private float throwHoldTime = 0.35f;

        public Vector2 Movement { get; private set; }
        public Vector2 Look { get; private set; }
        public float HoldZoom { get; private set; }

        public bool RunPressed { get; private set; }
        public bool PickPressed { get; private set; }
        public bool PickHeld { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool InteractHeld { get; private set; }
        public bool InteractReleased { get; private set; }
        public bool DropPressed { get; private set; }
        public bool ThrowPressed { get; private set; }
        public bool RotateHeld { get; private set; }
        public bool ToggleCursorPressed { get; private set; }

        private float dropThrowHoldTimer;
        private bool throwTriggered;
        private bool paused;

        private void Update()
        {
            HandlePause();

            if (paused)
            {
                ClearInput();
                return;
            }

            ReadMovement();
            ReadLook();
            ReadZoom();
            ReadActions();
        }

        private void LateUpdate()
        {
            PickPressed = false;
            InteractPressed = false;
            InteractReleased = false;
            DropPressed = false;
            ThrowPressed = false;
            ToggleCursorPressed = false;
        }

        private void ReadMovement()
        {
            if (Keyboard.current == null)
            {
                Movement = Vector2.zero;
                return;
            }

            Vector2 movement = Vector2.zero;

            if (Keyboard.current.aKey.isPressed)
                movement.x -= 1f;

            if (Keyboard.current.dKey.isPressed)
                movement.x += 1f;

            if (Keyboard.current.sKey.isPressed)
                movement.y -= 1f;

            if (Keyboard.current.wKey.isPressed)
                movement.y += 1f;

            Movement = movement.sqrMagnitude > 1f ? movement.normalized : movement;
        }

        private void ReadLook()
        {
            Look = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        }

        private void ReadZoom()
        {
            HoldZoom = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        }

        private void ReadActions()
        {
            if (Keyboard.current == null)
                return;

            RunPressed = Keyboard.current.leftShiftKey.isPressed;
            PickPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            PickHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
            InteractPressed = Keyboard.current.eKey.wasPressedThisFrame;
            InteractHeld = Keyboard.current.eKey.isPressed;
            InteractReleased = Keyboard.current.eKey.wasReleasedThisFrame;
            RotateHeld = Keyboard.current.rKey.isPressed;
            ToggleCursorPressed = Keyboard.current.vKey.wasPressedThisFrame;
            ReadDropThrow();
        }

        private void ReadDropThrow()
        {
            DropPressed = false;
            ThrowPressed = false;

            KeyControl key = Keyboard.current.gKey;

            if (key.wasPressedThisFrame)
            {
                dropThrowHoldTimer = 0f;
                throwTriggered = false;
            }

            if (key.isPressed)
            {
                dropThrowHoldTimer += Time.unscaledDeltaTime;

                if (!throwTriggered && dropThrowHoldTimer >= throwHoldTime)
                {
                    ThrowPressed = true;
                    throwTriggered = true;
                }
            }

            if (key.wasReleasedThisFrame)
            {
                if (!throwTriggered)
                    DropPressed = true;

                dropThrowHoldTimer = 0f;
                throwTriggered = false;
            }
        }

        private void HandlePause()
        {
            if (Keyboard.current == null || !Keyboard.current.pKey.wasPressedThisFrame)
                return;

            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ClearInput()
        {
            Movement = Vector2.zero;
            Look = Vector2.zero;
            HoldZoom = 0f;
            RunPressed = false;
            PickPressed = false;
            PickHeld = false;
            InteractPressed = false;
            InteractHeld = false;
            InteractReleased = false;
            DropPressed = false;
            ThrowPressed = false;
            RotateHeld = false;
            ToggleCursorPressed = false;
        }

        private void OnValidate()
        {
            throwHoldTime = Mathf.Max(0.05f, throwHoldTime);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[DefaultExecutionOrder(-100)]
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private float throwHoldTime = 0.35f;

    public Vector2 Movement { get; private set; }
    public Vector2 Look { get; private set; }
    public float HoldZoom { get; private set; }

    public bool RunPressed { get; private set; }
    public bool PickPressed { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool InteractReleased { get; private set; }
    public bool DropPressed { get; private set; }
    public bool ThrowPressed { get; private set; }
    public bool ToggleCursorPressed { get; private set; }
    public bool RotateHeld { get; private set; }

    private bool isPaused;
    private float dropThrowHoldTimer;
    private bool throwTriggered;

    private void Update()
    {
        HandlePause();

        if (isPaused)
        {
            ClearInput();
            return;
        }

        ReadMovement();
        ReadLook();
        ReadZoom();
        ReadActions();
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

        Movement = movement.sqrMagnitude > 1f
            ? movement.normalized
            : movement;
    }

    private void ReadLook()
    {
        Look = Mouse.current != null
            ? Mouse.current.delta.ReadValue()
            : Vector2.zero;
    }

    private void ReadZoom()
    {
        HoldZoom = Mouse.current != null
            ? Mouse.current.scroll.ReadValue().y
            : 0f;
    }

    private void ReadActions()
    {
        if (Keyboard.current == null)
            return;

        RunPressed = Keyboard.current.leftShiftKey.isPressed;
        PickPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        InteractHeld = Keyboard.current.eKey.isPressed;
        InteractPressed = Keyboard.current.eKey.wasPressedThisFrame;
        InteractReleased = Keyboard.current.eKey.wasReleasedThisFrame;
        ReadDropThrow();
        ToggleCursorPressed = Keyboard.current.vKey.wasPressedThisFrame;
        RotateHeld = Keyboard.current.rKey.isPressed;
    }

    private void ReadDropThrow()
    {
        DropPressed = false;
        ThrowPressed = false;

        KeyControl dropThrowKey = Keyboard.current.gKey;

        if (dropThrowKey.wasPressedThisFrame)
        {
            dropThrowHoldTimer = 0f;
            throwTriggered = false;
        }

        if (dropThrowKey.isPressed)
        {
            dropThrowHoldTimer += Time.unscaledDeltaTime;

            if (!throwTriggered && dropThrowHoldTimer >= throwHoldTime)
            {
                ThrowPressed = true;
                throwTriggered = true;
            }
        }

        if (dropThrowKey.wasReleasedThisFrame)
        {
            if (!throwTriggered)
                DropPressed = true;

            dropThrowHoldTimer = 0f;
            throwTriggered = false;
        }
    }

    private void LateUpdate()
    {
        InteractPressed = false;
        InteractReleased = false;
        PickPressed = false;
        DropPressed = false;
        ThrowPressed = false;
        ToggleCursorPressed = false;
    }

    private void HandlePause()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.pKey.wasPressedThisFrame)
            return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        Cursor.lockState = isPaused
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = isPaused;
    }

    private void ClearInput()
    {
        Movement = Vector2.zero;
        Look = Vector2.zero;

        RunPressed = false;
        PickPressed = false;
        InteractHeld = false;
        InteractPressed = false;
        InteractReleased = false;
        DropPressed = false;
        ThrowPressed = false;
        ToggleCursorPressed = false;
        RotateHeld = false;
        HoldZoom = 0f;
    }

    private void OnValidate()
    {
        throwHoldTime = Mathf.Max(0.05f, throwHoldTime);
    }
}



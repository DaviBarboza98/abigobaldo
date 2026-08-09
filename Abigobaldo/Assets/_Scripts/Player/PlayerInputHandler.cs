using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Movement { get; private set; }
    public Vector2 Look { get; private set; }
    public float HoldZoom { get; private set; }

    public bool RunPressed { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool InteractReleased { get; private set; }
    public bool DropPressed { get; private set; }
    public bool ThrowPressed { get; private set; }
    public bool ToggleCursorPressed { get; private set; }
    public bool RotateHeld { get; private set; }

    private bool isPaused;

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
        InteractHeld = Keyboard.current.eKey.isPressed;
        InteractPressed = Keyboard.current.eKey.wasPressedThisFrame;
        InteractReleased = Keyboard.current.eKey.wasReleasedThisFrame;
        DropPressed = Keyboard.current.gKey.wasPressedThisFrame;
        ThrowPressed = Keyboard.current.tKey.wasPressedThisFrame;
        ToggleCursorPressed = Keyboard.current.vKey.wasPressedThisFrame;
        RotateHeld = Keyboard.current.rKey.isPressed;
    }

    private void LateUpdate()
    {
        InteractPressed = false;
        InteractReleased = false;
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
        InteractHeld = false;
        InteractPressed = false;
        InteractReleased = false;
        DropPressed = false;
        ThrowPressed = false;
        ToggleCursorPressed = false;
        RotateHeld = false;
        HoldZoom = 0f;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Movement { get; private set; }
    public Vector2 Look { get; private set; }

    public bool RunPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool DropPressed { get; private set; }
    public bool ThrowPressed { get; private set; }
    public bool ToggleCursorPressed { get; private set; }

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
        ReadActions();
    }

    private void ReadMovement()
    {
        Vector2 movement = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed)
            movement.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            movement.x += 1f;

        if (Keyboard.current.sKey.isPressed)
            movement.y -= 1f;

        if (Keyboard.current.wKey.isPressed)
            movement.y += 1f;

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        Movement = movement;
    }

    private void ReadLook()
    {
        if (Mouse.current == null)
        {
            Look = Vector2.zero;
            return;
        }

        Look = Mouse.current.delta.ReadValue();
    }

    private void ReadActions()
    {
        if (Keyboard.current == null)
            return;

        RunPressed =
            Keyboard.current.leftShiftKey.isPressed;

        InteractPressed =
            Keyboard.current.eKey.wasPressedThisFrame;

        DropPressed =
            Keyboard.current.gKey.wasPressedThisFrame;

        ThrowPressed =
            Keyboard.current.tKey.wasPressedThisFrame;

        ToggleCursorPressed =
            Keyboard.current.vKey.wasPressedThisFrame;
    }

    private void LateUpdate()
    {
        InteractPressed = false;
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

        if (isPaused)
        {
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ClearInput()
    {
        Movement = Vector2.zero;
        Look = Vector2.zero;

        RunPressed = false;
        InteractPressed = false;
        DropPressed = false;
        ThrowPressed = false;
        ToggleCursorPressed = false;
    }
}
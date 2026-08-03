using UnityEngine;

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
            Movement = Vector2.zero;
            Look = Vector2.zero;
            RunPressed = false;
            InteractPressed = false;
            DropPressed = false;
            ThrowPressed = false;
            ToggleCursorPressed = false;
            return;
        }

        Movement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (Movement.sqrMagnitude > 1f)
            Movement.Normalize();

        Look = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        RunPressed = Input.GetKey(KeyCode.LeftShift);
        InteractPressed = Input.GetKeyDown(KeyCode.E);
        DropPressed = Input.GetKeyDown(KeyCode.G);
        ThrowPressed = Input.GetKeyDown(KeyCode.T);
        ToggleCursorPressed = Input.GetKeyDown(KeyCode.V);
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
        if (!Input.GetKeyDown(KeyCode.P))
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
}
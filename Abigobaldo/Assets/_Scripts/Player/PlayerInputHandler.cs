using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    // ==========================================
    // INPUT
    // ==========================================

    public Vector2 Movement { get; private set; }
    public Vector2 Look { get; private set; }

    public bool ToggleCursorPressed { get; private set; }

    // ==========================================
    // STATE
    // ==========================================

    private bool isPaused;

    private void Update()
    {
        HandlePause();

        if (isPaused)
            return;

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

        ToggleCursorPressed = Input.GetKeyDown(KeyCode.V);
    }

    private void LateUpdate()
    {
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
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Movement { get; private set; }
    public Vector2 Look { get; private set; }

    public bool ToggleCursorPressed { get; private set; }

    private void Update()
    {
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
}
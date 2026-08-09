using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerCursor : MonoBehaviour
{
    private PlayerInputHandler input;
    private bool cursorUnlocked;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
    }

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        HandleCursorToggle();
    }

    private void HandleCursorToggle()
    {
        if (!input.ToggleCursorPressed)
            return;

        cursorUnlocked = !cursorUnlocked;

        if (cursorUnlocked)
            UnlockCursor();
        else
            LockCursor();
    }

    public void LockCursor()
    {
        cursorUnlocked = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        cursorUnlocked = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

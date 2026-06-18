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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
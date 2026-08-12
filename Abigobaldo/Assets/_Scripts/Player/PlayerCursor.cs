using UnityEngine;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerCursor : MonoBehaviour
    {
        private PlayerInput input;
        private bool unlocked;

        private void Awake()
        {
            input = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            LockCursor();
        }

        private void Update()
        {
            if (!input.ToggleCursorPressed)
                return;

            if (unlocked)
                LockCursor();
            else
                UnlockCursor();
        }

        public void LockCursor()
        {
            unlocked = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            unlocked = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

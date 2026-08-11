using UnityEngine;

namespace Abigobaldo.Demo
{
    [RequireComponent(typeof(DemoPlayerInput))]
    public class DemoPlayerCursor : MonoBehaviour
    {
        private DemoPlayerInput input;
        private bool unlocked;

        private void Awake()
        {
            input = GetComponent<DemoPlayerInput>();
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

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerCursor : MonoBehaviour
    {
        [Header("Crosshair")]
        [SerializeField] private bool showCrosshair = true;
        [SerializeField] private Sprite crosshairSprite;
        [SerializeField, Min(1f)] private float crosshairSize = 8f;
        [SerializeField] private Color crosshairColor = Color.white;
        [SerializeField] private int crosshairSortingOrder = 1000;

        private PlayerInput input;
        private Canvas crosshairCanvas;
        private Image crosshairImage;

        public Vector2 AimScreenPosition => Cursor.lockState == CursorLockMode.Locked
            ? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            : GetPointerScreenPosition();

        private void Awake()
        {
            input = GetComponent<PlayerInput>();
            CreateCrosshair();
        }

        private void Start()
        {
            LockCursor();
        }

        private void Update()
        {
            UpdateCrosshairPosition();

            if (input.ToggleCursorPressed)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                    UnlockCursor();
                else
                    LockCursor();
            }
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

        private void CreateCrosshair()
        {
            GameObject canvasObject = new GameObject("Crosshair Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            crosshairCanvas = canvasObject.GetComponent<Canvas>();
            crosshairCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            crosshairCanvas.pixelPerfect = true;
            crosshairCanvas.sortingOrder = crosshairSortingOrder;

            GameObject imageObject = new GameObject("Crosshair", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(canvasObject.transform, false);
            crosshairImage = imageObject.GetComponent<Image>();
            crosshairImage.raycastTarget = false;
            crosshairImage.preserveAspect = true;

            ApplyCrosshairSettings();
            UpdateCrosshairPosition();
        }

        private void UpdateCrosshairPosition()
        {
            if (crosshairImage == null || crosshairCanvas == null)
                return;

            RectTransform crosshairRect = crosshairImage.rectTransform;

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                crosshairRect.anchoredPosition = Vector2.zero;
                return;
            }

            RectTransform canvasRect = (RectTransform)crosshairCanvas.transform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    AimScreenPosition,
                    null,
                    out Vector2 localPoint))
            {
                crosshairRect.anchoredPosition = localPoint;
            }
        }

        private static Vector2 GetPointerScreenPosition()
        {
            return Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        private void ApplyCrosshairSettings()
        {
            if (crosshairImage == null)
                return;

            crosshairImage.sprite = crosshairSprite;
            crosshairImage.enabled = showCrosshair && crosshairSprite != null;
            crosshairImage.color = crosshairColor;
            crosshairImage.rectTransform.sizeDelta = Vector2.one * Mathf.Max(1f, crosshairSize);

            if (crosshairCanvas != null)
                crosshairCanvas.sortingOrder = crosshairSortingOrder;
        }

        private void OnValidate()
        {
            crosshairSize = Mathf.Max(1f, crosshairSize);

            if (Application.isPlaying)
                ApplyCrosshairSettings();
        }
    }
}

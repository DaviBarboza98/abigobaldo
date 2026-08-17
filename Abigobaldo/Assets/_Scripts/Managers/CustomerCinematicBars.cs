using UnityEngine;
using UnityEngine.UI;

namespace Abigobaldo.Game
{
    /// <summary>Creates the only temporary UI used by the dialogue demo: two black cinematic bars.</summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class CustomerCinematicBars : MonoBehaviour
    {
        [SerializeField] private GameObject topBar;
        [SerializeField] private GameObject bottomBar;
        [SerializeField, Min(0f)] private float barHeight = 115f;
        [SerializeField] private Sprite barSprite;
        [Tooltip("Only for arranging the bars while editing the scene. They still start hidden in Play Mode.")]
        [SerializeField] private bool previewInEditor;

        public GameObject TopBar => topBar;
        public GameObject BottomBar => bottomBar;

        private void OnEnable()
        {
            if (!Application.isPlaying)
                EnsureCreated();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                EnsureCreated();
        }

        public void EnsureCreated()
        {
#if UNITY_EDITOR
            if (barSprite == null)
                barSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/preto.jpeg");
#endif
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            // Display 1 is the normal Game View. Display 2 is a separate monitor
            // output and would make the bars appear to be missing during testing.
            canvas.targetDisplay = 0;

            RectTransform canvasRect = transform as RectTransform;
            if (canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.anchoredPosition = Vector2.zero;
                canvasRect.sizeDelta = Vector2.zero;
                canvasRect.pivot = new Vector2(0.5f, 0.5f);
                canvasRect.localScale = Vector3.one;
            }

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            topBar = EnsureBar(topBar, "CinematicBar_Top", true);
            bottomBar = EnsureBar(bottomBar, "CinematicBar_Bottom", false);
            if (Application.isPlaying) Hide();
            else SetEditorPreview(previewInEditor);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (topBar == null || bottomBar == null) EnsureCreated();
            topBar.SetActive(true);
            bottomBar.SetActive(true);
            SetBarAlpha(topBar, progress);
            SetBarAlpha(bottomBar, progress);
        }

        public void Hide()
        {
            if (topBar != null) topBar.SetActive(false);
            if (bottomBar != null) bottomBar.SetActive(false);
        }

        private void SetEditorPreview(bool visible)
        {
            if (topBar == null || bottomBar == null) return;
            topBar.SetActive(visible);
            bottomBar.SetActive(visible);
            if (!visible) return;
            SetBarAlpha(topBar, 1f);
            SetBarAlpha(bottomBar, 1f);
        }

        private GameObject EnsureBar(GameObject current, string barName, bool top)
        {
            if (current == null)
            {
                Transform existing = transform.Find(barName);
                current = existing == null ? new GameObject(barName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)) : existing.gameObject;
                current.transform.SetParent(transform, false);
            }

            RectTransform rect = current.GetComponent<RectTransform>();
            if (rect == null) rect = current.AddComponent<RectTransform>();
            Image image = current.GetComponent<Image>();
            if (image == null) image = current.AddComponent<Image>();

            image.color = Color.black;
            image.sprite = barSprite;
            image.raycastTarget = false;
            rect.anchorMin = top ? new Vector2(0f, 1f) : Vector2.zero;
            rect.anchorMax = top ? Vector2.one : new Vector2(1f, 0f);
            rect.pivot = top ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, barHeight);
            return current;
        }

        private static void SetBarAlpha(GameObject bar, float alpha)
        {
            Image image = bar.GetComponent<Image>();
            if (image == null) return;
            Color color = image.color;
            color.a = Mathf.Clamp01(alpha);
            image.color = color;
        }
    }
}

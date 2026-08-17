using UnityEngine;
using UnityEngine.UI;

namespace Abigobaldo.Game
{
    /// <summary>Small world-space cooking meter displayed above its cooking container.</summary>
    [DisallowMultipleComponent]
    public sealed class CookingProgressBar : MonoBehaviour
    {
        [Header("Position")]
        [Tooltip("Create an Empty child in the container prefab, position it above the container, then assign it here.")]
        [SerializeField] private Transform barPivot;
        [Tooltip("Only used while no Pivot is assigned.")]
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.35f, 0f);
        [SerializeField] private Vector2 size = new Vector2(150f, 16f);
        [SerializeField] private float worldScale = 0.01f;
        [SerializeField] private bool followPlayerCamera = true;

        private ContainerStation station;
        private RecipeProgress progress;
        private GameObject canvasObject;
        private RectTransform canvasRect;
        private RectTransform fillRect;
        private RectTransform firstMarker;
        private RectTransform secondMarker;
        private RawImage fill;

        private void Awake()
        {
            station = GetComponent<ContainerStation>();
            if (station == null)
            {
                // This meter is only valid on a container. It prevents old or
                // accidentally added components on food prefabs from showing up.
                Destroy(this);
            }
        }

        private void LateUpdate()
        {
            if (station == null) return;
            progress = station != null ? station.CurrentRecipeProgress : null;
            if (progress == null || progress.Recipe == null || !progress.Recipe.UsesHeat || progress.State == FoodState.Carbonized)
            {
                if (canvasObject != null) canvasObject.SetActive(false);
                return;
            }

            EnsureVisuals();
            canvasObject.SetActive(true);
            canvasRect.localPosition = barPivot == null ? localOffset : Vector3.zero;
            canvasRect.sizeDelta = size;
            SetConsistentWorldScale();
            UpdateMeter();
            FacePlayerCamera();
        }

        private void EnsureVisuals()
        {
            if (canvasObject != null) return;

            canvasObject = new GameObject("CookingProgressBar", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(barPivot != null ? barPivot : transform, false);
            canvasRect = (RectTransform)canvasObject.transform;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 50;

            CreateBox("Background", canvasRect, Color.black, out _);
            fillRect = CreateBox("Fill", canvasRect, Color.white, out fill);
            firstMarker = CreateMarker("AlmostReadyMarker", canvasRect);
            secondMarker = CreateMarker("BurnedMarker", canvasRect);
        }

        private static RectTransform CreateBox(string objectName, RectTransform parent, Color color, out RawImage image)
        {
            GameObject child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            RectTransform rect = (RectTransform)child.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            image = child.GetComponent<RawImage>();
            image.texture = Texture2D.whiteTexture;
            image.color = color;
            return rect;
        }

        private static RectTransform CreateMarker(string objectName, RectTransform parent)
        {
            GameObject child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            RectTransform rect = (RectTransform)child.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(3f, 0f);
            RawImage image = child.GetComponent<RawImage>();
            image.texture = Texture2D.whiteTexture;
            image.color = Color.black;
            return rect;
        }

        private void UpdateMeter()
        {
            RecipeData recipe = progress.Recipe;
            float elapsed = progress.ElapsedTime;
            bool firstStage = elapsed < recipe.ProcessingTime;

            if (firstStage)
            {
                SetFill(Mathf.Clamp01(elapsed / Mathf.Max(0.01f, recipe.ProcessingTime)), Color.white);
                SetMarker(firstMarker, recipe.AlmostReadyTime / Mathf.Max(0.01f, recipe.ProcessingTime), true);
                secondMarker.gameObject.SetActive(false);
                return;
            }

            float dangerDuration = Mathf.Max(0.01f, recipe.CarbonizedTime - recipe.ProcessingTime);
            SetFill(Mathf.Clamp01((elapsed - recipe.ProcessingTime) / dangerDuration), new Color(1f, 0.75f, 0.2f));
            SetMarker(firstMarker, (recipe.OverdoneTime - recipe.ProcessingTime) / dangerDuration, true);
            SetMarker(secondMarker, (recipe.BurnedTime - recipe.ProcessingTime) / dangerDuration, true);
        }

        private void SetFill(float amount, Color color)
        {
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(amount, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            fill.color = color;
        }

        private static void SetMarker(RectTransform marker, float location, bool visible)
        {
            marker.gameObject.SetActive(visible);
            float point = Mathf.Clamp01(location);
            marker.anchorMin = new Vector2(point, 0f);
            marker.anchorMax = new Vector2(point, 1f);
            marker.anchoredPosition = Vector2.zero;
        }

        private void FacePlayerCamera()
        {
            if (!followPlayerCamera) return;
            Camera camera = Camera.main;
            if (camera == null || !camera.enabled) camera = FindObjectOfType<Camera>();
            if (camera == null) return;

            // Keep the bar upright: it only rotates on Y (the vertical axis),
            // never tilting up/down with the camera's pitch or roll.
            Vector3 direction = camera.transform.position - canvasRect.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
                canvasRect.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void SetConsistentWorldScale()
        {
            Vector3 parentScale = canvasRect.parent != null ? canvasRect.parent.lossyScale : Vector3.one;
            canvasRect.localScale = new Vector3(
                worldScale / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
                worldScale / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)),
                worldScale / Mathf.Max(0.0001f, Mathf.Abs(parentScale.z)));
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
        [Header("Dialogue texts — position these children freely")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];
        private Coroutine typeRoutine;
        public bool OptionsVisible { get; private set; }

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
            EnsureDialogueTexts();
            if (Application.isPlaying) { Hide(); HideDialogueTexts(); }
            else SetEditorPreview(previewInEditor);
        }

        public void ShowDialogue(string characterName, string line, string[] options)
        {
            EnsureDialogueTexts();
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            OptionsVisible = false;
            typeRoutine = StartCoroutine(TypeDialogue(characterName, line, options));
        }

        private IEnumerator TypeDialogue(string characterName, string line, string[] options)
        {
            characterNameText.text = characterName;
            characterNameText.gameObject.SetActive(true);
            dialogueText.gameObject.SetActive(true);
            for (int i = 0; i < optionTexts.Length; i++)
                optionTexts[i].gameObject.SetActive(false);
            dialogueText.text = string.Empty;
            foreach (char character in line)
            {
                dialogueText.text += character;
                yield return new WaitForSecondsRealtime(0.025f);
            }
            yield return new WaitForSecondsRealtime(0.5f);
            for (int i = 0; i < optionTexts.Length; i++)
            {
                bool visible = options != null && i < options.Length;
                optionTexts[i].gameObject.SetActive(visible);
                if (visible) optionTexts[i].text = options[i];
            }
            OptionsVisible = true;
            typeRoutine = null;
        }

        public void HideDialogueTexts()
        {
            if (typeRoutine != null) { StopCoroutine(typeRoutine); typeRoutine = null; }
            OptionsVisible = false;
            if (characterNameText != null) characterNameText.gameObject.SetActive(false);
            if (dialogueText != null) dialogueText.gameObject.SetActive(false);
            foreach (TextMeshProUGUI option in optionTexts) if (option != null) option.gameObject.SetActive(false);
        }

        private void EnsureDialogueTexts()
        {
            characterNameText = EnsureText(characterNameText, "Character Name");
            dialogueText = EnsureText(dialogueText, "Dialogue Text");
            for (int i = 0; i < optionTexts.Length; i++) optionTexts[i] = EnsureText(optionTexts[i], "Dialogue Option " + (i + 1));
        }

        private TextMeshProUGUI EnsureText(TextMeshProUGUI current, string textName)
        {
            if (current != null) return current;
            Transform existing = transform.Find(textName);
            GameObject target = existing != null ? existing.gameObject : new GameObject(textName, typeof(RectTransform), typeof(TextMeshProUGUI));
            target.transform.SetParent(transform, false);
            TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
            text.fontSize = 32; text.color = Color.white; text.enableWordWrapping = true;
            target.SetActive(!Application.isPlaying);
            return text;
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

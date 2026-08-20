using TMPro;
using UnityEngine;

namespace Abigobaldo.Game
{
    public sealed class PauseMenu : MonoBehaviour
    {
        private static GameObject panel;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainGame" || panel != null) return;
            var root = new GameObject("Pause Menu", typeof(RectTransform), typeof(Canvas));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 300;
            panel = new GameObject("Panel", typeof(RectTransform), typeof(TextMeshProUGUI)); panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>(); rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var text = panel.GetComponent<TextMeshProUGUI>(); text.text="<b>PAUSADO</b>\n\nP para continuar"; text.alignment=TextAlignmentOptions.Center; text.fontSize=46; text.color=Color.white;
            panel.SetActive(false);
        }
        public static void SetVisible(bool visible) { if (panel != null) panel.SetActive(visible); }
    }
}

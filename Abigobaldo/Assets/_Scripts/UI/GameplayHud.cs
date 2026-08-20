using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Abigobaldo.Game
{
    [ExecuteAlways]
    public sealed class GameplayHud : MonoBehaviour
    {
        private static TextMeshProUGUI interactPrompt;
        private static TextMeshProUGUI pickPrompt;
        private static TextMeshProUGUI orderText;
        private static TextMeshProUGUI dialogue;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainGame" || interactPrompt != null) return;
            var root = GameObject.Find("UICanvas") ?? new GameObject("HUD Canvas", typeof(RectTransform));
            if (root.GetComponent<GameplayHud>() == null) root.AddComponent<GameplayHud>();
        }

        private void Awake() => EnsureCreated();
        private void OnEnable() => EnsureCreated();
        private void EnsureCreated()
        {
            if (interactPrompt != null) return;
            var root = gameObject;
            if (!(transform is RectTransform))
            {
                Transform existing = transform.Find("HUD Canvas");
                root = existing != null ? existing.gameObject : new GameObject("HUD Canvas", typeof(RectTransform));
                root.transform.SetParent(transform, false);
            }
            var canvas = root.GetComponent<Canvas>();
            if (canvas == null) canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200;
            var scaler = root.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080);
            interactPrompt = Text(root.transform, "Interact Prompt", new Vector2(330,180), new Vector2(1260,70), TextAlignmentOptions.Center);
            pickPrompt = Text(root.transform, "Pick Prompt", new Vector2(330,115), new Vector2(1260,70), TextAlignmentOptions.Center);
            orderText = Text(root.transform, "Order Text", new Vector2(20,900), new Vector2(600,80), TextAlignmentOptions.TopLeft);
            interactPrompt.gameObject.SetActive(false); pickPrompt.gameObject.SetActive(false);
            orderText.gameObject.SetActive(false);
            dialogue = Text(root.transform, "Dialogo", new Vector2(330,90), new Vector2(1260,260), TextAlignmentOptions.Center); dialogue.gameObject.SetActive(false);
        }
        private static TextMeshProUGUI Text(Transform parent,string name,Vector2 min,Vector2 size,TextAlignmentOptions align)
        { var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); go.transform.SetParent(parent,false); var r=go.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=min/1920f; r.sizeDelta=size; var t=go.GetComponent<TextMeshProUGUI>(); t.fontSize=28; t.alignment=align; t.color=Color.white; t.enableWordWrapping=true; return t; }
        public static void ShowDialogue(string speaker,string line,string options)
        { if(dialogue==null)return; dialogue.text="<b>"+speaker+"</b>\n"+line+"\n\n"+options; dialogue.gameObject.SetActive(true); }
        public static void HideDialogue(){if(dialogue!=null)dialogue.gameObject.SetActive(false);}
        public static void SetPrompts(string interact, string pick)
        {
            if (interactPrompt != null) { interactPrompt.text = interact; interactPrompt.gameObject.SetActive(!string.IsNullOrEmpty(interact)); }
            if (pickPrompt != null) { pickPrompt.text = pick; pickPrompt.gameObject.SetActive(!string.IsNullOrEmpty(pick)); }
        }
        public static void SetOrder(string food)
        {
            if (orderText == null) return;
            orderText.text = "Pedido: " + food;
            orderText.gameObject.SetActive(!string.IsNullOrWhiteSpace(food));
        }
    }
}

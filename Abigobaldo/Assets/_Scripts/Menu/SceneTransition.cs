using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Mantem uma tela preta entre cenas para evitar um corte brusco na troca.
/// Pode ser chamada de qualquer botao pelo metodo estatico Carregar.
/// </summary>
public sealed class SceneTransition : MonoBehaviour
{
    private const float DuracaoPadrao = 0.35f;

    private static SceneTransition instancia;

    [SerializeField, Min(0.01f)] private float duracaoFade = DuracaoPadrao;

    private CanvasGroup telaDeFade;
    private bool emTransicao;

    public static void Carregar(string nomeDaCena)
    {
        if (string.IsNullOrWhiteSpace(nomeDaCena))
        {
            Debug.LogWarning("Tentativa de carregar uma cena sem nome.");
            return;
        }

        if (instancia == null)
        {
            var objetoDeTransicao = new GameObject("Scene Transition");
            instancia = objetoDeTransicao.AddComponent<SceneTransition>();
        }

        if (!instancia.emTransicao)
        {
            instancia.StartCoroutine(instancia.CarregarComFade(nomeDaCena));
        }
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);
        CriarTelaDeFade();
    }

    private void CriarTelaDeFade()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        var tela = new GameObject("Fade Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        tela.transform.SetParent(transform, false);

        var rectTransform = tela.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var imagem = tela.GetComponent<Image>();
        imagem.color = Color.black;
        imagem.raycastTarget = true;

        telaDeFade = tela.GetComponent<CanvasGroup>();
        telaDeFade.alpha = 0f;
        telaDeFade.blocksRaycasts = false;
    }

    private IEnumerator CarregarComFade(string nomeDaCena)
    {
        emTransicao = true;
        telaDeFade.blocksRaycasts = true;

        yield return AnimarAlpha(1f);

        AsyncOperation carregamento = SceneManager.LoadSceneAsync(nomeDaCena);
        while (!carregamento.isDone)
        {
            yield return null;
        }

        // RenderSettings is global. Reapply MainGame lighting after the old menu
        // scene has been released, otherwise its ambient/sky state can leak for
        // one frame (and leave white materials looking black or yellow).
        yield return null;
        UnityEngine.Object.FindObjectOfType<Abigobaldo.Game.LightingManager>()?.ApplyLighting();
        Abigobaldo.Game.GameSoundManager.EnsureForMainGame();

        yield return AnimarAlpha(0f);

        telaDeFade.blocksRaycasts = false;
        emTransicao = false;
    }

    private IEnumerator AnimarAlpha(float alphaFinal)
    {
        float alphaInicial = telaDeFade.alpha;
        float tempo = 0f;

        while (tempo < duracaoFade)
        {
            tempo += Time.unscaledDeltaTime;
            telaDeFade.alpha = Mathf.Lerp(alphaInicial, alphaFinal, tempo / duracaoFade);
            yield return null;
        }

        telaDeFade.alpha = alphaFinal;
    }
}

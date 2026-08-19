using UnityEngine;
using System.Collections;

public class PainelAnimacao : MonoBehaviour
{
    public float duracao = 0.25f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Abrir()
    {
        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(AnimarEntrada());
    }

    public void Fechar()
    {
        StopAllCoroutines();
        StartCoroutine(AnimarSaida());
    }

    IEnumerator AnimarEntrada()
    {
        float tempo = 0;

        rectTransform.localScale = Vector3.one * 0.85f;
        canvasGroup.alpha = 0;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = tempo / duracao;

            // Suaviza a animação
            progresso = 1 - Mathf.Pow(1 - progresso, 3);

            rectTransform.localScale =
                Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, progresso);

            canvasGroup.alpha = progresso;

            yield return null;
        }

        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1;
    }

    IEnumerator AnimarSaida()
    {
        float tempo = 0;

        while (tempo < duracao)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = tempo / duracao;
            progresso = 1 - Mathf.Pow(1 - progresso, 3);

            rectTransform.localScale =
                Vector3.Lerp(Vector3.one, Vector3.one * 0.85f, progresso);

            canvasGroup.alpha = 1 - progresso;

            yield return null;
        }

        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}
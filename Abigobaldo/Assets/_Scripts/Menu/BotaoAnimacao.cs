using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class BotaoAnimacao : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public float escalaHover = 1.1f;
    public float escalaClique = 0.9f;
    public float velocidade = 10f;

    private Vector3 escalaOriginal;
    private Vector3 escalaAlvo;

    private bool mouseEmCima = false;
    private bool clicando = false;

    void Start()
    {
        escalaOriginal = transform.localScale;
        escalaAlvo = escalaOriginal;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            escalaAlvo,
            velocidade * Time.deltaTime
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEmCima = true;

        if (!clicando)
        {
            escalaAlvo = escalaOriginal * escalaHover;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEmCima = false;

        if (!clicando)
        {
            escalaAlvo = escalaOriginal;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!clicando)
        {
            StartCoroutine(AnimacaoClique());
        }
    }

    IEnumerator AnimacaoClique()
    {
        clicando = true;

        // Diminui ao clicar
        escalaAlvo = escalaOriginal * escalaClique;

        yield return new WaitForSeconds(0.1f);

        // SEMPRE volta para o tamanho original
        escalaAlvo = escalaOriginal;

        yield return new WaitForSeconds(0.15f);

        clicando = false;

        // Depois que terminou, verifica se o mouse ainda está em cima
        // Se estiver, volta para o hover
        if (mouseEmCima)
        {
            escalaAlvo = escalaOriginal * escalaHover;
        }
        else
        {
            escalaAlvo = escalaOriginal;
        }
    }
}
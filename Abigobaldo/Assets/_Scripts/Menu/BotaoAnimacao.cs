using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class BotaoAnimacao : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public float escalaHover = 1.1f;
    public float escalaClique = 0.9f;
    public float velocidade = 10f;

    [Tooltip("Nome da cena que sera aberta depois da animacao. Deixe vazio para este botao nao trocar de cena.")]
    public string cenaParaCarregar;

    private Vector3 escalaOriginal;
    private Vector3 escalaAlvo;

    private bool mouseEmCima = false;
    private bool clicando = false;
    private Button botao;
    
    AudioManager audioManager;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
        escalaAlvo = escalaOriginal;

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

        // O Button e o canal confiavel de clique da UI. IPointerClick fica apenas
        // como apoio para objetos que nao usam o componente Button.
        if (!string.IsNullOrWhiteSpace(cenaParaCarregar) && TryGetComponent(out botao))
        {
            botao.onClick.AddListener(CarregarCena);
        }
    }

    private void Start()
    {
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
            audioManager.PlaySFX(audioManager.houverSound);
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
        audioManager.PlaySFX(audioManager.clickSound);

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

        // Um Button chama CarregarCena pelo onClick. Para elementos sem Button,
        // o clique por ponteiro continua funcionando normalmente.
        if (botao == null && !string.IsNullOrWhiteSpace(cenaParaCarregar))
        {
            CarregarCena();
        }
    }

    public void CarregarCena()
    {
        SceneManager.LoadScene(cenaParaCarregar);
    }
}

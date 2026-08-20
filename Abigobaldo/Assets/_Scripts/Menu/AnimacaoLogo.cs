using UnityEngine;
using UnityEngine.EventSystems;

public class AnimacaoLogo : MonoBehaviour, IPointerClickHandler
{
    public float altura = 10f;
    public float velocidade = 2f;

    private Vector3 posicaoInicial;

    public AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    void Start()
    {
        posicaoInicial = transform.localPosition;
    }

    void Update()
    {
        float movimento = Mathf.Sin(Time.time * velocidade) * altura;

        transform.localPosition = posicaoInicial + new Vector3(0, movimento, 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("LOGO CLICADA!");

        audioManager.PlaySFX(audioManager.cookSound);
    }
}
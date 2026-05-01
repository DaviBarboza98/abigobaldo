using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configurações de Escala")]
    [SerializeField] private float scalaNormal = 1f;
    [SerializeField] private float scalaHover = 1.2f;
    [SerializeField] private float velocidade = 10f;

    private Vector3 escalaAlvo;

    private void Start()
    {
        escalaAlvo = Vector3.one * scalaNormal;
    }

    private void Update()
    {
        // Faz a transição suave entre a escala atual e a alvo
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            escalaAlvo,
            Time.deltaTime * velocidade
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        escalaAlvo = Vector3.one * scalaHover; // Mouse entrou → cresce
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        escalaAlvo = Vector3.one * scalaNormal; // Mouse saiu → encolhe
    }
}
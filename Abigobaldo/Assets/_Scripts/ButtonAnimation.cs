using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonClickAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Configurações")]
    [SerializeField] private float scaleDown = 0.85f;
    [SerializeField] private float pressSpeed = 12f;       // Velocidade ao apertar
    [SerializeField] private float releaseSpeed = 8f;      // Velocidade ao soltar
    [SerializeField] private float overshoot = 1.08f;      // Quanto ultrapassa ao voltar (efeito elástico)

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine animationCoroutine;
    private bool isPressed = false;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = originalScale * scaleDown;
        StartAnim(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        StartAnim(true);
    }

    private void StartAnim(bool isReleasing)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        if (isReleasing)
            animationCoroutine = StartCoroutine(AnimateRelease());
        else
            animationCoroutine = StartCoroutine(AnimatePress());
    }

    // Animação de apertar (encolhe suavemente)
    private IEnumerator AnimatePress()
    {
        Vector3 pressTarget = originalScale * scaleDown;

        while (Vector3.Distance(transform.localScale, pressTarget) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                pressTarget,
                Time.deltaTime * pressSpeed
            );
            yield return null;
        }

        transform.localScale = pressTarget;
    }

    // Animação de soltar (volta com efeito elástico)
    private IEnumerator AnimateRelease()
    {
        // Fase 1 - Cresce além do tamanho original (overshoot)
        Vector3 overshootScale = originalScale * overshoot;

        while (Vector3.Distance(transform.localScale, overshootScale) > 0.008f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                overshootScale,
                Time.deltaTime * releaseSpeed
            );
            yield return null;
        }

        // Fase 2 - Volta ao tamanho original suavemente
        while (Vector3.Distance(transform.localScale, originalScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                Time.deltaTime * releaseSpeed
            );
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
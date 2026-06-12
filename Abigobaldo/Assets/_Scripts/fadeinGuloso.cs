using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeIn : MonoBehaviour
{
    [Header("=== CONFIGURAÇÕES ===")]
    public Image painelFade;

    [Range(0.1f, 3f)]
    public float velocidadeFade = 1f;

    void Start()
    {
        StartCoroutine(FazerFadeIn());
    }

    IEnumerator FazerFadeIn()
    {
        // Começa totalmente preto
        painelFade.color = new Color(0, 0, 0, 1);

        float timer = 1f;

        // Vai clareando até ficar transparente
        while (timer > 0f)
        {
            timer -= Time.deltaTime * velocidadeFade;
            painelFade.color = new Color(0, 0, 0, timer);
            yield return null;
        }

        // Garante que ficou transparente
        painelFade.color = new Color(0, 0, 0, 0);
    }
}
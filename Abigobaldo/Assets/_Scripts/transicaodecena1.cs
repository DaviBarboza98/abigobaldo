using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TrocaDeCena : MonoBehaviour
{
    [Header("=== CONFIGURAÇÕES ===")]
    public Image painelFade;
    public AudioSource audioSource;
    public AudioSource musicSource; // <--- ADICIONE ESTA LINHA
    public AudioClip somDeClique;

    [Range(0.1f, 3f)]
    public float velocidadeFade = 1f;

    [Header("=== MÚSICA ===")]
    public bool fadeMusic = true;
    [Range(1f, 10f)]
    public float fadeDuration = 2f;

    public void IrParaCena(string nomeDaCena)
    {
        StartCoroutine(TransicaoComFade(nomeDaCena));
    }

    IEnumerator TransicaoComFade(string nomeDaCena)
    {
        // 1 - Toca o som de clique
        if (audioSource != null && somDeClique != null)
        {
            audioSource.PlayOneShot(somDeClique);
        }

        // 2 - Fade out da música
        if (fadeMusic && musicSource != null)
        {
            yield return StartCoroutine(FadeMusicOut());
        }

        // 3 - Espera o som terminar
        yield return new WaitForSeconds(somDeClique.length);

        // 4 - Fade OUT (tela fica preta)
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * velocidadeFade;
            painelFade.color = new Color(0, 0, 0, timer);
            yield return null;
        }

        // Garante que ficou totalmente preto
        painelFade.color = new Color(0, 0, 0, 1);

        // 5 - Carrega a cena
        SceneManager.LoadScene(nomeDaCena);
    }

    IEnumerator FadeMusicOut()
    {
        float alpha = 1f;
        float timer = 0f;

        while (alpha > 0f && timer < fadeDuration)
        {
            timer += Time.deltaTime;
            alpha = 1f - (timer / fadeDuration);
            musicSource.volume = alpha;
            yield return null;
        }

        // Garante que ficou totalmente silencioso
        musicSource.volume = 0f;
    }
}
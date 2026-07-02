using TMPro;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public TMP_Text timerText;

    public float tempo = 30f;

    private bool iniciou = false;

    void Start()
    {
        AtualizarTexto();
    }

    void Update()
    {
        // Inicia no K
        if (Input.GetKeyDown(KeyCode.K))
        {
            iniciou = true;
        }

        if (!iniciou)
            return;

        if (tempo > 0)
        {
            tempo -= Time.deltaTime;

            if (tempo < 0)
                tempo = 0;

            AtualizarTexto();
        }
    }

    void AtualizarTexto()
    {
        int min = Mathf.FloorToInt(tempo / 60);
        int sec = Mathf.FloorToInt(tempo % 60);

        timerText.text = $"{min:00}:{sec:00}";
    }
}
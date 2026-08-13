using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimacaoLogo : MonoBehaviour
{
    // Start is called before the first frame update
    public float altura = 10f;
    public float velocidade = 2f;

    private Vector3 posicaoInicial;

    void Start()
    {
        posicaoInicial = transform.localPosition;
    }

    void Update()
    {
        float movimento = Mathf.Sin(Time.time * velocidade) * altura;

        transform.localPosition = posicaoInicial + new Vector3(0, movimento, 0);
    }
}

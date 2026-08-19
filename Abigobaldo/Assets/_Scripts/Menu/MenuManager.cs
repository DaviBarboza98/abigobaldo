using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public PainelAnimacao painelOpcoes;
    public PainelAnimacao painelCreditos;

    public void AbrirOpcoes()
    {
        painelCreditos.gameObject.SetActive(false);
        painelOpcoes.Abrir();
    }

    public void AbrirCreditos()
    {
        painelOpcoes.gameObject.SetActive(false);
        painelCreditos.Abrir();
    }

    public void FecharOpcoes()
    {
        painelOpcoes.Fechar();
    }

    public void FecharCreditos()
    {
        painelCreditos.Fechar();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para gerenciar cenas

public class MudarCena : MonoBehaviour
{
    // Método para ser chamado pelo botão
    public void CarregarCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena); // Carrega a cena pelo nome
    }
}

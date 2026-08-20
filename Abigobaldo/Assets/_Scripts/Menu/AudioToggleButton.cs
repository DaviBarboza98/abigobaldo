using UnityEngine;
using UnityEngine.UI;

public class AudioToggleButton : MonoBehaviour
{
    public Image imagemBotao;

    public Sprite spriteLigado;
    public Sprite spriteDesligado;

    public bool desligado = false;

    public enum TipoAudio
    {
        SFX,
        Music
    }

    public TipoAudio tipoAudio;

    public AudioManager audioManager;

    private void Start()
    {
        desligado = PlayerPrefs.GetInt(tipoAudio == TipoAudio.SFX ? AudioManager.SfxMutedKey : AudioManager.MusicMutedKey, 0) == 1;
        imagemBotao.sprite = desligado ? spriteDesligado : spriteLigado;
        if (tipoAudio == TipoAudio.SFX) audioManager.SetSFXMuted(desligado);
        else audioManager.SetMusicMuted(desligado);
    }

    public void AlternarAudio()
    {
        desligado = !desligado;

        if (desligado)
            imagemBotao.sprite = spriteDesligado;
        else
            imagemBotao.sprite = spriteLigado;

        if (tipoAudio == TipoAudio.SFX)
        {
            audioManager.SetSFXMuted(desligado);
        }
        else
        {
            audioManager.SetMusicMuted(desligado);
        }
    }
}

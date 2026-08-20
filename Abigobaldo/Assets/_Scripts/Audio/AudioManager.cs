using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public const string MusicMutedKey = "Audio.MusicMuted";
    public const string SfxMutedKey = "Audio.SfxMuted";
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip backgroundCookMusic;

    [SerializeField] AudioMixer audioMixer;

    public AudioClip backgroundMusic;
    public AudioClip houverSound;
    public AudioClip clickSound;
    public AudioClip cookSound;

    private void Start()
    {
        musicSource.clip = backgroundCookMusic;
        musicSource.Play();
        SetMusicMuted(PlayerPrefs.GetInt(MusicMutedKey, 0) == 1);
        SetSFXMuted(PlayerPrefs.GetInt(SfxMutedKey, 0) == 1);
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void SetMusicMuted(bool muted)
    {
        audioMixer.SetFloat("MusicVolume", muted ? -80f : 0f);
        PlayerPrefs.SetInt(MusicMutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFXMuted(bool muted)
    {
        audioMixer.SetFloat("SFXVolume", muted ? -80f : 0f);
        PlayerPrefs.SetInt(SfxMutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
    }
}

using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [SerializeField] AudioMixer audioMixer;

    public AudioClip backgroundMusic;
    public AudioClip houverSound;
    public AudioClip clickSound;
    public AudioClip cookSound;

    private void Start()
    {
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void SetMusicMuted(bool muted)
    {
        audioMixer.SetFloat("MusicVolume", muted ? -80f : 0f);
    }

    public void SetSFXMuted(bool muted)
    {
        audioMixer.SetFloat("SFXVolume", muted ? -80f : 0f);
    }
}
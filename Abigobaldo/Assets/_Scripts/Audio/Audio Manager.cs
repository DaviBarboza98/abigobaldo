using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource; 

    public AudioClip backgroundMusic;

    private void Start()
    {
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }
}

using System.Collections;
using UnityEngine;

namespace Abigobaldo.Game
{
    [DefaultExecutionOrder(-250)]
    public sealed class GameSoundManager : MonoBehaviour
    {
        public static GameSoundManager Instance { get; private set; }

        private AudioSource voiceSource;
        private AudioSource effectSource;
        private AudioSource fryingSource;
        private AudioSource blenderSource;
        private AudioClip abigobaldo;
        private AudioClip questionamento;
        private AudioClip decepcao;
        private AudioClip ulala;
        private AudioClip comidaGirando;
        private AudioClip sininho;
        private AudioClip[] risadas;
        private float nextInteractionVoice;
        private float nextSpinSound;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateForGame()
        {
            EnsureForMainGame();
        }

        public static void EnsureForMainGame()
        {
            if (Instance != null || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainGame") return;
            new GameObject("GameSoundManager").AddComponent<GameSoundManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            voiceSource = CreateSource("Voice");
            effectSource = CreateSource("Effects");
            fryingSource = CreateSource("Frying Loop");
            blenderSource = CreateSource("Blender Loop");
            fryingSource.loop = blenderSource.loop = true;
            LoadClips();
            bool muted = PlayerPrefs.GetInt(AudioManager.SfxMutedKey, 0) == 1;
            voiceSource.mute = effectSource.mute = fryingSource.mute = blenderSource.mute = muted;
            StartCoroutine(PlayRandomLaughs());
        }

        private AudioSource CreateSource(string sourceName)
        {
            var child = new GameObject(sourceName);
            child.transform.SetParent(transform);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        private void LoadClips()
        {
            abigobaldo = Load("abigobaldo"); questionamento = Load("questionamento");
            decepcao = Load("decepção"); ulala = Load("ulala"); comidaGirando = Load("comidaGirando");
            sininho = Load("sininho de balcao");
            risadas = new[] { Load("risada"), Load("risda2"), Load("risada3") };
            fryingSource.clip = Load("fritando"); blenderSource.clip = Load("liquidificador");
        }

        private static AudioClip Load(string name) => Resources.Load<AudioClip>("Audio/Gameplay/" + name);
        private IEnumerator PlayRandomLaughs()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(80f, 150f));
                AudioClip clip = risadas[Random.Range(0, risadas.Length)];
                if (clip != null) voiceSource.PlayOneShot(clip, 0.55f);
            }
        }

        public static void PlayBell() => Instance?.PlayEffect(Instance.sininho, 0.8f);
        public static void PlayPerfect() => Instance?.PlayVoice(Instance.ulala, 0.9f);
        public static void PlayDisappointment() => Instance?.PlayVoice(Instance.decepcao, 0.85f);
        public static void PlayQuestion() { if (Instance != null && Random.value < 0.35f) Instance.PlayVoice(Instance.questionamento, 0.55f); }
        public static void PlayInteractionVoice()
        {
            if (Instance == null || Time.unscaledTime < Instance.nextInteractionVoice) return;
            Instance.nextInteractionVoice = Time.unscaledTime + 8f;
            Instance.PlayVoice(Instance.abigobaldo, 0.6f);
        }
        public static void PlayOmeletSpin()
        {
            if (Instance == null || Time.unscaledTime < Instance.nextSpinSound) return;
            Instance.nextSpinSound = Time.unscaledTime + 4f;
            Instance.PlayEffect(Instance.comidaGirando, 0.8f);
        }
        public static void SetFrying(bool active, float volume)
        {
            if (Instance == null || Instance.fryingSource.clip == null) return;
            Instance.fryingSource.volume = Mathf.Clamp01(volume);
            if (active && !Instance.fryingSource.isPlaying) Instance.fryingSource.Play();
            if (!active && Instance.fryingSource.isPlaying) Instance.fryingSource.Stop();
        }
        public static void SetBlender(bool active)
        {
            if (Instance == null || Instance.blenderSource.clip == null) return;
            if (active && !Instance.blenderSource.isPlaying) Instance.blenderSource.Play();
            if (!active && Instance.blenderSource.isPlaying) Instance.blenderSource.Stop();
        }
        private void PlayVoice(AudioClip clip, float volume) { if (clip != null) voiceSource.PlayOneShot(clip, volume); }
        private void PlayEffect(AudioClip clip, float volume) { if (clip != null) effectSource.PlayOneShot(clip, volume); }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}

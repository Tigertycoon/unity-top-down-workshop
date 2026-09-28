using System.Collections.Generic;
using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Einfacher Audio-Manager für den Workshop.
    /// Verwaltet Musik und Soundeffekte.
    ///
    /// WICHTIG: Hat zwei AudioSource-Komponenten direkt drauf:
    /// - Eine für Musik (loop, leiser)
    /// - Eine für SFX (one-shot)
    ///
    /// Wenn keine AudioClips zugewiesen sind, werden einfache
    /// Piep-Töne per Code generiert, damit IMMER Sound-Feedback da ist.
    /// Kinder können über das Audio Preview Panel bessere Sounds auswählen.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources (werden automatisch eingerichtet)")]
        [Tooltip("Für Hintergrundmusik — wird automatisch auf Loop gesetzt")]
        public AudioSource musicSource;
        [Tooltip("Für Soundeffekte — spielt One-Shots")]
        public AudioSource sfxSource;

        [Header("Musik")]
        public AudioClip backgroundMusic;
        [Range(0f, 1f)]
        public float musicVolume = 0.4f;

        [Header("Soundeffekte")]
        [Tooltip("Sound beim Einsammeln")]
        public AudioClip pickupSound;
        [Tooltip("Sound bei Schaden/Treffer")]
        public AudioClip damageSound;
        [Tooltip("Sound beim Gewinnen")]
        public AudioClip winSound;
        [Tooltip("Sound wenn alle Objekte gesammelt")]
        public AudioClip allCollectedSound;
        [Tooltip("Sound wenn Zielzone bereit")]
        public AudioClip goalReadySound;
        [Tooltip("Sound bei Schalter")]
        public AudioClip switchSound;
        [Tooltip("Sound bei Tür öffnen")]
        public AudioClip doorOpenSound;
        [Tooltip("Sound bei gesperrter Tür")]
        public AudioClip lockedSound;
        [Tooltip("Sound bei Checkpoint")]
        public AudioClip checkpointSound;

        [Header("Erweiterte SFX-Liste")]
        [Tooltip("Zusätzliche Sounds die per Name abgespielt werden können")]
        public SFXEntry[] extraSoundEffects;

        // Dictionary für schnellen Zugriff
        private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

        // Generierte Fallback-Sounds
        private Dictionary<string, AudioClip> generatedSounds = new Dictionary<string, AudioClip>();

        [System.Serializable]
        public class SFXEntry
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float volume = 1f;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupAudioSources();
            BuildSFXDictionary();
            GenerateFallbackSounds();
        }

        private void SetupAudioSources()
        {
            // AudioSources erstellen/finden
            var sources = GetComponents<AudioSource>();

            if (sources.Length >= 2)
            {
                musicSource = sources[0];
                sfxSource = sources[1];
            }
            else if (sources.Length == 1)
            {
                musicSource = sources[0];
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            // Musik-Source konfigurieren
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            // SFX-Source konfigurieren
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        private void BuildSFXDictionary()
        {
            // Benannte Sounds registrieren
            RegisterSound("Pickup", pickupSound);
            RegisterSound("Damage", damageSound);
            RegisterSound("Win", winSound);
            RegisterSound("AllCollected", allCollectedSound);
            RegisterSound("GoalReady", goalReadySound);
            RegisterSound("Switch", switchSound);
            RegisterSound("DoorOpen", doorOpenSound);
            RegisterSound("Locked", lockedSound);
            RegisterSound("Checkpoint", checkpointSound);

            // Extra-Sounds
            if (extraSoundEffects != null)
            {
                foreach (var entry in extraSoundEffects)
                {
                    if (entry.clip != null && !sfxDictionary.ContainsKey(entry.name))
                        sfxDictionary[entry.name] = entry.clip;
                }
            }
        }

        private void RegisterSound(string name, AudioClip clip)
        {
            if (clip != null)
                sfxDictionary[name] = clip;
        }

        /// <summary>
        /// Generiert einfache Piep-Töne als Fallback,
        /// damit IMMER Audio-Feedback da ist — auch ohne zugewiesene Clips.
        /// </summary>
        private void GenerateFallbackSounds()
        {
            generatedSounds["Pickup"] = GenerateTone(880f, 0.1f, ToneType.Sine);      // Hoher Piep
            generatedSounds["Damage"] = GenerateTone(220f, 0.2f, ToneType.Square);     // Tiefer Brumm
            generatedSounds["Win"] = GenerateJingle();                                    // Dreiklang
            generatedSounds["AllCollected"] = GenerateTone(660f, 0.15f, ToneType.Sine); // Mittlerer Piep
            generatedSounds["GoalReady"] = GenerateTone(550f, 0.2f, ToneType.Sine);
            generatedSounds["Switch"] = GenerateTone(440f, 0.08f, ToneType.Square);
            generatedSounds["DoorOpen"] = GenerateTone(330f, 0.15f, ToneType.Sine);
            generatedSounds["Locked"] = GenerateTone(150f, 0.2f, ToneType.Square);
            generatedSounds["Checkpoint"] = GenerateTone(660f, 0.12f, ToneType.Sine);
        }

        private void Start()
        {
            PlayMusic();
        }

        /// <summary>Spielt Hintergrundmusik</summary>
        public void PlayMusic()
        {
            if (backgroundMusic == null || musicSource == null) return;
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        /// <summary>Wechselt die Hintergrundmusik</summary>
        public void ChangeMusic(AudioClip newMusic, float fadeTime = 1f)
        {
            if (newMusic == null) return;
            backgroundMusic = newMusic;
            musicSource.clip = newMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        /// <summary>Stoppt die Musik</summary>
        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        /// <summary>Spielt einen Soundeffekt nach Name</summary>
        public void PlaySFX(string sfxName)
        {
            if (string.IsNullOrEmpty(sfxName) || sfxSource == null) return;

            // 1. Aus zugewiesenen Clips
            if (sfxDictionary.TryGetValue(sfxName, out AudioClip clip))
            {
                float originalPitch = sfxSource.pitch;
                sfxSource.pitch = Random.Range(0.9f, 1.1f);
                sfxSource.PlayOneShot(clip);
                sfxSource.pitch = originalPitch;
                return;
            }

            // 2. Fallback: Generierter Ton
            if (generatedSounds.TryGetValue(sfxName, out AudioClip generated))
            {
                sfxSource.PlayOneShot(generated, 0.5f);
                return;
            }
        }

        /// <summary>Spielt einen beliebigen Clip direkt</summary>
        public void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        // === Ton-Generierung ===

        private enum ToneType { Sine, Square }

        private AudioClip GenerateTone(float frequency, float duration, ToneType type)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.RoundToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // Linearer Fade-out
                envelope *= envelope; // Quadratisch für natürlicheren Abklang

                float wave;
                if (type == ToneType.Sine)
                    wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
                else
                    wave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frequency * t));

                samples[i] = wave * envelope * 0.3f;
            }

            var clip = AudioClip.Create($"Generated_{frequency}Hz", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateJingle()
        {
            int sampleRate = 44100;
            float noteDuration = 0.15f;
            float[] frequencies = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5 E5 G5 C6
            int totalSamples = Mathf.RoundToInt(sampleRate * noteDuration * frequencies.Length);
            float[] samples = new float[totalSamples];

            for (int n = 0; n < frequencies.Length; n++)
            {
                int noteStart = Mathf.RoundToInt(n * noteDuration * sampleRate);
                int noteSamples = Mathf.RoundToInt(noteDuration * sampleRate);

                for (int i = 0; i < noteSamples && (noteStart + i) < totalSamples; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = 1f - (t / noteDuration);
                    samples[noteStart + i] += Mathf.Sin(2f * Mathf.PI * frequencies[n] * t)
                                              * envelope * 0.25f;
                }
            }

            var clip = AudioClip.Create("Generated_Jingle", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Definition eines Themenwelt-Presets (Wald, Höhle, Schnee, Ruine, Wüste).
    /// Enthält alle visuellen und akustischen Einstellungen für ein Thema.
    /// Betreuer erstellen diese vorab — Kinder wählen nur aus.
    /// </summary>
    [CreateAssetMenu(fileName = "Neues Theme", menuName = "Workshop/Theme")]
    public class ThemeData : ScriptableObject
    {
        [Header("Grundinfo")]
        public string themeName = "Wald";
        [TextArea(1, 3)]
        public string description = "Ein dichter, geheimnisvoller Wald";
        public Sprite themeIcon;

        [Header("Farbpalette")]
        [Tooltip("Hauptfarbe des Themes (für UI-Akzente, Partikel etc.)")]
        public Color primaryColor = new Color(0.2f, 0.7f, 0.3f);
        public Color secondaryColor = new Color(0.4f, 0.3f, 0.15f);
        public Color accentColor = new Color(1f, 0.9f, 0.3f);

        [Header("Hintergrund")]
        public Color backgroundColor = new Color(0.15f, 0.2f, 0.1f);
        [Tooltip("Kamera-Hintergrundfarbe")]
        public Color cameraBackgroundColor = new Color(0.1f, 0.15f, 0.05f);

        [Header("Beleuchtung")]
        [Tooltip("Globale Lichtfarbe (für 2D Global Light)")]
        public Color ambientLightColor = Color.white;
        [Range(0.3f, 1.5f)]
        public float ambientIntensity = 1f;

        [Header("Audio")]
        public AudioClip backgroundMusic;
        public AudioClip ambientLoop;
        [Range(0f, 1f)]
        public float musicVolume = 0.4f;

        [Header("Prefab-Referenzen (kuratierte Auswahl)")]
        [Tooltip("Boden-Tiles/Tilemaps für dieses Thema")]
        public GameObject[] groundPrefabs;

        [Tooltip("Dekorations-Objekte (Bäume, Felsen, Lampen...)")]
        public GameObject[] decorPrefabs;

        [Tooltip("Sammelobjekte die zu diesem Thema passen")]
        public GameObject[] collectiblePrefabs;

        [Tooltip("Gefahren die zu diesem Thema passen")]
        public GameObject[] hazardPrefabs;

        [Tooltip("Wand/Rand-Prefabs")]
        public GameObject[] wallPrefabs;

        [Header("Partikeleffekte")]
        [Tooltip("Farbe für Pickup-Partikel in diesem Theme")]
        public Color pickupParticleColor = new Color(1f, 0.9f, 0.2f);

        [Tooltip("Umgebungspartikel (Blätter, Schneeflocken, Staubkörner...)")]
        public GameObject ambientParticlesPrefab;

        /// <summary>Wendet dieses Theme auf die aktive Szene an</summary>
        public void Apply()
        {
            // Kamera-Hintergrund
            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = cameraBackgroundColor;

            // Musik wechseln
            if (backgroundMusic != null)
                AudioManager.Instance?.ChangeMusic(backgroundMusic);

            // Partikelfarbe setzen
            var effectSpawner = Object.FindAnyObjectByType<PickupEffectSpawner>();
            if (effectSpawner != null)
                effectSpawner.defaultColor = pickupParticleColor;

            Debug.Log($"[Workshop] Theme '{themeName}' angewendet!");
        }
    }
}

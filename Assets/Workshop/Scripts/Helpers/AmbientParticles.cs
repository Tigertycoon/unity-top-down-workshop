using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Erzeugt atmosphärische Partikel (Blätter, Schneeflocken, Staubkörner)
    /// per Code. Kein Prefab nötig — einfach den Typ auswählen.
    /// </summary>
    public class AmbientParticles : MonoBehaviour
    {
        [Header("Partikel-Typ")]
        public AmbientType particleType = AmbientType.Leaves;

        [Header("Einstellungen")]
        [Range(1, 50)]
        public int particleCount = 15;

        [Range(0.1f, 3f)]
        public float particleSize = 0.15f;

        [Range(0.5f, 5f)]
        public float fallSpeed = 1.5f;

        public Color particleColor = new Color(0.4f, 0.7f, 0.2f, 0.6f);

        [Tooltip("Bereich in dem Partikel erscheinen")]
        public Vector2 spawnArea = new Vector2(20f, 12f);

        private ParticleSystem ps;

        private void Start()
        {
            CreateParticleSystem();
        }

        private void CreateParticleSystem()
        {
            ps = gameObject.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 8f;
            main.startSize = particleSize;
            main.maxParticles = particleCount * 3;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true;
            main.playOnAwake = true;

            // Typ-spezifische Einstellungen
            switch (particleType)
            {
                case AmbientType.Leaves:
                    main.startColor = particleColor;
                    main.startSpeed = fallSpeed;
                    main.startRotation3D = true;
                    main.gravityModifier = 0.3f;
                    break;

                case AmbientType.Snow:
                    main.startColor = new Color(1f, 1f, 1f, 0.7f);
                    main.startSpeed = fallSpeed * 0.5f;
                    main.gravityModifier = 0.15f;
                    break;

                case AmbientType.Dust:
                    main.startColor = new Color(0.8f, 0.7f, 0.5f, 0.3f);
                    main.startSpeed = fallSpeed * 0.3f;
                    main.gravityModifier = 0f;
                    break;

                case AmbientType.Fireflies:
                    main.startColor = new Color(1f, 1f, 0.5f, 0.8f);
                    main.startSpeed = fallSpeed * 0.2f;
                    main.gravityModifier = -0.05f;
                    main.startSize = particleSize * 0.5f;
                    break;

                case AmbientType.Embers:
                    main.startColor = new Color(1f, 0.4f, 0.1f, 0.7f);
                    main.startSpeed = fallSpeed * 0.8f;
                    main.gravityModifier = -0.2f;
                    break;
            }

            // Emission
            var emission = ps.emission;
            emission.rateOverTime = particleCount;

            // Form: Breiter Bereich über dem Spielfeld
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(spawnArea.x, 1f, spawnArea.y);

            if (particleType == AmbientType.Fireflies || particleType == AmbientType.Dust)
            {
                // Überall im Bereich spawnen
                shape.scale = new Vector3(spawnArea.x, spawnArea.y, 1f);

                // Noise für organischere Bewegung
                var noise = ps.noise;
                noise.enabled = true;
                noise.strength = 0.5f;
                noise.frequency = 0.5f;
            }

            // Alpha über Lebenszeit
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.1f),
                    new GradientAlphaKey(1f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Renderer
            var renderer = GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 50;
        }
    }

    public enum AmbientType
    {
        [Tooltip("Fallende Blätter — für Wald")]
        Leaves,

        [Tooltip("Schneeflocken — für Schnee/Winter")]
        Snow,

        [Tooltip("Schwebender Staub — für Höhle/Ruine")]
        Dust,

        [Tooltip("Leuchtende Glühwürmchen — für Nachtszenen")]
        Fireflies,

        [Tooltip("Glühende Funken — für Lava/Feuer-Bereiche")]
        Embers
    }
}

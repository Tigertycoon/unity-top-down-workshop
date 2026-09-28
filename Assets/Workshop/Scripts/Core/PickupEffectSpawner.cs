using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Erzeugt Partikeleffekte beim Einsammeln von Objekten.
    /// Wird in der Szene platziert und von Collectibles aufgerufen.
    /// </summary>
    public class PickupEffectSpawner : MonoBehaviour
    {
        [Header("Effekt-Einstellungen")]
        public Color defaultColor = new Color(1f, 0.9f, 0.2f); // Gold
        public int particleCount = 12;
        public float particleSpeed = 3f;
        public float particleLifetime = 0.6f;
        public float particleSize = 0.15f;

        /// <summary>Spawnt einen Pickup-Effekt an der gegebenen Position</summary>
        public void SpawnAt(Vector3 position)
        {
            SpawnAt(position, defaultColor);
        }

        /// <summary>Spawnt einen Pickup-Effekt mit bestimmter Farbe</summary>
        public void SpawnAt(Vector3 position, Color color)
        {
            // ParticleSystem per Code erstellen
            var go = new GameObject("PickupEffect");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();

            // WICHTIG: Erst stoppen bevor wir konfigurieren,
            // weil Unity das System sonst automatisch startet
            // und dann die duration-Änderung einen Fehler wirft.
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = 0.1f;
            main.startLifetime = particleLifetime;
            main.startSpeed = particleSpeed;
            main.startSize = particleSize;
            main.startColor = color;
            main.maxParticles = particleCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = false;
            main.playOnAwake = false; // Manuell starten!
            main.gravityModifier = 0.5f;

            // Burst-Emission
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] {
                new ParticleSystem.Burst(0f, (short)particleCount)
            });

            // Kreisförmig ausstoßen
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            // Über Lebenszeit kleiner werden
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // Farbe ausblenden
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            // Renderer-Material
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = color;

            // Jetzt erst abspielen
            ps.Play();

            // Aufräumen nach Lebenszeit
            Destroy(go, particleLifetime + 0.5f);
        }
    }
}

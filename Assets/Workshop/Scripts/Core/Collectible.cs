using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Ein einsammelbares Objekt. Einfach auf ein Sprite-GameObject legen.
    /// Das Objekt "bobbelt" sanft auf und ab und hat einen Leuchteffekt.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [Header("Einstellungen")]
        [Tooltip("Wie viel dieses Objekt zur Sammlung beiträgt")]
        [Min(1)]
        public int value = 1;

        [Tooltip("Art des Sammelobjekts (für verschiedene Zähler)")]
        public CollectibleType collectibleType = CollectibleType.Standard;

        [Header("Animation")]
        [Tooltip("Schwebeanimation aktivieren")]
        public bool bobbing = true;

        [Tooltip("Höhe der Schwebeanimation")]
        [Range(0f, 0.5f)]
        public float bobHeight = 0.15f;

        [Tooltip("Geschwindigkeit der Schwebeanimation")]
        [Range(0.5f, 5f)]
        public float bobSpeed = 2f;

        [Header("Effekte")]
        [Tooltip("Partikeleffekt beim Einsammeln (wird automatisch erzeugt wenn leer)")]
        public GameObject pickupEffectPrefab;

        // Intern
        private Vector3 originalPosition;
        private float bobOffset;
        private bool collected = false;

        private void Start()
        {
            originalPosition = transform.position;
            // Zufälliger Startpunkt, damit nicht alle Objekte synchron schweben
            bobOffset = Random.Range(0f, Mathf.PI * 2f);

            // Collider muss Trigger sein
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            if (collected) return;

            if (bobbing)
            {
                float newY = originalPosition.y +
                    Mathf.Sin((Time.time * bobSpeed) + bobOffset) * bobHeight;
                transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected) return;

            // Nur der Spieler kann einsammeln
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;

            collected = true;

            // Mission-Manager benachrichtigen
            MissionManager.Instance?.CollectItem(this);

            // Visueller Effekt
            SpawnPickupEffect();

            // Audio
            AudioManager.Instance?.PlaySFX("Pickup");

            // Objekt deaktivieren (nicht zerstören — für Reset)
            gameObject.SetActive(false);
        }

        private void SpawnPickupEffect()
        {
            if (pickupEffectPrefab != null)
            {
                var effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            else
            {
                // Fallback: Einfacher Skalierungs-Effekt
                // (Partikeleffekt wird vom PickupEffectSpawner erstellt, falls vorhanden)
                var spawner = FindAnyObjectByType<PickupEffectSpawner>();
                spawner?.SpawnAt(transform.position);
            }
        }

        /// <summary>Setzt das Objekt zurück (für Level-Reset)</summary>
        public void ResetCollectible()
        {
            collected = false;
            transform.position = originalPosition;
            gameObject.SetActive(true);
        }
    }

    public enum CollectibleType
    {
        Standard,   // Kristalle, Münzen etc.
        Key,        // Schlüssel (öffnet Türen)
        Bonus,      // Bonusobjekte (optional)
        Artifact    // Spezielle Artefakte
    }
}

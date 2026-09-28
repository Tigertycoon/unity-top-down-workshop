using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Top-Down-Bewegung für den Spielercharakter.
    /// Nutzt Rigidbody2D für physikbasierte Bewegung.
    ///
    /// ANIMATOR-KOMPATIBILITÄT:
    /// Arbeitet mit dem Super Retro Collection Animator zusammen,
    /// der "speed" (float) und "orientation" (int) als Parameter hat.
    /// Orientierung: 0=oben, 2=links, 4=unten, 6=rechts.
    ///
    /// Falls CharacterAppearance (aus dem Asset-Pack) auf dem gleichen
    /// GameObject liegt, überlässt dieses Script die Animation-Updates
    /// dem CharacterAppearance-Script, da es die Sprite-Sheet-Logik
    /// korrekt handhabt.
    ///
    /// Kinder müssen dieses Script NICHT anfassen — es funktioniert out of the box.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Bewegung")]
        [Tooltip("Geschwindigkeit der Figur")]
        [Range(1f, 15f)]
        public float moveSpeed = 4f;

        [Header("Unverwundbarkeit nach Treffer")]
        [Tooltip("Sekunden, in denen der Spieler nach einem Treffer nicht erneut Schaden nehmen kann")]
        public float invincibilityDuration = 1.5f;

        [Header("Knockback")]
        [Tooltip("Stärke des Rückstoßes bei Gefahr")]
        public float knockbackForce = 8f;

        [Header("Mehrfach-Treffer → Reset")]
        [Tooltip("Nach wie vielen Treffern wird der Spieler zum Start zurückgesetzt (0 = nie)")]
        [Range(0, 10)]
        public int hitsBeforeReset = 3;

        // --- Intern ---
        private Rigidbody2D rb;
        private Vector2 movement;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private bool hasCharacterAppearance = false;

        private bool isInvincible = false;
        private float invincibilityTimer = 0f;
        private bool isKnockedBack = false;
        private float knockbackTimer = 0f;
        private const float KNOCKBACK_DURATION = 0.25f;

        // Treffer-Zähler für Reset-Mechanik
        private int currentHitCount = 0;

        // Letzte Orientierung (für Idle)
        private int lastOrientation = 4; // Standard: nach unten schauend

        // Startposition für Reset
        private Vector3 startPosition;
        private Vector3 initialPosition;

        // Singleton für einfachen Zugriff
        public static PlayerController Instance { get; private set; }

        /// <summary>Ist der Spieler gerade unverwundbar?</summary>
        public bool IsInvincible => isInvincible;

        /// <summary>Aktuelle Trefferzahl seit letztem Reset</summary>
        public int CurrentHitCount => currentHitCount;

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Prüfen ob CharacterAppearance (aus Asset-Pack) vorhanden ist
            // Wenn ja, übernimmt es die Animator-Updates
            hasCharacterAppearance = GetComponent("CharacterAppearance") != null;

            // Rigidbody-Setup für Top-Down
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            startPosition = transform.position;
            initialPosition = startPosition;
        }

        private void OnDisable()
        {
            movement = Vector2.zero;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        public void ResetForMission()
        {
            startPosition = initialPosition;
            ResetToStart();
        }

        private void Update()
        {
            // Keine Eingabe während Knockback
            if (isKnockedBack)
            {
                knockbackTimer -= Time.deltaTime;
                if (knockbackTimer <= 0f)
                    isKnockedBack = false;
                // Kein return hier — Unverwundbarkeit muss weiterlaufen
            }

            // Eingabe lesen (nur wenn nicht im Knockback)
            if (!isKnockedBack)
            {
                movement.x = Input.GetAxisRaw("Horizontal");
                movement.y = Input.GetAxisRaw("Vertical");

                // Diagonal-Bewegung normalisieren (gleiche Speed in alle Richtungen)
                if (movement.sqrMagnitude > 1f)
                    movement.Normalize();
            }

            // Animator-Parameter setzen NUR wenn CharacterAppearance NICHT vorhanden ist
            // (CharacterAppearance handhabt Animation selbst über Position-Delta)
            if (animator != null && !hasCharacterAppearance)
            {
                float speed = Mathf.Abs(movement.x) + Mathf.Abs(movement.y);
                animator.SetFloat("speed", speed);

                // Orientierung als Integer: 0=oben, 2=links, 4=unten, 6=rechts
                if (movement.x > 0.1f)
                    lastOrientation = 6;
                else if (movement.x < -0.1f)
                    lastOrientation = 2;
                else if (movement.y > 0.1f)
                    lastOrientation = 0;
                else if (movement.y < -0.1f)
                    lastOrientation = 4;

                animator.SetInteger("orientation", lastOrientation);
            }

            // Unverwundbarkeits-Timer und Blink-Effekt
            if (isInvincible)
            {
                invincibilityTimer -= Time.deltaTime;

                // Blink-Effekt
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f) > 0.5f ? 1f : 0.3f;
                    Color c = spriteRenderer.color;
                    c.a = alpha;
                    spriteRenderer.color = c;
                }

                if (invincibilityTimer <= 0f)
                {
                    isInvincible = false;
                    if (spriteRenderer != null)
                    {
                        Color c = spriteRenderer.color;
                        c.a = 1f;
                        spriteRenderer.color = c;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (isKnockedBack) return;

            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Wird von Hazards aufgerufen. Knockback + Unverwundbarkeit.
        /// Nach X Treffern (konfigurierbar) wird stattdessen resettet.
        /// </summary>
        public void TakeDamage(Vector2 damageSource)
        {
            if (isInvincible) return;

            currentHitCount++;

            // Prüfen ob Reset ausgelöst werden soll
            if (hitsBeforeReset > 0 && currentHitCount >= hitsBeforeReset)
            {
                ResetToStart();
                MissionManager.Instance?.OnPlayerHit();
                return;
            }

            // Knockback-Richtung berechnen
            Vector2 knockDir = ((Vector2)transform.position - damageSource).normalized;
            rb.linearVelocity = knockDir * knockbackForce;

            isKnockedBack = true;
            knockbackTimer = KNOCKBACK_DURATION;

            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            // Event für Audio etc.
            MissionManager.Instance?.OnPlayerHit();
        }

        /// <summary>Setzt Spieler an Startposition zurück (für harte Gefahren)</summary>
        public void ResetToStart()
        {
            transform.position = startPosition;
            rb.linearVelocity = Vector2.zero;
            movement = Vector2.zero;
            isKnockedBack = false;
            currentHitCount = 0;

            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }

        /// <summary>Setzt die Startposition neu (z.B. bei Checkpoint)</summary>
        public void SetCheckpoint(Vector3 pos)
        {
            startPosition = pos;
            currentHitCount = 0; // Treffer-Zähler bei Checkpoint zurücksetzen
        }
    }
}

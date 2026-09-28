using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Eine Gefahrenzone oder ein Gefahrenobjekt.
    /// Kann als Zone (Trigger) oder als bewegliches Objekt funktionieren.
    ///
    /// Zwei Modi:
    /// - Knockback (Standard): Schubst den Spieler zurück + kurze Unverwundbarkeit
    /// - Reset: Setzt den Spieler zum Start zurück (für Abgründe, Lava etc.)
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        [Header("Gefahren-Typ")]
        public HazardMode mode = HazardMode.Knockback;

        [Header("Patrouille (optional)")]
        [Tooltip("Bewegt sich zwischen Startposition und Zielpunkt")]
        public bool patrol = false;
        public Transform patrolTarget;
        [Range(0.5f, 5f)]
        public float patrolSpeed = 2f;

        [Header("Visuelle Warnung")]
        [Tooltip("Farbe der Gefahrenzone im Editor")]
        public Color gizmoColor = new Color(1f, 0.2f, 0.2f, 0.3f);

        // Intern
        private Vector3 startPos;
        private Vector3 endPos;
        private bool movingToEnd = true;

        private void Start()
        {
            // Collider als Trigger setzen
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            startPos = transform.position;
            if (patrolTarget != null)
                endPos = patrolTarget.position;
        }

        public void ResetHazard()
        {
            transform.position = startPos;
            movingToEnd = true;
        }

        private void Update()
        {
            if (!patrol || patrolTarget == null) return;

            Vector3 target = movingToEnd ? endPos : startPos;
            transform.position = Vector3.MoveTowards(transform.position, target,
                patrolSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
                movingToEnd = !movingToEnd;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null || player.IsInvincible) return;

            switch (mode)
            {
                case HazardMode.Knockback:
                    player.TakeDamage(transform.position);
                    break;

                case HazardMode.Reset:
                    player.ResetToStart();
                    AudioManager.Instance?.PlaySFX("Damage");
                    break;
            }
        }

        // Für kontinuierliche Gefahrenzonen (z.B. Lava)
        private void OnTriggerStay2D(Collider2D other)
        {
            if (mode != HazardMode.Reset) return;

            var player = other.GetComponent<PlayerController>();
            if (player == null || player.IsInvincible) return;

            player.ResetToStart();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;

            var col = GetComponent<Collider2D>();
            if (col is BoxCollider2D box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }

            // Patrouillenpfad zeichnen
            if (patrol && patrolTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, patrolTarget.position);
                Gizmos.DrawWireSphere(patrolTarget.position, 0.2f);
            }
        }
    }

    public enum HazardMode
    {
        [Tooltip("Schubst den Spieler zurück")]
        Knockback,

        [Tooltip("Setzt den Spieler zum Start zurück")]
        Reset
    }
}

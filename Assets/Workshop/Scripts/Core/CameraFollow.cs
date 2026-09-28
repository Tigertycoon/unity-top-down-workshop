using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Sanfte Kamerasteuerung für Top-Down-Spiele.
    /// Folgt dem Spieler mit einstellbarer Verzögerung.
    /// Optional: Kamera-Grenzen, damit man nicht über den Levelrand hinausschaut.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Ziel")]
        [Tooltip("Wird automatisch auf den Spieler gesetzt, wenn leer")]
        public Transform target;

        [Header("Kameraverhalten")]
        [Tooltip("Wie schnell die Kamera folgt (höher = schneller)")]
        [Range(1f, 20f)]
        public float smoothSpeed = 8f;

        [Tooltip("Versatz der Kamera relativ zum Spieler")]
        public Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Kamera-Grenzen (optional)")]
        [Tooltip("Aktiviert Kamera-Grenzen, damit das Level nicht überschaut wird")]
        public bool useBounds = false;
        public Vector2 minBounds;
        public Vector2 maxBounds;

        [Header("Vorschau-Versatz")]
        [Tooltip("Schaut ein Stück in Bewegungsrichtung voraus")]
        [Range(0f, 3f)]
        public float lookAheadDistance = 1f;

        private Vector3 currentVelocity;

        private void Start()
        {
            // Automatisch Spieler finden
            if (target == null)
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                    target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Zielposition mit Vorschau
            Vector3 desiredPosition = target.position + offset;

            // Sanftes Nachfolgen
            Vector3 smoothed = Vector3.SmoothDamp(
                transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

            // Grenzen anwenden
            if (useBounds)
            {
                smoothed.x = Mathf.Clamp(smoothed.x, minBounds.x, maxBounds.x);
                smoothed.y = Mathf.Clamp(smoothed.y, minBounds.y, maxBounds.y);
            }

            smoothed.z = offset.z;
            transform.position = smoothed;
        }

        /// <summary>Setzt die Kameragrenzen basierend auf einem Collider (z.B. Level-Bounds)</summary>
        public void SetBoundsFromCollider(Collider2D boundsCollider)
        {
            if (boundsCollider == null) return;
            useBounds = true;
            Bounds b = boundsCollider.bounds;

            Camera cam = GetComponent<Camera>();
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            minBounds = new Vector2(b.min.x + halfWidth, b.min.y + halfHeight);
            maxBounds = new Vector2(b.max.x - halfWidth, b.max.y - halfHeight);
        }
    }
}

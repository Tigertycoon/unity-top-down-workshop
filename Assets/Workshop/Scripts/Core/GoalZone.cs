using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Die Zielzone der Mission. Wenn der Spieler sie betritt
    /// und genug Objekte gesammelt hat, wird die Mission abgeschlossen.
    ///
    /// Kann optional erst aktiviert werden, wenn alle Sammelobjekte
    /// gefunden wurden (konfigurierbar).
    ///
    /// Erstellt automatisch ein sichtbares Quadrat-Sprite wenn
    /// keins zugewiesen ist, damit die Zone immer sichtbar ist.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoalZone : MonoBehaviour
    {
        [Header("Ziel-Einstellungen")]
        [Tooltip("Muss der Spieler ALLE Sammelobjekte haben, um das Ziel zu erreichen?")]
        public bool requireAllCollectibles = true;

        [Tooltip("Mindestanzahl Sammelobjekte (nur wenn requireAll = false)")]
        public int minimumCollectibles = 3;

        [Header("Visuelle Hinweise")]
        [Tooltip("Farbe wenn das Ziel noch gesperrt ist")]
        public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [Tooltip("Farbe wenn das Ziel aktiv ist")]
        public Color activeColor = new Color(0.2f, 1f, 0.4f, 0.6f);

        [Header("Größe")]
        [Tooltip("Größe der Zielzone in Unity-Einheiten")]
        public Vector2 zoneSize = new Vector2(2f, 2f);

        // Intern
        private bool isActive = false;
        private SpriteRenderer spriteRenderer;

        // Pulsier-Animation
        private float pulseTimer = 0f;

        public bool IsActive => isActive;

        private void Start()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            // Collider-Größe an zoneSize anpassen
            if (col is BoxCollider2D box)
                box.size = Vector2.one; // Root scale below supplies the requested world size once.

            spriteRenderer = GetComponent<SpriteRenderer>();

            // Automatisch ein Sprite erstellen wenn keins da ist
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (spriteRenderer.sprite == null)
                spriteRenderer.sprite = CreateDefaultSprite();

            spriteRenderer.sortingOrder = 3;
            transform.localScale = new Vector3(zoneSize.x, zoneSize.y, 1f);

            UpdateVisuals();
        }

        /// <summary>Erstellt ein einfaches weißes Quadrat-Sprite per Code</summary>
        private Sprite CreateDefaultSprite()
        {
            int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 255, 255, 255);

            tex.SetPixels32(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private void Update()
        {
            // Prüfe ob genug Objekte gesammelt wurden
            bool shouldBeActive = CheckRequirements();

            if (shouldBeActive != isActive)
            {
                isActive = shouldBeActive;
                UpdateVisuals();

                if (isActive)
                    AudioManager.Instance?.PlaySFX("GoalReady");
            }

            // Pulsier-Effekt wenn aktiv
            if (isActive && spriteRenderer != null)
            {
                pulseTimer += Time.deltaTime;
                float pulse = 0.4f + Mathf.Sin(pulseTimer * 3f) * 0.2f;
                Color c = activeColor;
                c.a = pulse;
                spriteRenderer.color = c;
            }
        }

        private bool CheckRequirements()
        {
            var mission = MissionManager.Instance;
            if (mission == null) return true;

            if (requireAllCollectibles)
                return mission.CollectedCount >= mission.TotalCollectibles;
            else
                return mission.CollectedCount >= minimumCollectibles;
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
                spriteRenderer.color = isActive ? activeColor : lockedColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive) return;

            var player = other.GetComponent<PlayerController>();
            if (player == null) return;

            // Mission abschließen!
            MissionManager.Instance?.CompleteMission();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0.3f, 0.4f);
            Gizmos.DrawCube(transform.position, new Vector3(zoneSize.x, zoneSize.y, 0.1f));
            Gizmos.color = new Color(0f, 1f, 0.3f, 0.8f);
            Gizmos.DrawWireCube(transform.position, new Vector3(zoneSize.x, zoneSize.y, 0.1f));

            // Label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * (zoneSize.y / 2f + 0.3f),
                "ZIEL", new GUIStyle() {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter
                });
            #endif
        }
    }
}

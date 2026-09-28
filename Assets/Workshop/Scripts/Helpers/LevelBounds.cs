using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Definiert die Grenzen des Levels.
    /// Erstellt 4 unsichtbare Wände (BoxCollider2D) am Rand,
    /// die den Spieler physisch aufhalten.
    /// Setzt auch die Kameragrenzen.
    ///
    /// Funktionsweise: Beim Start werden 4 Kind-Objekte mit dicken
    /// BoxCollider2D erstellt (oben, unten, links, rechts).
    /// Die Physik-Engine kümmert sich um den Rest — kein Clamping nötig.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelBounds : MonoBehaviour
    {
        [Header("Einstellungen")]
        [Tooltip("Kamera auf diesen Bereich begrenzen")]
        public bool constrainCamera = true;

        [Tooltip("Spieler am Rand aufhalten (unsichtbare Wände)")]
        public bool constrainPlayer = true;

        [Tooltip("Farbe der Grenzen im Editor")]
        public Color boundsColor = new Color(0.2f, 0.5f, 1f, 0.3f);

        // Dicke der unsichtbaren Wände
        private const float WALL_THICKNESS = 2f;

        private BoxCollider2D boundsCollider;

        public static LevelBounds Instance { get; private set; }

        private void Start()
        {
            Instance = this;
            boundsCollider = GetComponent<BoxCollider2D>();
            // Haupt-Collider als Trigger lassen (für Referenz), nicht als Wand
            boundsCollider.isTrigger = true;

            Bounds b = boundsCollider.bounds;
            Debug.Log($"[LevelBounds] Aktiv! Bereich: X({b.min.x:F1} bis {b.max.x:F1}), Y({b.min.y:F1} bis {b.max.y:F1})");

            if (constrainPlayer)
                CreateWalls(b);

            if (constrainCamera)
            {
                var camFollow = FindAnyObjectByType<CameraFollow>();
                if (camFollow != null)
                    camFollow.SetBoundsFromCollider(boundsCollider);
            }
        }

        /// <summary>
        /// Erstellt 4 unsichtbare Wände als Kind-Objekte.
        /// Jede Wand ist ein BoxCollider2D der dick genug ist,
        /// dass kein Rigidbody durchrutschen kann.
        /// </summary>
        private void CreateWalls(Bounds b)
        {
            float w = b.size.x;
            float h = b.size.y;
            Vector3 center = b.center;

            // Oben
            CreateWall("Wand_Oben", center + new Vector3(0, h / 2f + WALL_THICKNESS / 2f, 0),
                new Vector2(w + WALL_THICKNESS * 2, WALL_THICKNESS));

            // Unten
            CreateWall("Wand_Unten", center + new Vector3(0, -h / 2f - WALL_THICKNESS / 2f, 0),
                new Vector2(w + WALL_THICKNESS * 2, WALL_THICKNESS));

            // Links
            CreateWall("Wand_Links", center + new Vector3(-w / 2f - WALL_THICKNESS / 2f, 0, 0),
                new Vector2(WALL_THICKNESS, h + WALL_THICKNESS * 2));

            // Rechts
            CreateWall("Wand_Rechts", center + new Vector3(w / 2f + WALL_THICKNESS / 2f, 0, 0),
                new Vector2(WALL_THICKNESS, h + WALL_THICKNESS * 2));
        }

        private void CreateWall(string name, Vector3 position, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(transform);
            wall.transform.position = position;
            wall.layer = gameObject.layer;

            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
            // KEIN Trigger — das ist eine echte physische Wand!
            col.isTrigger = false;
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null) return;

            Gizmos.color = boundsColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);

            // Wände andeuten
            Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.15f);
            Gizmos.DrawCube(col.offset, col.size);

            // Ecken markieren
            Vector2 half = col.size / 2f;
            Gizmos.color = new Color(boundsColor.r, boundsColor.g, boundsColor.b, 0.8f);
            float cornerSize = 0.3f;
            Vector2[] corners = {
                new Vector2(-half.x, -half.y),
                new Vector2(half.x, -half.y),
                new Vector2(half.x, half.y),
                new Vector2(-half.x, half.y)
            };
            foreach (var c in corners)
                Gizmos.DrawWireCube((Vector3)col.offset + (Vector3)c, Vector3.one * cornerSize);
        }
    }
}

using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Zeigt einen Richtungspfeil zum nächsten Sammelobjekt an,
    /// wenn es keines mehr auf dem Bildschirm gibt.
    /// </summary>
    public class CollectibleArrow : MonoBehaviour
    {
        [Header("Pfeil-Einstellungen")]
        [Tooltip("Entfernung des Pfeils vom Spieler")]
        public float arrowDistance = 1.5f;

        [Tooltip("Farbe des Pfeils")]
        public Color arrowColor = new Color(1f, 0.85f, 0.2f, 0.7f);

        [Tooltip("Größe des Pfeils")]
        public float arrowSize = 0.4f;

        [Tooltip("Erst anzeigen, wenn der Spieler X Sekunden nichts gesammelt hat")]
        public float showDelay = 5f;

        // Intern
        private GameObject arrowObject;
        private SpriteRenderer arrowRenderer;
        private float timeSinceLastCollect = 0f;
        private int lastCollectedCount = 0;
        private Camera mainCam;

        private void Start()
        {
            CreateArrow();
            mainCam = Camera.main;
            if (mainCam == null)
                mainCam = FindAnyObjectByType<Camera>();
        }

        private void CreateArrow()
        {
            arrowObject = new GameObject("CollectibleArrow");
            arrowObject.transform.SetParent(transform);

            arrowRenderer = arrowObject.AddComponent<SpriteRenderer>();
            arrowRenderer.sortingOrder = 100;
            arrowRenderer.color = arrowColor;

            // Einfaches Dreieck als Sprite erstellen
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color32[32 * 32];

            // Dreieck zeichnen (Pfeil zeigt nach oben)
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float normalizedY = y / 31f;
                    float halfWidth = (1f - normalizedY) * 0.5f;
                    float normalizedX = x / 31f - 0.5f;

                    if (Mathf.Abs(normalizedX) <= halfWidth)
                        pixels[y * 32 + x] = new Color32(255, 255, 255, 255);
                    else
                        pixels[y * 32 + x] = new Color32(0, 0, 0, 0);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            arrowRenderer.sprite = Sprite.Create(tex,
                new Rect(0, 0, 32, 32),
                new Vector2(0.5f, 0.5f),
                32f);

            arrowObject.transform.localScale = Vector3.one * arrowSize;
            arrowObject.SetActive(false);
        }

        private void Update()
        {
            if (arrowObject == null) return;

            var mission = MissionManager.Instance;
            var player = PlayerController.Instance;
            if (mission == null || player == null || mission.MissionCompleted)
            {
                arrowObject.SetActive(false);
                return;
            }

            // Kamera cachen falls noch nicht vorhanden
            if (mainCam == null)
            {
                mainCam = Camera.main;
                if (mainCam == null)
                    mainCam = FindAnyObjectByType<Camera>();
                if (mainCam == null)
                {
                    arrowObject.SetActive(false);
                    return;
                }
            }

            // Timer aktualisieren
            if (mission.CollectedCount != lastCollectedCount)
            {
                lastCollectedCount = mission.CollectedCount;
                timeSinceLastCollect = 0f;
            }
            timeSinceLastCollect += Time.deltaTime;

            // Nächstes aktives Collectible finden
            Collectible nearest = FindNearestCollectible(player.transform.position);

            if (nearest == null || timeSinceLastCollect < showDelay)
            {
                arrowObject.SetActive(false);
                return;
            }

            // Ist das Collectible auf dem Bildschirm sichtbar?
            Vector3 viewportPos = mainCam.WorldToViewportPoint(nearest.transform.position);
            bool isVisible = viewportPos.x > 0.05f && viewportPos.x < 0.95f &&
                            viewportPos.y > 0.05f && viewportPos.y < 0.95f &&
                            viewportPos.z > 0f;

            if (isVisible)
            {
                arrowObject.SetActive(false);
                return;
            }

            // Pfeil anzeigen und ausrichten
            arrowObject.SetActive(true);

            Vector3 direction = (nearest.transform.position - player.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            arrowObject.transform.position = player.transform.position + direction * arrowDistance;
            arrowObject.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Pulsieren
            float pulse = 0.8f + Mathf.Sin(Time.time * 3f) * 0.2f;
            Color c = arrowColor;
            c.a = arrowColor.a * pulse;
            arrowRenderer.color = c;
        }

        private Collectible FindNearestCollectible(Vector3 from)
        {
            Collectible nearest = null;
            float nearestDist = float.MaxValue;

            var all = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
            foreach (var c in all)
            {
                if (!c.gameObject.activeSelf) continue;

                float dist = Vector3.Distance(from, c.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = c;
                }
            }

            return nearest;
        }
    }
}

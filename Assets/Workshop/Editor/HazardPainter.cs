using UnityEngine;
using UnityEditor;

namespace Workshop.EditorTools
{
    /// <summary>
    /// HAZARD PAINTER / ZONE TOOL
    /// Gefahrenzonen einfach per Klick-und-Ziehen malen.
    /// Viel einfacher als einzelne Collider manuell zu bauen.
    /// </summary>
    public class HazardPainter : EditorWindow
    {
        private HazardMode paintMode = HazardMode.Knockback;
        private float zoneWidth = 2f;
        private float zoneHeight = 2f;

        [MenuItem("Workshop/Gefahrenzonen malen", false, 65)]
        public static void ShowWindow()
        {
            var window = GetWindow<HazardPainter>("Gefahrenzonen");
            window.minSize = new Vector2(280, 350);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("⚠ Gefahrenzonen", titleStyle);
            EditorGUILayout.Space(10);

            // Modus
            EditorGUILayout.LabelField("Art der Gefahr", EditorStyles.boldLabel);
            paintMode = (HazardMode)EditorGUILayout.EnumPopup("Gefahren-Typ", paintMode);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                paintMode == HazardMode.Knockback ?
                    "Knockback: Schubst den Spieler zurück (nicht zu hart)" :
                    "Reset: Setzt den Spieler zum Start zurück (Abgründe, Lava)",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Größe
            EditorGUILayout.LabelField("Größe der Zone", EditorStyles.boldLabel);
            zoneWidth = EditorGUILayout.Slider("Breite", zoneWidth, 0.5f, 10f);
            zoneHeight = EditorGUILayout.Slider("Höhe", zoneHeight, 0.5f, 10f);

            EditorGUILayout.Space(5);

            // Schnellgrößen
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Klein")) { zoneWidth = 1f; zoneHeight = 1f; }
            if (GUILayout.Button("Mittel")) { zoneWidth = 2f; zoneHeight = 2f; }
            if (GUILayout.Button("Groß")) { zoneWidth = 4f; zoneHeight = 4f; }
            if (GUILayout.Button("Streifen")) { zoneWidth = 6f; zoneHeight = 1f; }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);

            // Erstellen-Button
            Color btnColor = paintMode == HazardMode.Knockback ?
                new Color(1f, 0.6f, 0.2f) : new Color(1f, 0.2f, 0.1f);
            GUI.backgroundColor = btnColor;

            if (GUILayout.Button("Gefahrenzone erstellen!", GUILayout.Height(40)))
            {
                CreateHazardZone();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // Info über bestehende Zonen
            var hazards = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None);
            EditorGUILayout.LabelField($"Gefahren im Level: {hazards.Length}",
                EditorStyles.miniLabel);
        }

        private void CreateHazardZone()
        {
            // Position: Mitte des SceneView
            Vector3 pos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                pos = SceneView.lastActiveSceneView.camera.transform.position;
                pos.z = 0;
            }

            int count = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None).Length + 1;

            var go = new GameObject($"Gefahrenzone_{count}");
            go.transform.position = pos;

            // Sprite für visuelle Darstellung
            var sr = WorkshopShapes.AddRenderer(go);
            sr.sortingOrder = 1;

            Color zoneColor = paintMode == HazardMode.Knockback ?
                new Color(1f, 0.4f, 0.1f, 0.4f) : new Color(1f, 0.1f, 0.05f, 0.5f);
            sr.color = zoneColor;

            // Collider
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(zoneWidth, zoneHeight);
            col.isTrigger = true;

            // Skalierung für visuelles Feedback
            go.transform.localScale = new Vector3(zoneWidth, zoneHeight, 1f);
            col.size = Vector2.one; // Collider passt sich der Skalierung an

            // Hazard-Komponente
            var hazard = go.AddComponent<Hazard>();
            hazard.mode = paintMode;

            // In Container packen
            var container = GameObject.Find("--- GEFAHREN ---");
            if (container != null)
                go.transform.SetParent(container.transform);

            Undo.RegisterCreatedObjectUndo(go, "Gefahrenzone erstellt");
            Selection.activeGameObject = go;

            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();

            Debug.Log($"[Workshop] Gefahrenzone ({paintMode}) erstellt: " +
                     $"{zoneWidth}x{zoneHeight}");
        }
    }
}

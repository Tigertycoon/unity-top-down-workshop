using UnityEngine;
using UnityEditor;

namespace Workshop.EditorTools
{
    /// <summary>
    /// COLLECTIBLE MANAGER
    /// Zeigt alle Sammelobjekte in der Szene übersichtlich an.
    /// Kinder können hier sehen, wie viele sie haben und schnell
    /// zu einzelnen Objekten springen.
    /// </summary>
    public class CollectibleManagerEditor : EditorWindow
    {
        private Vector2 scrollPos;

        [MenuItem("Workshop/Sammelobjekte verwalten", false, 60)]
        public static void ShowWindow()
        {
            var window = GetWindow<CollectibleManagerEditor>("Sammelobjekte");
            window.minSize = new Vector2(300, 350);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("⭐ Sammelobjekte", titleStyle);
            EditorGUILayout.Space(5);

            var collectibles = Object.FindObjectsByType<Collectible>(FindObjectsSortMode.InstanceID);

            // Zusammenfassung
            EditorGUILayout.HelpBox(
                $"Du hast {collectibles.Length} Sammelobjekte in deinem Level.\n" +
                (collectibles.Length < 3 ? "Tipp: Mindestens 3 sind gut!" :
                 collectibles.Length > 10 ? "Das sind ganz schön viele! Vielleicht ein paar weniger?" :
                 "Das klingt gut!"),
                collectibles.Length < 1 ? MessageType.Warning : MessageType.Info);

            EditorGUILayout.Space(5);

            // Schnell-Hinzufügen
            GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
            if (GUILayout.Button("+ Neues Sammelobjekt hinzufügen", GUILayout.Height(30)))
            {
                Vector3 pos = Vector3.zero;
                if (SceneView.lastActiveSceneView != null)
                {
                    pos = SceneView.lastActiveSceneView.camera.transform.position;
                    pos.z = 0;
                }
                pos += new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);

                var go = new GameObject($"Sammelobjekt_{collectibles.Length + 1}");
                go.transform.position = pos;
                WorkshopShapes.AddRenderer(go).color = new Color(1f, 0.9f, 0.2f);
                var col = go.AddComponent<CircleCollider2D>();
                col.radius = 0.4f;
                col.isTrigger = true;
                go.AddComponent<Collectible>();

                var container = GameObject.Find("--- SAMMELOBJEKTE ---");
                if (container != null) go.transform.SetParent(container.transform);

                Undo.RegisterCreatedObjectUndo(go, "Sammelobjekt hinzugefügt");
                Selection.activeGameObject = go;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // Liste aller Sammelobjekte
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < collectibles.Length; i++)
            {
                var c = collectibles[i];
                if (c == null) continue;

                EditorGUILayout.BeginHorizontal("box");

                // Name
                EditorGUILayout.LabelField($"  {i + 1}. {c.gameObject.name}",
                    GUILayout.Width(180));

                // Typ
                EditorGUILayout.LabelField(c.collectibleType.ToString(),
                    EditorStyles.miniLabel, GUILayout.Width(60));

                // Sprite-Status
                var sr = c.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null)
                {
                    EditorGUILayout.LabelField("⚠ Kein Bild!",
                        new GUIStyle(EditorStyles.miniLabel) {
                            normal = { textColor = Color.yellow }
                        }, GUILayout.Width(70));
                }

                // Auswählen-Button
                if (GUILayout.Button("→", GUILayout.Width(25)))
                {
                    Selection.activeGameObject = c.gameObject;
                    if (SceneView.lastActiveSceneView != null)
                        SceneView.lastActiveSceneView.FrameSelected();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // Auto-refresh
        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}

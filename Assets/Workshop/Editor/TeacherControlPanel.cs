using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Workshop.EditorTools
{
    /// <summary>
    /// TEACHER CONTROL PANEL
    /// Schnellzugriff-Panel für Betreuer.
    /// Buttons für häufige Aktionen: Szene zurücksetzen, duplizieren, testen.
    /// </summary>
    public class TeacherControlPanel : EditorWindow
    {
        [MenuItem("Workshop/Betreuer-Panel", false, 30)]
        public static void ShowWindow()
        {
            var window = GetWindow<TeacherControlPanel>("Betreuer-Panel");
            window.minSize = new Vector2(300, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("🎓 Betreuer-Panel", titleStyle);
            EditorGUILayout.Space(10);

            // --- Schnellaktionen ---
            EditorGUILayout.LabelField("Schnellaktionen", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            if (GUILayout.Button("▶ Spiel testen (Play)", GUILayout.Height(30)))
            {
                EditorApplication.isPlaying = !EditorApplication.isPlaying;
            }

            EditorGUILayout.Space(3);

            if (GUILayout.Button("🔄 Level im Spiel zurücksetzen", GUILayout.Height(25)))
            {
                if (Application.isPlaying)
                    MissionManager.Instance?.ResetMission();
                else
                    Debug.Log("[Workshop] Funktioniert nur im Play-Mode!");
            }

            if (GUILayout.Button("Tilemap-Ebenen vorbereiten", GUILayout.Height(25)))
            {
                TilemapLayerSetup.PrepareDefaultLayers();
            }

            EditorGUILayout.Space(10);

            // --- Szenen ---
            EditorGUILayout.LabelField("Szenen-Verwaltung", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            if (GUILayout.Button("📄 Aktuelle Szene duplizieren", GUILayout.Height(25)))
            {
                DuplicateCurrentScene();
            }

            if (GUILayout.Button("💾 Szene speichern", GUILayout.Height(25)))
            {
                EditorSceneManager.SaveOpenScenes();
                Debug.Log("[Workshop] Szene gespeichert!");
            }

            EditorGUILayout.Space(10);

            // --- Workshop-Szenen öffnen ---
            EditorGUILayout.LabelField("Workshop-Szenen", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            DrawSceneButton("Workshop_Template");
            DrawSceneButton("Workshop_Extended");

            EditorGUILayout.Space(10);

            // --- Erweiterungen verwalten ---
            EditorGUILayout.LabelField("Erweiterungen verwalten", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Workshop dauert doch kürzer? Hier kannst du Extras " +
                "schnell entfernen, ohne die Grundmission kaputt zu machen.",
                MessageType.None);
            EditorGUILayout.Space(3);

            // Schalter/Tür Status prüfen
            bool hasSwitches = Object.FindAnyObjectByType<DoorSwitch>() != null;
            bool hasCheckpoints = Object.FindAnyObjectByType<Checkpoint>() != null;
            var erweiterungenContainer = GameObject.Find("--- ERWEITERUNGEN ---");

            if (hasSwitches || hasCheckpoints)
            {
                if (hasSwitches)
                {
                    GUI.backgroundColor = new Color(1f, 0.7f, 0.3f);
                    if (GUILayout.Button("Alle Schalter & Türen entfernen", GUILayout.Height(25)))
                    {
                        if (EditorUtility.DisplayDialog("Schalter & Türen entfernen?",
                            "Alle DoorSwitch-Objekte (Schalter und Türen) werden gelöscht. " +
                            "Die restliche Mission bleibt intakt.\n\n" +
                            "Das kann mit Strg+Z rückgängig gemacht werden.",
                            "Ja, entfernen", "Abbrechen"))
                        {
                            var allDoors = Object.FindObjectsByType<DoorSwitch>(FindObjectsSortMode.None);
                            foreach (var d in allDoors)
                                Undo.DestroyObjectImmediate(d.gameObject);
                            Debug.Log($"[Workshop] {allDoors.Length} Schalter/Türen entfernt.");
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (hasCheckpoints)
                {
                    GUI.backgroundColor = new Color(1f, 0.7f, 0.3f);
                    if (GUILayout.Button("Alle Checkpoints entfernen", GUILayout.Height(25)))
                    {
                        if (EditorUtility.DisplayDialog("Checkpoints entfernen?",
                            "Alle Checkpoint-Objekte werden gelöscht.\n\n" +
                            "Das kann mit Strg+Z rückgängig gemacht werden.",
                            "Ja, entfernen", "Abbrechen"))
                        {
                            var allCP = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
                            foreach (var cp in allCP)
                                Undo.DestroyObjectImmediate(cp.gameObject);
                            Debug.Log($"[Workshop] {allCP.Length} Checkpoints entfernt.");
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }

                // Leeren Erweiterungen-Container aufräumen
                if (erweiterungenContainer != null && erweiterungenContainer.transform.childCount == 0)
                {
                    Undo.DestroyObjectImmediate(erweiterungenContainer);
                }
            }
            else
            {
                EditorGUILayout.LabelField("  Keine Erweiterungen in der Szene.", EditorStyles.miniLabel);
                EditorGUILayout.Space(3);

                // Erweiterungen nachträglich hinzufügen
                if (GUILayout.Button("+ Schalter & Tür hinzufügen"))
                {
                    var parent = GameObject.Find("--- ERWEITERUNGEN ---");
                    if (parent == null)
                    {
                        parent = new GameObject("--- ERWEITERUNGEN ---");
                        Undo.RegisterCreatedObjectUndo(parent, "Erweiterungen-Container");
                    }

                    var switchGO = new GameObject("Schalter");
                    switchGO.transform.position = new Vector3(-3, 4, 0);
                    switchGO.transform.SetParent(parent.transform);
                    WorkshopShapes.AddRenderer(switchGO).color = Color.yellow;
                    var swCol = switchGO.AddComponent<BoxCollider2D>();
                    swCol.size = new Vector2(0.8f, 0.8f);
                    swCol.isTrigger = true;
                    var sw = switchGO.AddComponent<DoorSwitch>();
                    sw.switchType = SwitchType.Switch;

                    var doorGO = new GameObject("Tür");
                    doorGO.transform.position = new Vector3(0, 6, 0);
                    doorGO.transform.SetParent(parent.transform);
                    WorkshopShapes.AddRenderer(doorGO).color = new Color(0.6f, 0.3f, 0f);
                    doorGO.AddComponent<BoxCollider2D>().size = new Vector2(2f, 0.5f);
                    var door = doorGO.AddComponent<DoorSwitch>();
                    door.switchType = SwitchType.Door;

                    sw.connectedDoors.Add(door);

                    Undo.RegisterCreatedObjectUndo(switchGO, "Schalter");
                    Undo.RegisterCreatedObjectUndo(doorGO, "Tür");
                    Selection.activeGameObject = switchGO;
                }

                if (GUILayout.Button("+ Checkpoint hinzufügen"))
                {
                    var cpGO = new GameObject("Checkpoint");
                    cpGO.transform.position = new Vector3(0, 4, 0);
                    WorkshopShapes.AddRenderer(cpGO).color = Color.cyan;
                    var cpCol = cpGO.AddComponent<CircleCollider2D>();
                    cpCol.radius = 0.5f;
                    cpCol.isTrigger = true;
                    cpGO.AddComponent<Checkpoint>();
                    Undo.RegisterCreatedObjectUndo(cpGO, "Checkpoint");
                    Selection.activeGameObject = cpGO;
                }
            }

            EditorGUILayout.Space(10);

            // --- Werkzeuge öffnen ---
            EditorGUILayout.LabelField("Werkzeuge öffnen", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            if (GUILayout.Button("Mission Setup Wizard"))
                MissionSetupWizard.ShowWindow();

            if (GUILayout.Button("Level prüfen"))
                LevelValidator.ShowWindow();

            if (GUILayout.Button("Theme-Wechsler"))
                ThemeApplier.ShowWindow();

            if (GUILayout.Button("Kinder-Panel"))
                WorkshopBuilderPanel.ShowWindow();

            EditorGUILayout.Space(10);

            // --- Screenshots ---
            EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            if (GUILayout.Button("📸 Screenshot erstellen", GUILayout.Height(25)))
            {
                TakeScreenshot();
            }

            EditorGUILayout.Space(5);

            // --- Info ---
            EditorGUILayout.HelpBox(
                "Tipp: Nutze den Mission Setup Wizard, um schnell " +
                "neue Missionen für einzelne Kinder oder Gruppen anzulegen.",
                MessageType.Info);
        }

        private void DrawSceneButton(string sceneName)
        {
            string path = $"Assets/Workshop/Scenes/{sceneName}.unity";
            bool exists = System.IO.File.Exists(
                System.IO.Path.Combine(Application.dataPath, $"Workshop/Scenes/{sceneName}.unity"));

            GUI.enabled = exists;
            if (GUILayout.Button(sceneName + (exists ? "" : " (nicht vorhanden)")))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
            GUI.enabled = true;
        }

        private void DuplicateCurrentScene()
        {
            var scene = SceneManager.GetActiveScene();
            string currentPath = scene.path;

            if (string.IsNullOrEmpty(currentPath))
            {
                EditorUtility.DisplayDialog("Fehler",
                    "Die Szene muss erst gespeichert werden!", "OK");
                return;
            }

            string directory = System.IO.Path.GetDirectoryName(currentPath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(currentPath);
            string extension = System.IO.Path.GetExtension(currentPath);

            string newPath = $"{directory}/{fileName}_Kopie{extension}";
            int counter = 1;
            while (System.IO.File.Exists(newPath))
            {
                newPath = $"{directory}/{fileName}_Kopie{counter}{extension}";
                counter++;
            }

            // Erst speichern
            EditorSceneManager.SaveScene(scene);

            // Dann duplizieren
            AssetDatabase.CopyAsset(currentPath, newPath);
            AssetDatabase.Refresh();

            Debug.Log($"[Workshop] Szene dupliziert: {newPath}");
            EditorUtility.DisplayDialog("Szene dupliziert!",
                $"Kopie erstellt:\n{newPath}", "OK");
        }

        private void TakeScreenshot()
        {
            string folder = "Assets/Workshop/Screenshots";
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string missionName = "screenshot";

            var manager = Object.FindAnyObjectByType<MissionManager>();
            if (manager != null)
                missionName = manager.missionName.Replace(" ", "_");

            string path = $"{folder}/{missionName}_{timestamp}.png";
            ScreenCapture.CaptureScreenshot(path);

            Debug.Log($"[Workshop] Screenshot gespeichert: {path}");
            EditorUtility.DisplayDialog("Screenshot!",
                $"Gespeichert unter:\n{path}", "OK");
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Workshop.EditorTools
{
    /// <summary>
    /// WORKSHOP BUILDER PANEL — Das Haupt-Werkzeug für Kinder!
    ///
    /// Vereinfachte Oberfläche mit großen, klaren Buttons.
    /// Kinder können hier:
    /// - Sammelobjekte hinzufügen
    /// - Gefahren platzieren
    /// - Deko setzen
    /// - Ihre Mission benennen
    /// - Die Checkliste sehen
    /// </summary>
    public class WorkshopBuilderPanel : EditorWindow
    {
        private string editMissionName = "";
        private string editMissionDesc = "";
        private Vector2 scrollPos;
        private int selectedTab = 0;
        private readonly string[] tabs = { "Bauen", "Checkliste", "Testen" };

        [MenuItem("Workshop/Kinder-Panel (Builder)", false, 50)]
        public static void ShowWindow()
        {
            var window = GetWindow<WorkshopBuilderPanel>("Mein Spiel bauen!");
            window.minSize = new Vector2(350, 600);
        }

        private void OnEnable()
        {
            // Aktuelle Missionsdaten laden
            var manager = FindAnyObjectByType<MissionManager>();
            if (manager != null)
            {
                editMissionName = manager.missionName;
                editMissionDesc = manager.missionDescription;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // Titel
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Mein Spiel bauen!", titleStyle);
            EditorGUILayout.Space(5);

            // Tab-Auswahl
            selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.Height(30));
            EditorGUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            switch (selectedTab)
            {
                case 0: DrawBuildTab(); break;
                case 1: DrawChecklistTab(); break;
                case 2: DrawTestTab(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        // ============================================
        // TAB 1: BAUEN
        // ============================================
        private void DrawBuildTab()
        {
            // --- Missionsname ---
            EditorGUILayout.LabelField("Wie heißt deine Mission?", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            editMissionName = EditorGUILayout.TextField(editMissionName);
            editMissionDesc = EditorGUILayout.TextArea(editMissionDesc, GUILayout.Height(50));

            if (GUILayout.Button("Name speichern", GUILayout.Height(25)))
            {
                var manager = FindAnyObjectByType<MissionManager>();
                if (manager != null)
                {
                    Undo.RecordObject(manager, "Missionsname ändern");
                    manager.missionName = editMissionName;
                    manager.missionDescription = editMissionDesc;
                    EditorUtility.SetDirty(manager);
                }
            }

            EditorGUILayout.Space(15);

            // --- Objekte hinzufügen ---
            EditorGUILayout.LabelField("Was möchtest du hinzufügen?", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Große farbige Buttons
            DrawBigButton("Sammelobjekt", "Fügt ein neues Sammelobjekt hinzu",
                new Color(1f, 0.85f, 0.2f), () => PlaceObject(ObjectType.Collectible));

            DrawBigButton("Gefahr / Monster", "Platziert eine Gefahr",
                new Color(1f, 0.3f, 0.2f), () => PlaceObject(ObjectType.Hazard));

            DrawBigButton("Deko (Baum, Felsen...)", "Setzt ein Deko-Objekt",
                new Color(0.3f, 0.8f, 0.4f), () => PlaceObject(ObjectType.Decor));

            DrawBigButton("Schalter", "Platziert einen Schalter",
                new Color(1f, 0.7f, 0.1f), () => PlaceObject(ObjectType.Switch));

            DrawBigButton("Tür", "Platziert eine Tür (mit Schalter verbinden!)",
                new Color(0.6f, 0.4f, 0.15f), () => PlaceObject(ObjectType.Door));

            EditorGUILayout.Space(10);

            // --- Schnellzugriff ---
            EditorGUILayout.LabelField("Schnell-Aktionen", EditorStyles.boldLabel);

            if (GUILayout.Button("Spieler auswählen", GUILayout.Height(25)))
            {
                var player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                    Selection.activeGameObject = player.gameObject;
            }

            if (GUILayout.Button("Zielzone auswählen", GUILayout.Height(25)))
            {
                var goal = FindAnyObjectByType<GoalZone>();
                if (goal != null)
                    Selection.activeGameObject = goal.gameObject;
            }
        }

        // ============================================
        // TAB 2: CHECKLISTE
        // ============================================
        private void DrawChecklistTab()
        {
            EditorGUILayout.LabelField("Hast du alles?", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            var manager = FindAnyObjectByType<MissionManager>();
            bool hasPlayer = FindAnyObjectByType<PlayerController>() != null;
            bool hasGoal = FindAnyObjectByType<GoalZone>() != null;
            int collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;
            int hazards = FindObjectsByType<Hazard>(FindObjectsSortMode.None).Length;
            bool hasName = manager != null && manager.missionName != "Meine Mission"
                          && !string.IsNullOrEmpty(manager.missionName);

            DrawChecklistItem("Spieler", hasPlayer);
            DrawChecklistItem("Zielzone", hasGoal);
            DrawChecklistItem($"Sammelobjekte ({collectibles})", collectibles >= 3);
            DrawChecklistItem($"Gefahren ({hazards})", hazards >= 1);
            DrawChecklistItem("Mission hat einen Namen", hasName);

            // Bonus-Items
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Bonus (wenn du Zeit hast)", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            bool hasDeco = GameObject.Find("--- DEKO ---")?.transform.childCount > 0;
            bool hasSwitch = FindAnyObjectByType<DoorSwitch>() != null;
            bool hasCheckpoint = FindAnyObjectByType<Checkpoint>() != null;

            DrawChecklistItem("Deko platziert", hasDeco == true, true);
            DrawChecklistItem("Schalter & Tür", hasSwitch, true);
            DrawChecklistItem("Checkpoint", hasCheckpoint, true);

            EditorGUILayout.Space(15);

            // Gesamtstatus
            bool isReady = hasPlayer && hasGoal && collectibles >= 1;
            if (isReady)
            {
                EditorGUILayout.HelpBox(
                    "Super! Dein Spiel ist bereit zum Testen!\n" +
                    "Gehe zum 'Testen'-Tab und drücke Play!",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Es fehlt noch etwas. " +
                    "Schau dir die Liste oben an!",
                    MessageType.Warning);
            }
        }

        // ============================================
        // TAB 3: TESTEN
        // ============================================
        private void DrawTestTab()
        {
            EditorGUILayout.LabelField("Dein Spiel testen!", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Großer Play-Button
            GUI.backgroundColor = Application.isPlaying ?
                new Color(1f, 0.4f, 0.3f) : new Color(0.3f, 0.9f, 0.4f);

            string playText = Application.isPlaying ? "Stopp!" : "Spielen!";
            if (GUILayout.Button(playText, GUILayout.Height(60)))
            {
                EditorApplication.isPlaying = !EditorApplication.isPlaying;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Steuerung:\n" +
                    "Pfeiltasten oder WASD = Bewegen\n" +
                    "R = Neustart\n" +
                    "Escape = Spiel beenden",
                    MessageType.Info);

                EditorGUILayout.Space(5);

                if (GUILayout.Button("Mission zurücksetzen", GUILayout.Height(30)))
                {
                    MissionManager.Instance?.ResetMission();
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Drücke den großen grünen Button, " +
                    "um dein Spiel auszuprobieren!",
                    MessageType.Info);

                EditorGUILayout.Space(5);

                // Level-Prüfung
                if (GUILayout.Button("Level prüfen (vor dem Testen)", GUILayout.Height(30)))
                {
                    LevelValidator.ShowWindow();
                }
            }

            EditorGUILayout.Space(10);

            // Tipps
            EditorGUILayout.LabelField("Tipps", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "• Teste dein Spiel oft! Auch wenn es noch nicht fertig ist.\n" +
                "• Ist es zu leicht? Füge mehr Gefahren hinzu.\n" +
                "• Ist es zu schwer? Mache den Weg breiter oder die Gefahren kleiner.\n" +
                "• Vergiss nicht, deiner Mission einen coolen Namen zu geben!",
                MessageType.None);
        }

        // ============================================
        // HILFSMETHODEN
        // ============================================

        private void DrawBigButton(string label, string tooltip, Color color, System.Action onClick)
        {
            GUI.backgroundColor = color;
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 40
            };

            if (GUILayout.Button(new GUIContent(label, tooltip), style))
            {
                onClick?.Invoke();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawChecklistItem(string label, bool done, bool isBonus = false)
        {
            EditorGUILayout.BeginHorizontal();

            string icon = done ? "☑" : "☐";
            Color color = done ? new Color(0.2f, 0.8f, 0.3f) :
                          (isBonus ? Color.gray : new Color(1f, 0.5f, 0.2f));

            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                normal = { textColor = color }
            };

            EditorGUILayout.LabelField($"  {icon}  {label}", style, GUILayout.Height(25));
            EditorGUILayout.EndHorizontal();
        }

        private enum ObjectType { Collectible, Hazard, Decor, Switch, Door }

        private void PlaceObject(ObjectType type)
        {
            // Position: Mitte des aktuellen SceneView
            Vector3 pos = Vector3.zero;
            if (SceneView.lastActiveSceneView != null)
            {
                pos = SceneView.lastActiveSceneView.camera.transform.position;
                pos.z = 0;
            }

            // Leicht versetzt platzieren (nicht übereinander)
            pos += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);

            GameObject go = null;

            switch (type)
            {
                case ObjectType.Collectible:
                    go = CreateCollectible(pos);
                    break;
                case ObjectType.Hazard:
                    go = CreateHazard(pos);
                    break;
                case ObjectType.Decor:
                    go = CreateDecor(pos);
                    break;
                case ObjectType.Switch:
                    go = CreateSwitch(pos);
                    break;
                case ObjectType.Door:
                    go = CreateDoor(pos);
                    break;
            }

            if (go != null)
            {
                Undo.RegisterCreatedObjectUndo(go, $"{type} erstellt");
                Selection.activeGameObject = go;

                // In Scene View fokussieren
                if (SceneView.lastActiveSceneView != null)
                    SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        private GameObject CreateCollectible(Vector3 pos)
        {
            int count = FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length + 1;
            var go = new GameObject($"Sammelobjekt_{count}");
            go.transform.position = pos;

            var sr = WorkshopShapes.AddRenderer(go);
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.9f, 0.2f);

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;

            go.AddComponent<Collectible>();

            // In Container packen
            var container = GameObject.Find("--- SAMMELOBJEKTE ---");
            if (container != null) go.transform.SetParent(container.transform);

            return go;
        }

        private GameObject CreateHazard(Vector3 pos)
        {
            int count = FindObjectsByType<Hazard>(FindObjectsSortMode.None).Length + 1;
            var go = new GameObject($"Gefahr_{count}");
            go.transform.position = pos;

            var sr = WorkshopShapes.AddRenderer(go);
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.2f, 0.2f, 0.7f);

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);
            collider.isTrigger = true;

            go.AddComponent<Hazard>();

            var container = GameObject.Find("--- GEFAHREN ---");
            if (container != null) go.transform.SetParent(container.transform);

            return go;
        }

        private GameObject CreateDecor(Vector3 pos)
        {
            var go = new GameObject("Deko");
            go.transform.position = pos;

            var sr = WorkshopShapes.AddRenderer(go);
            sr.sortingOrder = 2;
            sr.color = new Color(0.4f, 0.7f, 0.3f);

            var container = GameObject.Find("--- DEKO ---");
            if (container != null) go.transform.SetParent(container.transform);

            return go;
        }

        private GameObject CreateSwitch(Vector3 pos)
        {
            var go = new GameObject("Schalter");
            go.transform.position = pos;

            var sr = WorkshopShapes.AddRenderer(go);
            sr.sortingOrder = 5;
            sr.color = Color.yellow;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.8f);
            col.isTrigger = true;

            var sw = go.AddComponent<DoorSwitch>();
            sw.switchType = SwitchType.Switch;

            return go;
        }

        private GameObject CreateDoor(Vector3 pos)
        {
            var go = new GameObject("Tür");
            go.transform.position = pos;

            var sr = WorkshopShapes.AddRenderer(go);
            sr.sortingOrder = 5;
            sr.color = new Color(0.6f, 0.3f, 0f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 0.5f);

            var door = go.AddComponent<DoorSwitch>();
            door.switchType = SwitchType.Door;

            return go;
        }

    }
}

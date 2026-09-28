using UnityEngine;
using UnityEditor;

namespace Workshop.EditorTools
{
    /// <summary>
    /// LEVEL VALIDATOR
    /// Prüft ob eine Mission spielbar ist und gibt klare Warnungen aus.
    /// Perfekt für Betreuer, um schnell zu sehen ob ein Kind alles hat.
    /// </summary>
    public class LevelValidator : EditorWindow
    {
        private MissionValidationResult lastResult;
        private Vector2 scrollPos;
        private bool noManagerWarning = false;

        [MenuItem("Workshop/Level prüfen", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<LevelValidator>("Level-Prüfung");
            window.minSize = new Vector2(350, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("✅ Level-Prüfung", titleStyle);
            EditorGUILayout.Space(5);

            // Prüfen-Button
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("Level jetzt prüfen!", GUILayout.Height(35)))
            {
                ValidateCurrentScene();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // Warnung ohne MissionManager
            if (noManagerWarning)
            {
                EditorGUILayout.HelpBox(
                    "Kein MissionManager in der Szene! " +
                    "Nutze den Mission Setup Wizard oder füge einen manuell hinzu.",
                    MessageType.Error);
            }

            // Ergebnisse anzeigen
            if (lastResult != null)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                // Gesamtstatus
                if (lastResult.IsPlayable)
                {
                    EditorGUILayout.HelpBox(
                        "Das Level ist SPIELBAR! Alle Grundvoraussetzungen erfüllt.",
                        MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Das Level ist NOCH NICHT SPIELBAR. Siehe Details unten.",
                        MessageType.Warning);
                }

                EditorGUILayout.Space(5);

                // Einzelprüfungen
                DrawCheck("Spieler vorhanden", lastResult.hasPlayer,
                    "Erstelle ein GameObject mit dem PlayerController-Script.");

                DrawCheck("Zielzone vorhanden", lastResult.hasGoal,
                    "Erstelle ein GameObject mit dem GoalZone-Script.");

                DrawCheck($"Sammelobjekte: {lastResult.collectibleCount}",
                    lastResult.hasEnoughCollectibles,
                    "Mindestens 1 Sammelobjekt wird benötigt.");

                // Zusätzliche Infos
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Weitere Details", EditorStyles.boldLabel);

                EditorGUILayout.LabelField($"  Gefahren: {lastResult.hazardCount}");
                DrawCheck("Missionsname gesetzt", lastResult.hasName,
                    "Der Missionsname ist noch auf 'Meine Mission'.");

                // Erweiterte Prüfungen
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Erweiterte Prüfungen", EditorStyles.boldLabel);

                bool hasAudio = Object.FindAnyObjectByType<AudioManager>() != null;
                DrawCheck("Audio Manager", hasAudio,
                    "Kein AudioManager gefunden. Audio funktioniert nicht.");

                bool hasUI = Object.FindAnyObjectByType<WorkshopUI>() != null;
                DrawCheck("Workshop UI", hasUI,
                    "Kein WorkshopUI gefunden. HUD und Win-Screen fehlen.");

                bool hasCam = Object.FindAnyObjectByType<CameraFollow>() != null;
                DrawCheck("Kamera-Follow", hasCam,
                    "Keine CameraFollow-Komponente. Kamera folgt nicht dem Spieler.");

                bool hasTilemapLayers = TilemapLayerSetup.HasPreparedBlockingLayers();
                DrawCheck("Tilemap-Ebenen mit Blockern", hasTilemapLayers,
                    "Nutze Workshop > Tilemap-Ebenen vorbereiten, damit Wasser, Haeuser und Waende eigene Collider-Ebenen haben.");

                // Warnungen für übliche Fehler
                var collectibles = Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None);
                foreach (var c in collectibles)
                {
                    if (c.GetComponent<SpriteRenderer>() == null ||
                        c.GetComponent<SpriteRenderer>().sprite == null)
                    {
                        EditorGUILayout.HelpBox(
                            $"'{c.gameObject.name}' hat kein Sprite! " +
                            "Es wird im Spiel unsichtbar sein.",
                            MessageType.Warning);
                    }
                }

                var player = Object.FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    if (player.GetComponent<SpriteRenderer>()?.sprite == null)
                    {
                        EditorGUILayout.HelpBox(
                            "Der Spieler hat kein Sprite! " +
                            "Weise ein Charakter-Sprite zu.",
                            MessageType.Warning);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void ValidateCurrentScene()
        {
            var manager = Object.FindAnyObjectByType<MissionManager>();
            if (manager != null)
            {
                lastResult = manager.ValidateMission();
                noManagerWarning = false;
            }
            else
            {
                // Ohne MissionManager manuell prüfen
                lastResult = new MissionValidationResult
                {
                    hasPlayer = Object.FindAnyObjectByType<PlayerController>() != null,
                    hasGoal = Object.FindAnyObjectByType<GoalZone>() != null,
                    collectibleCount = Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length,
                    hasEnoughCollectibles = Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length >= 1,
                    hazardCount = Object.FindObjectsByType<Hazard>(FindObjectsSortMode.None).Length,
                    hasName = false
                };

                noManagerWarning = true;
            }

            Repaint();
        }

        private void DrawCheck(string label, bool passed, string failHint)
        {
            EditorGUILayout.BeginHorizontal();
            string icon = passed ? "✓" : "✗";
            Color color = passed ? new Color(0.2f, 0.8f, 0.3f) : new Color(1f, 0.3f, 0.2f);

            GUIStyle style = new GUIStyle(EditorStyles.label) { normal = { textColor = color } };
            EditorGUILayout.LabelField($"  {icon} {label}", style);
            EditorGUILayout.EndHorizontal();

            if (!passed)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"    → {failHint}",
                    new GUIStyle(EditorStyles.miniLabel) { wordWrap = true });
                EditorGUI.indentLevel--;
            }
        }
    }
}

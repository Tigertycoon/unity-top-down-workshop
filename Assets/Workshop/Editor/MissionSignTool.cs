using UnityEngine;
using UnityEditor;

namespace Workshop.EditorTools
{
    /// <summary>
    /// MISSION SIGN / TITLE TOOL
    /// Erstellt ein Schild-GameObject in der Szene mit dem Missionsnamen.
    /// Zeigt den Namen auch auf einem optionalen Startscreen an.
    ///
    /// Kinder können hier ihren Missionsnamen eingeben und
    /// ein Schild im Level platzieren, das den Namen zeigt.
    /// </summary>
    public class MissionSignTool : EditorWindow
    {
        private string missionTitle = "";
        private Color signColor = new Color(0.6f, 0.4f, 0.2f);
        private Color textColor = Color.white;

        [MenuItem("Workshop/Missionsname & Schild", false, 75)]
        public static void ShowWindow()
        {
            var window = GetWindow<MissionSignTool>("Missionsname");
            window.minSize = new Vector2(300, 300);

            var manager = Object.FindAnyObjectByType<MissionManager>();
            if (manager != null)
                window.missionTitle = manager.missionName;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("📝 Dein Missionsname", titleStyle);
            EditorGUILayout.Space(10);

            // Name eingeben
            EditorGUILayout.LabelField("Wie heißt deine Mission?", EditorStyles.boldLabel);
            missionTitle = EditorGUILayout.TextField(missionTitle);

            EditorGUILayout.Space(5);

            // Vorschläge
            EditorGUILayout.LabelField("Ideen:", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Kristallsuche")) missionTitle = "Die große Kristallsuche";
            if (GUILayout.Button("Rettung")) missionTitle = "Rettungsmission Alpha";
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Expedition")) missionTitle = "Monster-Expedition";
            if (GUILayout.Button("Geheimnis")) missionTitle = "Das Geheimnis der Ruine";
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Labor")) missionTitle = "Flucht aus dem Labor";
            if (GUILayout.Button("Artefakte")) missionTitle = "Die verlorenen Artefakte";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Farben
            signColor = EditorGUILayout.ColorField("Schildfarbe", signColor);
            textColor = EditorGUILayout.ColorField("Textfarbe", textColor);

            EditorGUILayout.Space(10);

            // Name überall setzen
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("Namen überall aktualisieren!", GUILayout.Height(35)))
            {
                ApplyMissionName();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            // Hinweis
            EditorGUILayout.HelpBox(
                "Der Name erscheint im Spiel oben links " +
                "und beim Start der Mission als großer Titel.",
                MessageType.Info);
        }

        private void ApplyMissionName()
        {
            if (string.IsNullOrEmpty(missionTitle))
            {
                EditorUtility.DisplayDialog("Kein Name!",
                    "Gib deiner Mission erst einen Namen!", "OK");
                return;
            }

            // MissionManager aktualisieren
            var manager = Object.FindAnyObjectByType<MissionManager>();
            if (manager != null)
            {
                Undo.RecordObject(manager, "Missionsname ändern");
                manager.missionName = missionTitle;
                EditorUtility.SetDirty(manager);
            }

            Debug.Log($"[Workshop] Missionsname gesetzt: '{missionTitle}'");

            EditorUtility.DisplayDialog("Name gesetzt!",
                $"Deine Mission heißt jetzt:\n\n\"{missionTitle}\"\n\n" +
                "Der Name wird im Spiel angezeigt!",
                "Cool!");
        }
    }
}

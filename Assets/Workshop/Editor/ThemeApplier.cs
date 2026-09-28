using UnityEngine;
using UnityEditor;
using System.IO;

namespace Workshop.EditorTools
{
    /// <summary>
    /// THEME / PALETTE APPLIER
    /// Wechselt mit einem Klick das Thema der aktuellen Szene.
    /// Zeigt alle verfügbaren Themes als Karten mit Vorschau.
    /// </summary>
    public class ThemeApplier : EditorWindow
    {
        private ThemeData[] availableThemes;
        private Vector2 scrollPos;
        private ThemeData currentTheme;

        [MenuItem("Workshop/Theme-Wechsler", false, 40)]
        public static void ShowWindow()
        {
            var window = GetWindow<ThemeApplier>("Theme-Wechsler");
            window.minSize = new Vector2(350, 400);
            window.RefreshThemes();
        }

        private void OnEnable()
        {
            RefreshThemes();
        }

        private void RefreshThemes()
        {
            // Alle ThemeData-Assets finden
            string[] guids = AssetDatabase.FindAssets("t:ThemeData");
            availableThemes = new ThemeData[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                availableThemes[i] = AssetDatabase.LoadAssetAtPath<ThemeData>(path);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("🎨 Theme-Wechsler", titleStyle);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Themes neu laden"))
                RefreshThemes();

            EditorGUILayout.Space(10);

            if (availableThemes == null || availableThemes.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Keine Themes gefunden!\n\n" +
                    "Erstelle Theme-Assets über:\n" +
                    "Rechtsklick → Create → Workshop → Theme",
                    MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var theme in availableThemes)
            {
                if (theme == null) continue;
                DrawThemeCard(theme);
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawThemeCard(ThemeData theme)
        {
            bool isCurrent = theme == currentTheme;

            // Hintergrund
            Color bgColor = isCurrent ? new Color(0.3f, 0.6f, 0.3f, 0.3f) : new Color(0.25f, 0.25f, 0.25f);
            EditorGUILayout.BeginVertical(new GUIStyle("box"));

            // Theme-Name und Beschreibung
            EditorGUILayout.BeginHorizontal();

            // Farbvorschau
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(30, 30), theme.primaryColor);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(30, 30), theme.secondaryColor);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(30, 30), theme.accentColor);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(theme.themeName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(theme.description, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // Details
            EditorGUILayout.LabelField(
                $"  Musik: {(theme.backgroundMusic != null ? theme.backgroundMusic.name : "keine")} | " +
                $"Deko: {(theme.decorPrefabs != null ? theme.decorPrefabs.Length : 0)} | " +
                $"Gefahren: {(theme.hazardPrefabs != null ? theme.hazardPrefabs.Length : 0)}",
                EditorStyles.miniLabel);

            // Anwenden-Button
            EditorGUILayout.BeginHorizontal();
            if (isCurrent)
            {
                GUI.enabled = false;
                GUILayout.Button("✓ Aktiv", GUILayout.Height(25));
                GUI.enabled = true;
            }
            else
            {
                GUI.backgroundColor = theme.primaryColor;
                if (GUILayout.Button($"Theme anwenden", GUILayout.Height(25)))
                {
                    theme.Apply();
                    currentTheme = theme;
                    Debug.Log($"[Workshop] Theme '{theme.themeName}' angewendet!");
                }
                GUI.backgroundColor = Color.white;
            }

            // Bearbeiten
            if (GUILayout.Button("Bearbeiten", GUILayout.Width(80), GUILayout.Height(25)))
            {
                Selection.activeObject = theme;
                EditorGUIUtility.PingObject(theme);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}

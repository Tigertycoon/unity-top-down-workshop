using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Workshop.EditorTools
{
    /// <summary>
    /// AUDIO PREVIEW PANEL
    /// Lässt Kinder Musik und Soundeffekte vorhören und auswählen.
    /// Durchsucht den Audio-Ordner und zeigt alle Clips als Buttons an.
    /// </summary>
    public class AudioPreviewPanel : EditorWindow
    {
        private Vector2 scrollPos;
        private AudioClip previewClip;
        private List<AudioClip> musicClips = new List<AudioClip>();
        private List<AudioClip> sfxClips = new List<AudioClip>();

        [MenuItem("Workshop/Audio vorhören", false, 70)]
        public static void ShowWindow()
        {
            var window = GetWindow<AudioPreviewPanel>("Audio vorhören");
            window.minSize = new Vector2(300, 400);
            window.RefreshClips();
        }

        private void OnEnable()
        {
            RefreshClips();
        }

        private void RefreshClips()
        {
            musicClips.Clear();
            sfxClips.Clear();

            // Audio aus Workshop/Audio/ laden
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] {
                "Assets/Workshop/Audio",
                "Assets/Audio"
            });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;

                if (path.Contains("Music") || path.Contains("Musik"))
                    musicClips.Add(clip);
                else
                    sfxClips.Add(clip);
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
            EditorGUILayout.LabelField("🔊 Audio vorhören", titleStyle);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Audio-Dateien neu laden"))
                RefreshClips();

            EditorGUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Musik
            EditorGUILayout.LabelField("Musik", EditorStyles.boldLabel);
            if (musicClips.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Keine Musik gefunden.\n" +
                    "Lege Audio-Dateien in Assets/Workshop/Audio/Music/ ab.",
                    MessageType.Info);
            }
            else
            {
                foreach (var clip in musicClips)
                {
                    DrawAudioClipEntry(clip, true);
                }
            }

            EditorGUILayout.Space(10);

            // SFX
            EditorGUILayout.LabelField("Soundeffekte", EditorStyles.boldLabel);
            if (sfxClips.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Keine SFX gefunden.\n" +
                    "Lege Audio-Dateien in Assets/Workshop/Audio/SFX/ ab.",
                    MessageType.Info);
            }
            else
            {
                foreach (var clip in sfxClips)
                {
                    DrawAudioClipEntry(clip, false);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(5);

            // Stopp-Button
            if (GUILayout.Button("Alles stoppen", GUILayout.Height(25)))
            {
                StopPreview();
            }
        }

        private void DrawAudioClipEntry(AudioClip clip, bool isMusic)
        {
            EditorGUILayout.BeginHorizontal("box");

            // Name und Länge
            string duration = $"{clip.length:F1}s";
            EditorGUILayout.LabelField($"  {clip.name} ({duration})", GUILayout.MinWidth(150));

            // Vorhören
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("▶", GUILayout.Width(30)))
            {
                PlayPreview(clip);
            }
            GUI.backgroundColor = Color.white;

            // Als Musik/SFX setzen
            if (isMusic)
            {
                if (GUILayout.Button("Als Musik", GUILayout.Width(70)))
                {
                    var audioManager = Object.FindAnyObjectByType<AudioManager>();
                    if (audioManager != null)
                    {
                        Undo.RecordObject(audioManager, "Musik ändern");
                        audioManager.backgroundMusic = clip;
                        EditorUtility.SetDirty(audioManager);
                        Debug.Log($"[Workshop] Musik auf '{clip.name}' gesetzt!");
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void PlayPreview(AudioClip clip)
        {
            StopPreview();

            // Unity Editor Audio-Preview nutzen
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            if (audioUtilClass != null)
            {
                var playClipMethod = audioUtilClass.GetMethod("PlayPreviewClip",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                    null,
                    new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                    null);

                if (playClipMethod != null)
                {
                    playClipMethod.Invoke(null, new object[] { clip, 0, false });
                    previewClip = clip;
                }
            }
        }

        private void StopPreview()
        {
            var unityEditorAssembly = typeof(AudioImporter).Assembly;
            var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            if (audioUtilClass != null)
            {
                var stopMethod = audioUtilClass.GetMethod("StopAllPreviewClips",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                stopMethod?.Invoke(null, null);
            }

            previewClip = null;
        }

        private void OnDisable()
        {
            StopPreview();
        }
    }
}

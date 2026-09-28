using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Workshop.EditorTools
{
    public static class StarterProject
    {
        [MenuItem("Workshop/Startszene öffnen", false, 0)]
        public static void OpenStarter()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene("Assets/Workshop/Scenes/Workshop_Template.unity");
        }

        // Batch entrypoint for regenerating the two checked-in starter scenes.
        public static void Generate()
        {
            Directory.CreateDirectory("Assets/Workshop/Scenes");
            for (var index = 0; index < 2; index++)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                MissionSetupWizard.CreateStarterMission(index == 1);
                var bounds = new GameObject("LevelBounds", typeof(BoxCollider2D), typeof(LevelBounds));
                var collider = bounds.GetComponent<BoxCollider2D>();
                collider.size = new Vector2(24, 24);
                collider.isTrigger = true;
                var player = Object.FindAnyObjectByType<PlayerController>();
                player.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.65f, 1f);
                var name = index == 0 ? "Workshop_Template" : "Workshop_Extended";
                EditorSceneManager.SaveScene(scene, $"Assets/Workshop/Scenes/{name}.unity");
            }
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Workshop/Scenes/Workshop_Template.unity", true),
                new EditorBuildSettingsScene("Assets/Workshop/Scenes/Workshop_Extended.unity", true)
            };
            EditorSceneManager.OpenScene("Assets/Workshop/Scenes/Workshop_Template.unity");
            AssetDatabase.SaveAssets();
            if (Application.isBatchMode)
            {
                // Leave no selected scene objects for native shutdown after batch generation.
                Selection.objects = new Object[0];
                Undo.ClearAll();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            Debug.Log("WORKSHOP_STARTERS_READY");
        }
    }
}

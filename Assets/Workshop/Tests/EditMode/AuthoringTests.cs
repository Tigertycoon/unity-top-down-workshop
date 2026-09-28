using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Workshop.EditorTools;
using Object = UnityEngine.Object;

namespace Workshop.Tests
{
    public class AuthoringTests
    {
        [SetUp]
        public void Setup() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        [TearDown]
        public void Teardown() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        [Test]
        public void WizardCreatesPlayableVisibleObjectsWithoutAnArtPack()
        {
            MissionSetupWizard.CreateStarterMission(true);
            var mission = Object.FindAnyObjectByType<MissionManager>();
            Assert.That(mission.ValidateMission().IsPlayable, Is.True);
            Assert.That(Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None), Has.Length.EqualTo(5));
            Assert.That(Object.FindObjectsByType<DoorSwitch>(FindObjectsSortMode.None), Has.Length.EqualTo(2));
            Assert.That(Object.FindAnyObjectByType<Checkpoint>(), Is.Not.Null);
            Assert.That(Camera.main.orthographic, Is.True);
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            foreach (var renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                Assert.That(renderer.sprite, Is.Not.Null, renderer.name);
        }

        [Test]
        public void WizardRefusesToDuplicateAnExistingMission()
        {
            MissionSetupWizard.CreateStarterMission();
            Assert.Throws<InvalidOperationException>(() => MissionSetupWizard.CreateStarterMission());
            Assert.That(Object.FindObjectsByType<MissionManager>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        [Test]
        public void TilemapSetupIsIdempotentAndSeparatesHazardsFromBlockers()
        {
            TilemapLayerSetup.PrepareDefaultLayers(false);
            TilemapLayerSetup.PrepareDefaultLayers(false);
            Assert.That(Object.FindObjectsByType<Grid>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None), Has.Length.EqualTo(6));
            Assert.That(TilemapLayerSetup.HasPreparedBlockingLayers(), Is.True);
            var hazard = Object.FindAnyObjectByType<Hazard>();
            Assert.That(hazard.mode, Is.EqualTo(HazardMode.Reset));
            Assert.That(hazard.GetComponent<Collider2D>().isTrigger, Is.True);
        }

        [TestCase("CreateCollectible")]
        [TestCase("CreateHazard")]
        [TestCase("CreateDecor")]
        [TestCase("CreateSwitch")]
        [TestCase("CreateDoor")]
        public void BuilderCreatesVisibleObjects(string method)
        {
            var panel = ScriptableObject.CreateInstance<WorkshopBuilderPanel>();
            try
            {
                var create = typeof(WorkshopBuilderPanel).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
                var go = (GameObject)create.Invoke(panel, new object[] { Vector3.zero });
                Assert.That(go.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
            }
            finally { Object.DestroyImmediate(panel); }
        }

        [TestCase("Workshop_Template")]
        [TestCase("Workshop_Extended")]
        public void SavedStarterHasNoMissingScriptsOrVendorDependencies(string name)
        {
            var path = $"Assets/Workshop/Scenes/{name}.unity";
            var scene = EditorSceneManager.OpenScene(path);
            foreach (var root in scene.GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero);
            Assert.That(Object.FindAnyObjectByType<MissionManager>().ValidateMission().IsPlayable, Is.True);
            var dependencies = AssetDatabase.GetDependencies(path, true);
            Assert.That(dependencies.Any(p => p.Contains("/Gif/") || p.Contains("/RPGPowerForge/")), Is.False);
        }
    }
}

using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Workshop.Tests
{
    public class MissionFlowTests
    {
        private Scene scene;
        private MissionManager mission;
        private PlayerController player;
        private Collectible key;
        private Collectible item;
        private GoalZone goal;
        private DoorSwitch door;
        private DoorSwitch trigger;
        private Checkpoint checkpoint;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            scene = SceneManager.CreateScene("MissionTest");
            SceneManager.SetActiveScene(scene);
            mission = new GameObject("Mission").AddComponent<MissionManager>();
            mission.showIntro = false;
            var playerGO = new GameObject("Player", typeof(BoxCollider2D), typeof(Rigidbody2D));
            player = playerGO.AddComponent<PlayerController>();
            key = Make<Collectible>("Key", new Vector2(3, 0));
            key.bobbing = false;
            key.value = 2;
            key.collectibleType = CollectibleType.Key;
            item = Make<Collectible>("Item", new Vector2(6, 0));
            item.bobbing = false;
            goal = Make<GoalZone>("Goal", new Vector2(10, 0));
            door = Make<DoorSwitch>("Door", new Vector2(9, 3));
            door.switchType = SwitchType.Door;
            trigger = Make<DoorSwitch>("Switch", new Vector2(6, 3));
            trigger.requiresKey = true;
            trigger.connectedDoors.Add(door);
            checkpoint = Make<Checkpoint>("Checkpoint", new Vector2(3, 3));
            yield return null;
            yield return new WaitForFixedUpdate();
        }

        private static T Make<T>(string name, Vector2 position) where T : Component
        {
            var go = new GameObject(name, typeof(BoxCollider2D));
            go.transform.position = position;
            return go.AddComponent<T>();
        }

        private IEnumerator MoveTo(Vector2 position)
        {
            player.GetComponent<Rigidbody2D>().position = position;
            Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator WeightedItemsCannotCompleteMissionEarlyOrCountTwice()
        {
            Assert.That(mission.TotalCollectibles, Is.EqualTo(3));
            yield return MoveTo(key.transform.position);
            Assert.That(key.gameObject.activeSelf, Is.False);
            Assert.That(mission.CollectedCount, Is.EqualTo(2));
            mission.CollectItem(key);
            Assert.That(mission.CollectedCount, Is.EqualTo(2));
            yield return MoveTo(goal.transform.position);
            Assert.That(mission.MissionCompleted, Is.False);
            yield return MoveTo(item.transform.position);
            yield return MoveTo(goal.transform.position);
            Assert.That(mission.MissionCompleted, Is.True);
            Assert.That(player.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator CollectedInactiveKeyUnlocksDoorAndResetRestoresTheWholeMission()
        {
            yield return MoveTo(trigger.transform.position);
            Assert.That(door.IsActivated, Is.False);
            yield return MoveTo(key.transform.position);
            yield return MoveTo(trigger.transform.position);
            Assert.That(door.IsActivated, Is.True);
            yield return MoveTo(checkpoint.transform.position);
            player.ResetToStart();
            Assert.That(Vector3.Distance(player.transform.position, checkpoint.transform.position), Is.LessThan(0.01));
            mission.ResetMission();
            Assert.That(mission.CollectedCount, Is.Zero);
            Assert.That(key.gameObject.activeSelf, Is.True);
            Assert.That(door.IsActivated, Is.False);
            Assert.That(door.GetComponent<Collider2D>().enabled, Is.True);
            Assert.That(mission.HasCollectedType(CollectibleType.Key), Is.False);
            Assert.That(player.transform.position.sqrMagnitude, Is.LessThan(0.001));
            yield return MoveTo(Vector2.zero);
            yield return MoveTo(checkpoint.transform.position);
            player.ResetToStart();
            Assert.That(Vector3.Distance(player.transform.position, checkpoint.transform.position), Is.LessThan(0.01));
        }

        [UnityTest]
        public IEnumerator GoalColliderUsesConfiguredSizeOnce()
        {
            Physics2D.SyncTransforms();
            Assert.That(goal.GetComponent<BoxCollider2D>().bounds.size.x, Is.EqualTo(goal.zoneSize.x).Within(0.001));
            Assert.That(goal.GetComponent<BoxCollider2D>().bounds.size.y, Is.EqualTo(goal.zoneSize.y).Within(0.001));
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisablingPlayerStopsKnockbackVelocity()
        {
            player.TakeDamage(Vector2.left);
            Assert.That(player.GetComponent<Rigidbody2D>().linearVelocity.sqrMagnitude, Is.GreaterThan(0));
            mission.CompleteMission();
            Assert.That(player.GetComponent<Rigidbody2D>().linearVelocity, Is.EqualTo(Vector2.zero));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SavedExtendedSceneStartsCompletesAndRestarts()
        {
            yield return SceneManager.LoadSceneAsync("Workshop_Extended", LoadSceneMode.Single);
            yield return null;
            mission = MissionManager.Instance;
            player = PlayerController.Instance;
            Assert.That(WorkshopUI.Instance, Is.Not.Null);
            Assert.That(AudioManager.Instance, Is.Not.Null);
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None).All(t => t.font != null), Is.True);
            var items = Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None);
            Assert.That(items.Length, Is.EqualTo(5));
            foreach (var collectible in items) yield return MoveTo(collectible.transform.position);
            goal = Object.FindAnyObjectByType<GoalZone>();
            yield return MoveTo(goal.transform.position);
            Assert.That(mission.MissionCompleted, Is.True);
            mission.ResetMission();
            Assert.That(mission.MissionCompleted, Is.False);
            Assert.That(player.enabled, Is.True);
            Assert.That(items.All(c => c.gameObject.activeSelf), Is.True);
            scene = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("EmptyAfterStarter"));
        }
    }
}

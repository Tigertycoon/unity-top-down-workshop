using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Workshop
{
    /// <summary>
    /// Zentrale Steuerung einer Mission. Verwaltet:
    /// - Gesammelte Objekte
    /// - Missionsstatus (läuft, gewonnen)
    /// - Events für UI und Audio
    ///
    /// Singleton — es gibt immer nur einen MissionManager pro Szene.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        [Header("Mission")]
        [Tooltip("Name der Mission (wird im UI angezeigt)")]
        public string missionName = "Meine Mission";

        [Tooltip("Kurze Beschreibung / Auftrag")]
        [TextArea(2, 4)]
        public string missionDescription = "Sammle alle Kristalle und erreiche das Ziel!";

        [Header("Einstellungen")]
        [Tooltip("Zeigt ein Intro am Start der Mission")]
        public bool showIntro = true;

        [Tooltip("Zeit bis das Intro automatisch verschwindet")]
        public float introDuration = 3f;

        [Header("Events")]
        public UnityEvent onItemCollected;
        public UnityEvent onMissionComplete;
        public UnityEvent onPlayerDamaged;

        // --- Status ---
        public int CollectedCount { get; private set; } = 0;
        public int TotalCollectibles { get; private set; } = 0;
        public bool MissionCompleted { get; private set; } = false;

        // Alle Collectibles in der Szene
        private List<Collectible> allCollectibles = new List<Collectible>();

        private readonly HashSet<Collectible> collectedItems = new HashSet<Collectible>();

        public bool HasCollectedType(CollectibleType type) =>
            System.Linq.Enumerable.Any(collectedItems, item => item != null && item.collectibleType == type);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Alle Sammelobjekte in der Szene finden
            allCollectibles.AddRange(FindObjectsByType<Collectible>(FindObjectsSortMode.None));
            TotalCollectibles = 0;
            foreach (var item in allCollectibles) TotalCollectibles += Mathf.Max(1, item.value);
            CollectedCount = 0;
            MissionCompleted = false;

            // Intro anzeigen
            if (showIntro)
            {
                WorkshopUI.Instance?.ShowMissionIntro(missionName, missionDescription, introDuration);
            }

            // UI aktualisieren
            WorkshopUI.Instance?.UpdateCollectibleCounter(CollectedCount, TotalCollectibles);
        }

        /// <summary>Wird von Collectible aufgerufen wenn eingesammelt</summary>
        public void CollectItem(Collectible item)
        {
            if (MissionCompleted || item == null || !allCollectibles.Contains(item) || !collectedItems.Add(item)) return;
            CollectedCount += Mathf.Max(1, item.value);
            onItemCollected?.Invoke();
            WorkshopUI.Instance?.UpdateCollectibleCounter(CollectedCount, TotalCollectibles);

            // Alle gesammelt? Hinweis anzeigen
            if (CollectedCount >= TotalCollectibles)
            {
                WorkshopUI.Instance?.ShowHint("Alle gesammelt! Finde das Ziel!");
                AudioManager.Instance?.PlaySFX("AllCollected");
            }
        }

        /// <summary>Mission abgeschlossen!</summary>
        public void CompleteMission()
        {
            if (MissionCompleted) return;
            MissionCompleted = true;

            onMissionComplete?.Invoke();
            AudioManager.Instance?.PlaySFX("Win");
            AudioManager.Instance?.StopMusic();

            WorkshopUI.Instance?.ShowWinScreen(missionName, CollectedCount, TotalCollectibles);

            // Spieler einfrieren
            if (PlayerController.Instance != null)
                PlayerController.Instance.enabled = false;
        }

        /// <summary>Wird aufgerufen wenn der Spieler Schaden nimmt</summary>
        public void OnPlayerHit()
        {
            onPlayerDamaged?.Invoke();
            AudioManager.Instance?.PlaySFX("Damage");
        }

        /// <summary>Setzt die gesamte Mission zurück</summary>
        public void ResetMission()
        {
            CollectedCount = 0;
            MissionCompleted = false;
            collectedItems.Clear();
            foreach (var door in FindObjectsByType<DoorSwitch>(FindObjectsSortMode.None)) door.ResetDoorSwitch();
            foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None)) checkpoint.ResetCheckpoint();
            foreach (var hazard in FindObjectsByType<Hazard>(FindObjectsSortMode.None)) hazard.ResetHazard();
            AudioManager.Instance?.PlayMusic();

            foreach (var c in allCollectibles)
                if (c != null) c.ResetCollectible();

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.enabled = true;
                PlayerController.Instance.ResetForMission();
            }

            WorkshopUI.Instance?.UpdateCollectibleCounter(0, TotalCollectibles);
            WorkshopUI.Instance?.HideWinScreen();
        }

        /// <summary>Prüft ob die Mission gültig aufgebaut ist</summary>
        public MissionValidationResult ValidateMission()
        {
            var result = new MissionValidationResult();

            // Spieler vorhanden?
            result.hasPlayer = FindAnyObjectByType<PlayerController>() != null;

            // Zielzone vorhanden?
            result.hasGoal = FindAnyObjectByType<GoalZone>() != null;

            // Sammelobjekte?
            var collectibles = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
            result.collectibleCount = collectibles.Length;
            result.hasEnoughCollectibles = collectibles.Length >= 1;

            // Gefahren?
            var hazards = FindObjectsByType<Hazard>(FindObjectsSortMode.None);
            result.hazardCount = hazards.Length;

            // Missionsname?
            result.hasName = !string.IsNullOrEmpty(missionName) && missionName != "Meine Mission";

            return result;
        }
    }

    /// <summary>Ergebnis der Missions-Validierung</summary>
    [System.Serializable]
    public class MissionValidationResult
    {
        public bool hasPlayer;
        public bool hasGoal;
        public int collectibleCount;
        public bool hasEnoughCollectibles;
        public int hazardCount;
        public bool hasName;

        public bool IsPlayable => hasPlayer && hasGoal && hasEnoughCollectibles;

        public override string ToString()
        {
            string status = IsPlayable ? "SPIELBAR" : "NOCH NICHT FERTIG";
            return $"[{status}] Spieler: {(hasPlayer ? "Ja" : "FEHLT")} | " +
                   $"Ziel: {(hasGoal ? "Ja" : "FEHLT")} | " +
                   $"Sammelobjekte: {collectibleCount} | " +
                   $"Gefahren: {hazardCount} | " +
                   $"Name: {(hasName ? "Ja" : "Standard")}";
        }
    }
}

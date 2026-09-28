using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// Definition einer Mission / eines Missionstyps.
    /// Wird vom Mission Setup Wizard verwendet.
    ///
    /// Vordefinierte Missionstypen:
    /// - Forschungsmission
    /// - Schatzsuche
    /// - Rettungsmission
    /// - Natur-/Monster-Expedition
    /// - Geheime Anlage / Labor
    /// - Verlorene Artefakte / Magische Kristalle
    /// </summary>
    [CreateAssetMenu(fileName = "Neue Mission", menuName = "Workshop/Mission")]
    public class MissionData : ScriptableObject
    {
        [Header("Mission")]
        public string missionName = "Neue Mission";

        [TextArea(2, 5)]
        public string missionDescription = "Beschreibe deine Mission hier...";

        public MissionType missionType = MissionType.Forschung;

        public Sprite missionIcon;

        [Header("Zugehöriges Theme")]
        public ThemeData theme;

        [Header("Spielvorgaben")]
        [Tooltip("Empfohlene Anzahl Sammelobjekte")]
        [Range(1, 20)]
        public int suggestedCollectibleCount = 5;

        [Tooltip("Empfohlene Anzahl Gefahren")]
        [Range(0, 10)]
        public int suggestedHazardCount = 3;

        [Tooltip("Sollen alle Sammelobjekte nötig sein für das Ziel?")]
        public bool requireAllCollectibles = true;

        [Header("Workshop-Variante")]
        [Tooltip("Für welche Zeitvariante ist diese Mission gedacht?")]
        public WorkshopDuration targetDuration = WorkshopDuration.Min60;

        [Header("Erweiterungen (90/120 Min)")]
        public bool includeSecondArea = false;
        public bool includeDoorSwitchMechanic = false;
        public bool includeBonusObjective = false;

        [Header("Texte für die Mission")]
        [Tooltip("Intro-Text der am Anfang angezeigt wird")]
        [TextArea(2, 4)]
        public string introText = "Deine Mission beginnt...";

        [Tooltip("Text bei Sieg")]
        [TextArea(2, 4)]
        public string winText = "Mission erfolgreich abgeschlossen!";
    }

    public enum MissionType
    {
        [Tooltip("Gebiet erforschen und Proben sammeln")]
        Forschung,

        [Tooltip("Schätze finden und einsammeln")]
        Schatzsuche,

        [Tooltip("Jemanden retten / etwas befreien")]
        Rettung,

        [Tooltip("Expedition durch Natur oder Monster-Gebiet")]
        Expedition,

        [Tooltip("Geheime Anlage / Labor erkunden")]
        GeheimerOrt,

        [Tooltip("Verlorene magische Artefakte finden")]
        MagischeArtefakte,

        [Tooltip("Eigener Missionstyp")]
        Eigener
    }

    public enum WorkshopDuration
    {
        [Tooltip("Schnelle Mission — 60 Minuten Workshop")]
        Min60,

        [Tooltip("Mission mit Erweiterung — 90 Minuten Workshop")]
        Min90,

        [Tooltip("Rundes Spiel — 120 Minuten Workshop")]
        Min120
    }
}

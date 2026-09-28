using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Workshop.EditorTools
{
    /// <summary>
    /// MISSION SETUP WIZARD
    /// Erstellt mit wenigen Klicks eine neue, spielbare Mission-Szene.
    /// Für Betreuer gedacht — legt alle nötigen Grundobjekte automatisch an.
    ///
    /// Nutzt einen optionalen eigenen Spieler-Prefab oder sichtbare Standardsprites.
    /// Missionstypen haben echten Einfluss auf Texte, Farben und Labels.
    /// </summary>
    public class MissionSetupWizard : EditorWindow
    {
        private string missionName = "";
        private string missionDescription = "";
        private MissionType missionType = MissionType.Forschung;
        private WorkshopDuration duration = WorkshopDuration.Min60;
        private ThemeData selectedTheme;
        private int collectibleCount = 5;
        private int hazardCount = 3;
        private bool addDoorSwitch = false;
        private bool addCheckpoint = false;

        // Prefab-Referenz (wird automatisch gesucht)
        private GameObject playerPrefab;

        [MenuItem("Workshop/Mission Setup Wizard", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<MissionSetupWizard>("Mission Setup Wizard");
            window.minSize = new Vector2(420, 650);
        }

        public static void CreateStarterMission(bool extensions = false)
        {
            var wizard = CreateInstance<MissionSetupWizard>();
            try
            {
                wizard.missionName = "Kristall-Expedition";
                wizard.missionDescription = "Sammle die Kristalle und erreiche das grüne Ziel.";
                wizard.addDoorSwitch = extensions;
                wizard.addCheckpoint = extensions;
                wizard.CreateMission(false);
            }
            finally { DestroyImmediate(wizard); }
        }

        private void OnGUI()
        {

            EditorGUILayout.Space(10);

            // Titel
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Mission Setup Wizard", titleStyle);
            EditorGUILayout.Space(5);

            playerPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Eigener Spieler-Prefab (optional)", playerPrefab, typeof(GameObject), false);
            EditorGUILayout.HelpBox("Ohne Prefab werden sichtbare Standardsprites verwendet.", MessageType.Info);
            EditorGUILayout.Space(5);

            // === MISSIONSTYP ===
            EditorGUILayout.LabelField("Missionstyp", EditorStyles.boldLabel);
            MissionType newType = (MissionType)EditorGUILayout.EnumPopup("Typ", missionType);
            if (newType != missionType)
            {
                missionType = newType;
                ApplyMissionTypeDefaults();
            }

            // Beschreibung des Typs anzeigen
            EditorGUILayout.HelpBox(GetMissionTypeDescription(missionType), MessageType.None);
            EditorGUILayout.Space(5);

            // === GRUNDEINSTELLUNGEN ===
            EditorGUILayout.LabelField("Mission", EditorStyles.boldLabel);
            missionName = EditorGUILayout.TextField("Missionsname", missionName);
            missionDescription = EditorGUILayout.TextArea(missionDescription, GUILayout.Height(40));

            EditorGUILayout.Space(5);

            // === WORKSHOP-DAUER ===
            EditorGUILayout.LabelField("Workshop-Dauer", EditorStyles.boldLabel);
            WorkshopDuration newDuration = (WorkshopDuration)EditorGUILayout.EnumPopup("Dauer", duration);
            if (newDuration != duration)
            {
                duration = newDuration;
                ApplyDurationDefaults();
            }

            // Duration-Info
            string durationInfo = duration switch
            {
                WorkshopDuration.Min60 => "Schnelle Mission: Karte bauen, Objekte setzen, testen. Kein Extra.",
                WorkshopDuration.Min90 => "Mit Erweiterung: Ein zusätzlicher Bereich oder Schalter/Tür.",
                WorkshopDuration.Min120 => "Rundes Spiel: Ausgereifter Aufbau, Deko, mehrere Bereiche.",
                _ => ""
            };
            EditorGUILayout.HelpBox(durationInfo, MessageType.None);

            EditorGUILayout.Space(5);

            // === THEME ===
            EditorGUILayout.LabelField("Themenwelt (optional)", EditorStyles.boldLabel);
            selectedTheme = (ThemeData)EditorGUILayout.ObjectField(
                "Theme", selectedTheme, typeof(ThemeData), false);

            EditorGUILayout.Space(5);

            // === SPIELVORGABEN ===
            EditorGUILayout.LabelField("Spielvorgaben", EditorStyles.boldLabel);
            string collectibleLabel = GetCollectibleLabel(missionType);
            collectibleCount = EditorGUILayout.IntSlider(collectibleLabel, collectibleCount, 1, 15);
            hazardCount = EditorGUILayout.IntSlider("Gefahren", hazardCount, 0, 10);

            EditorGUILayout.Space(5);

            // === ERWEITERUNGEN ===
            EditorGUILayout.LabelField("Erweiterungen", EditorStyles.boldLabel);
            addDoorSwitch = EditorGUILayout.Toggle("Schalter + Tür", addDoorSwitch);
            addCheckpoint = EditorGUILayout.Toggle("Checkpoint", addCheckpoint);

            if (duration == WorkshopDuration.Min60 && (addDoorSwitch || addCheckpoint))
            {
                EditorGUILayout.HelpBox(
                    "Bei 60 Minuten sind Erweiterungen nicht empfohlen — " +
                    "die Zeit reicht kaum für die Grundmission.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space(15);

            // === ERSTELLEN ===
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("Mission erstellen!", GUILayout.Height(45)))
            {
                CreateMission();
            }
            GUI.backgroundColor = Color.white;
        }

        // =============================================
        // MISSIONSTYP-ABHÄNGIGE DEFAULTS
        // =============================================

        private void ApplyMissionTypeDefaults()
        {
            switch (missionType)
            {
                case MissionType.Forschung:
                    if (string.IsNullOrEmpty(missionName) || IsDefaultName(missionName))
                        missionName = "Forschungsmission";
                    missionDescription = "Erkunde das Gebiet und sammle alle Proben ein!";
                    break;
                case MissionType.Schatzsuche:
                    if (string.IsNullOrEmpty(missionName) || IsDefaultName(missionName))
                        missionName = "Die große Schatzsuche";
                    missionDescription = "Finde alle verborgenen Schätze und bringe sie zum Ziel!";
                    break;
                case MissionType.Rettung:
                    if (string.IsNullOrEmpty(missionName) || IsDefaultName(missionName))
                        missionName = "Rettungsmission";
                    missionDescription = "Sammle die Hilfspakete und erreiche den Rettungspunkt!";
                    break;
                case MissionType.Expedition:
                    if (string.IsNullOrEmpty(missionName) || IsDefaultName(missionName))
                        missionName = "Monster-Expedition";
                    missionDescription = "Weiche den Monstern aus und sammle alle Beweise!";
                    break;
                case MissionType.GeheimerOrt:
                    if (string.IsNullOrEmpty(missionName) || IsDefaultName(missionName))
                        missionName = "Geheimes Labor";
                    missionDescription = "Finde die Geheimdaten und entkommen aus der Anlage!";
                    break;
                case MissionType.MagischeArtefakte:
                    if (string.IsNullOrEmpty(missionName) || IsDefaultName(missionName))
                        missionName = "Verlorene Artefakte";
                    missionDescription = "Sammle die magischen Kristalle, bevor es zu spät ist!";
                    break;
                case MissionType.Eigener:
                    if (string.IsNullOrEmpty(missionName) || IsDefaultName(missionName))
                        missionName = "";
                    missionDescription = "";
                    break;
            }
        }

        private bool IsDefaultName(string name)
        {
            string[] defaults = {
                "Meine Mission", "Forschungsmission", "Die große Schatzsuche",
                "Rettungsmission", "Monster-Expedition", "Geheimes Labor",
                "Verlorene Artefakte", ""
            };
            foreach (var d in defaults)
                if (name == d) return true;
            return false;
        }

        private void ApplyDurationDefaults()
        {
            switch (duration)
            {
                case WorkshopDuration.Min60:
                    collectibleCount = Mathf.Min(collectibleCount, 5);
                    hazardCount = Mathf.Min(hazardCount, 3);
                    addDoorSwitch = false;
                    addCheckpoint = false;
                    break;
                case WorkshopDuration.Min90:
                    if (collectibleCount < 4) collectibleCount = 5;
                    if (hazardCount < 2) hazardCount = 3;
                    break;
                case WorkshopDuration.Min120:
                    if (collectibleCount < 5) collectibleCount = 7;
                    if (hazardCount < 3) hazardCount = 4;
                    addCheckpoint = true;
                    break;
            }
        }

        private string GetMissionTypeDescription(MissionType type)
        {
            return type switch
            {
                MissionType.Forschung => "Gebiet erforschen und Proben sammeln. Sammelobjekte = Forschungsproben.",
                MissionType.Schatzsuche => "Versteckte Schätze finden. Sammelobjekte = Goldmünzen/Edelsteine.",
                MissionType.Rettung => "Hilfspakete sammeln und Rettungspunkt erreichen. Sammelobjekte = Hilfspakete.",
                MissionType.Expedition => "Durch gefährliches Gebiet navigieren. Sammelobjekte = Beweise/Fotos.",
                MissionType.GeheimerOrt => "Geheime Anlage erkunden. Sammelobjekte = Geheimdaten/Schlüsselkarten.",
                MissionType.MagischeArtefakte => "Magische Kristalle und Artefakte finden. Sammelobjekte = Kristalle.",
                MissionType.Eigener => "Eigener Missionstyp. Alles frei gestaltbar!",
                _ => ""
            };
        }

        private string GetCollectibleLabel(MissionType type)
        {
            return type switch
            {
                MissionType.Forschung => "Forschungsproben",
                MissionType.Schatzsuche => "Schätze",
                MissionType.Rettung => "Hilfspakete",
                MissionType.Expedition => "Beweise",
                MissionType.GeheimerOrt => "Geheimdaten",
                MissionType.MagischeArtefakte => "Kristalle",
                _ => "Sammelobjekte"
            };
        }

        private Color GetCollectibleColor(MissionType type)
        {
            return type switch
            {
                MissionType.Forschung => new Color(0.2f, 0.8f, 1f),       // Hellblau
                MissionType.Schatzsuche => new Color(1f, 0.85f, 0.1f),    // Gold
                MissionType.Rettung => new Color(1f, 0.4f, 0.4f),         // Rot
                MissionType.Expedition => new Color(0.4f, 1f, 0.4f),      // Grün
                MissionType.GeheimerOrt => new Color(0.6f, 0.4f, 1f),     // Lila
                MissionType.MagischeArtefakte => new Color(0.9f, 0.3f, 1f), // Magenta
                _ => new Color(1f, 0.9f, 0.2f)
            };
        }

        private Color GetHazardColor(MissionType type)
        {
            return type switch
            {
                MissionType.Forschung => new Color(0.8f, 0.4f, 0f, 0.6f),     // Giftig-Orange
                MissionType.Schatzsuche => new Color(0.6f, 0.1f, 0.1f, 0.6f), // Falle-Rot
                MissionType.Rettung => new Color(1f, 0.3f, 0f, 0.6f),         // Feuer
                MissionType.Expedition => new Color(0.5f, 0f, 0.5f, 0.6f),    // Monster-Lila
                MissionType.GeheimerOrt => new Color(0.1f, 0.8f, 0.1f, 0.5f), // Laser-Grün
                MissionType.MagischeArtefakte => new Color(0.3f, 0f, 0.3f, 0.6f), // Dunkellila
                _ => new Color(1f, 0.2f, 0.2f, 0.6f)
            };
        }

        // =============================================
        // MISSION ERSTELLEN
        // =============================================

        private void CreateMission(bool showDialog = true)
        {
            if (Object.FindAnyObjectByType<MissionManager>() != null)
            {
                if (showDialog) EditorUtility.DisplayDialog("Mission vorhanden", "Nutze eine neue Szene für eine weitere Mission.", "OK");
                else throw new System.InvalidOperationException("Scene already contains a mission.");
                return;
            }
            // Defaults anwenden wenn Name/Beschreibung leer
            if (string.IsNullOrEmpty(missionName))
                ApplyMissionTypeDefaults();
            if (string.IsNullOrEmpty(missionName))
                missionName = "Meine Mission";

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Mission erstellen");

            // 1. Manager erstellen
            var managerGO = new GameObject("--- MISSION MANAGER ---");
            var manager = managerGO.AddComponent<MissionManager>();
            manager.missionName = missionName;
            manager.missionDescription = missionDescription;
            Undo.RegisterCreatedObjectUndo(managerGO, "Mission Manager");

            // 2. Audio Manager
            var audioGO = new GameObject("AudioManager");
            audioGO.AddComponent<AudioSource>(); // Erste Source (Musik)
            audioGO.AddComponent<AudioSource>(); // Zweite Source (SFX)
            audioGO.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(audioGO, "Audio Manager");

            // 3. UI
            var uiGO = new GameObject("WorkshopUI");
            uiGO.AddComponent<WorkshopUI>();
            Undo.RegisterCreatedObjectUndo(uiGO, "Workshop UI");

            // 4. Effekt-Spawner (Farbe passend zum Missionstyp)
            var effectGO = new GameObject("PickupEffectSpawner");
            var effectSpawner = effectGO.AddComponent<PickupEffectSpawner>();
            effectSpawner.defaultColor = GetCollectibleColor(missionType);
            Undo.RegisterCreatedObjectUndo(effectGO, "Effekt-Spawner");

            // 4b. Tilemap-Ebenen fuer Kinder vorbereiten
            TilemapLayerSetup.PrepareDefaultLayers(false);

            // =============================================
            // 5. SPIELER — aus Prefab oder manuell
            // =============================================

            // Spawn-Zentrum bestimmen: LevelBounds nutzen wenn vorhanden
            Vector3 spawnCenter = Vector3.zero;
            var existingBounds = Object.FindAnyObjectByType<LevelBounds>();
            if (existingBounds != null)
            {
                var boundsCol = existingBounds.GetComponent<BoxCollider2D>();
                if (boundsCol != null)
                    spawnCenter = boundsCol.bounds.center;
            }

            GameObject playerGO;

            if (playerPrefab != null)
            {
                // Prefab instanziieren — hat Animator, CharacterAppearance, Sprite, Collider, RB
                playerGO = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerGO.name = "Spieler";
                playerGO.transform.position = spawnCenter;

                // Altes PlayerMovement deaktivieren (aus Asset-Pack)
                var oldMovement = playerGO.GetComponent("PlayerMovement") as MonoBehaviour;
                if (oldMovement != null)
                    oldMovement.enabled = false;

                // Unser PlayerController hinzufügen
                if (playerGO.GetComponent<PlayerController>() == null)
                {
                    var pc = playerGO.AddComponent<PlayerController>();
                    pc.moveSpeed = 4f; // Gleich wie Original-Prefab
                }

                // CollectibleArrow hinzufügen
                if (playerGO.GetComponent<CollectibleArrow>() == null)
                    playerGO.AddComponent<CollectibleArrow>();

                // Kamera ist Kind des Prefabs — CameraFollow drauf
                var childCam = playerGO.GetComponentInChildren<Camera>();
                if (childCam != null)
                {
                    // Bestehende Szenen-Kamera entfernen (verhindert doppelten AudioListener)
                    var existingCam = Camera.main;
                    if (existingCam != null && existingCam.gameObject != childCam.gameObject)
                    {
                        Undo.DestroyObjectImmediate(existingCam.gameObject);
                    }

                    // Kamera aus dem Spieler-Hierarchy lösen,
                    // damit sie eigenständig folgt
                    childCam.transform.SetParent(null);
                    childCam.gameObject.name = "Main Camera";
                    childCam.tag = "MainCamera";

                    if (childCam.GetComponent<CameraFollow>() == null)
                    {
                        var camFollow = childCam.gameObject.AddComponent<CameraFollow>();
                        camFollow.target = playerGO.transform;
                    }

                    // Sicherstellen dass genau EIN AudioListener existiert
                    var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                    for (int l = 1; l < listeners.Length; l++)
                        Object.DestroyImmediate(listeners[l]);

                    Undo.RegisterCreatedObjectUndo(childCam.gameObject, "Kamera eingerichtet");
                }
            }
            else
            {
                // Fallback: Spieler manuell erstellen
                playerGO = new GameObject("Spieler");
                playerGO.transform.position = spawnCenter;

                var sr = WorkshopShapes.AddRenderer(playerGO);
                sr.sortingOrder = 4;
                sr.color = Color.white;

                var rb = playerGO.GetComponent<Rigidbody2D>();
                if (rb == null) rb = playerGO.AddComponent<Rigidbody2D>();

                var col = playerGO.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.6f, 0.29f);
                col.edgeRadius = 0.1f;
                col.offset = new Vector2(0f, 0.02f);

                playerGO.AddComponent<PlayerController>().moveSpeed = 4f;
                playerGO.AddComponent<CollectibleArrow>();

                // Szene-Kamera einrichten
                var cam = Camera.main;
                if (cam != null && cam.GetComponent<CameraFollow>() == null)
                {
                    var camFollow = cam.gameObject.AddComponent<CameraFollow>();
                    camFollow.target = playerGO.transform;
                    cam.orthographic = true;
                    cam.orthographicSize = 8f;
                    Undo.RegisterCompleteObjectUndo(cam.gameObject, "Kamera eingerichtet");
                }
            }

            // Every generated mission needs a camera, including an initially empty scene.
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraGO.tag = "MainCamera";
                mainCamera = cameraGO.GetComponent<Camera>();
                Undo.RegisterCreatedObjectUndo(cameraGO, "Kamera erstellt");
            }
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 8f;
            mainCamera.backgroundColor = new Color(0.08f, 0.11f, 0.15f);
            mainCamera.transform.position = spawnCenter + Vector3.back * 10f;
            var follow = mainCamera.GetComponent<CameraFollow>();
            if (follow == null) follow = Undo.AddComponent<CameraFollow>(mainCamera.gameObject);
            follow.target = playerGO.transform;

            Undo.RegisterCreatedObjectUndo(playerGO, "Spieler erstellt");

            // =============================================
            // 6. SAMMELOBJEKTE (Farbe/Label nach Missionstyp)
            // =============================================
            var collectiblesParent = new GameObject("--- SAMMELOBJEKTE ---");
            Undo.RegisterCreatedObjectUndo(collectiblesParent, "Sammelobjekte-Container");

            Color collectibleColor = GetCollectibleColor(missionType);
            string collectibleBaseName = GetCollectibleLabel(missionType);

            for (int i = 0; i < collectibleCount; i++)
            {
                float angle = (i / (float)collectibleCount) * Mathf.PI * 2f;
                float radius = 4f + (collectibleCount > 5 ? 2f : 0f);
                Vector3 pos = spawnCenter + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );

                var collectGO = new GameObject($"{collectibleBaseName}_{i + 1}");
                collectGO.transform.position = pos;
                collectGO.transform.SetParent(collectiblesParent.transform);

                var sr = WorkshopShapes.AddRenderer(collectGO);
                sr.sortingOrder = 5;
                sr.color = collectibleColor;

                var collider = collectGO.AddComponent<CircleCollider2D>();
                collider.radius = 0.4f;
                collider.isTrigger = true;

                collectGO.AddComponent<Collectible>();
                Undo.RegisterCreatedObjectUndo(collectGO, "Sammelobjekt");
            }

            // =============================================
            // 7. GEFAHREN (Farbe nach Missionstyp)
            // =============================================
            var hazardsParent = new GameObject("--- GEFAHREN ---");
            Undo.RegisterCreatedObjectUndo(hazardsParent, "Gefahren-Container");

            Color hazardColor = GetHazardColor(missionType);

            for (int i = 0; i < hazardCount; i++)
            {
                float angle = ((i + 0.5f) / (float)hazardCount) * Mathf.PI * 2f;
                Vector3 pos = spawnCenter + new Vector3(
                    Mathf.Cos(angle) * 3f,
                    Mathf.Sin(angle) * 3f,
                    0f
                );

                var hazardGO = new GameObject($"Gefahr_{i + 1}");
                hazardGO.transform.position = pos;
                hazardGO.transform.SetParent(hazardsParent.transform);

                var sr = WorkshopShapes.AddRenderer(hazardGO);
                sr.sortingOrder = 5;
                sr.color = hazardColor;

                var collider = hazardGO.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 1f);
                collider.isTrigger = true;

                hazardGO.AddComponent<Hazard>();
                Undo.RegisterCreatedObjectUndo(hazardGO, "Gefahr");
            }

            // =============================================
            // 8. ZIELZONE
            // =============================================
            var goalGO = new GameObject("--- ZIELZONE ---");
            goalGO.transform.position = spawnCenter + new Vector3(0, 8, 0);
            var goalCollider = goalGO.AddComponent<BoxCollider2D>();
            goalCollider.isTrigger = true;
            goalGO.AddComponent<GoalZone>();
            Undo.RegisterCreatedObjectUndo(goalGO, "Zielzone");

            // =============================================
            // 9. ERWEITERUNGEN (Schalter/Tür, Checkpoint)
            // =============================================
            if (addDoorSwitch)
            {
                var erweiterungenParent = new GameObject("--- ERWEITERUNGEN ---");

                var switchGO = new GameObject("Schalter");
                switchGO.transform.position = spawnCenter + new Vector3(-3, 4, 0);
                switchGO.transform.SetParent(erweiterungenParent.transform);
                var swSR = WorkshopShapes.AddRenderer(switchGO);
                swSR.sortingOrder = 5;
                swSR.color = Color.yellow;
                var swCol = switchGO.AddComponent<BoxCollider2D>();
                swCol.size = new Vector2(0.8f, 0.8f);
                swCol.isTrigger = true;
                var sw = switchGO.AddComponent<DoorSwitch>();
                sw.switchType = SwitchType.Switch;

                var doorGO = new GameObject("Tür");
                doorGO.transform.position = spawnCenter + new Vector3(0, 6, 0);
                doorGO.transform.SetParent(erweiterungenParent.transform);
                var doorSR = WorkshopShapes.AddRenderer(doorGO);
                doorSR.sortingOrder = 5;
                doorSR.color = new Color(0.6f, 0.3f, 0f);
                doorGO.AddComponent<BoxCollider2D>().size = new Vector2(2f, 0.5f);
                var door = doorGO.AddComponent<DoorSwitch>();
                door.switchType = SwitchType.Door;

                sw.connectedDoors.Add(door);

                Undo.RegisterCreatedObjectUndo(erweiterungenParent, "Erweiterungen");
                Undo.RegisterCreatedObjectUndo(switchGO, "Schalter");
                Undo.RegisterCreatedObjectUndo(doorGO, "Tür");

                if (addCheckpoint)
                {
                    var checkpointGO = new GameObject("Checkpoint");
                    checkpointGO.transform.position = spawnCenter + new Vector3(0, 4, 0);
                    checkpointGO.transform.SetParent(erweiterungenParent.transform);
                    var cpSR = WorkshopShapes.AddRenderer(checkpointGO);
                    cpSR.sortingOrder = 5;
                    cpSR.color = Color.cyan;
                    var cpCol = checkpointGO.AddComponent<CircleCollider2D>();
                    cpCol.radius = 0.5f;
                    cpCol.isTrigger = true;
                    checkpointGO.AddComponent<Checkpoint>();
                    Undo.RegisterCreatedObjectUndo(checkpointGO, "Checkpoint");
                }
            }
            else if (addCheckpoint)
            {
                var checkpointGO = new GameObject("Checkpoint");
                checkpointGO.transform.position = spawnCenter + new Vector3(0, 4, 0);
                var cpSR = WorkshopShapes.AddRenderer(checkpointGO);
                cpSR.sortingOrder = 5;
                cpSR.color = Color.cyan;
                var cpCol = checkpointGO.AddComponent<CircleCollider2D>();
                cpCol.radius = 0.5f;
                cpCol.isTrigger = true;
                checkpointGO.AddComponent<Checkpoint>();
                Undo.RegisterCreatedObjectUndo(checkpointGO, "Checkpoint");
            }

            // 10. Deko-Container
            var decoParent = new GameObject("--- DEKO ---");
            Undo.RegisterCreatedObjectUndo(decoParent, "Deko-Container");

            // Theme anwenden
            if (selectedTheme != null)
                selectedTheme.Apply();

            // Szene als unsaved markieren
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                SceneManager.GetActiveScene());

            Debug.Log($"[Workshop] Mission '{missionName}' erstellt! " +
                     $"(Typ: {missionType}, {collectibleCount} {collectibleBaseName}, " +
                     $"{hazardCount} Gefahren, Prefab: {(playerPrefab != null ? "Ja" : "Nein")})");

            string prefabInfo = playerPrefab != null ?
                "Der Spieler hat Animator, Sprite und Charakter-System." :
                "Die Mission verwendet sichtbare Standardsprites.";

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            if (showDialog) EditorUtility.DisplayDialog("Mission erstellt!",
                $"Mission: \"{missionName}\"\n" +
                $"Typ: {missionType}\n" +
                $"{collectibleBaseName}: {collectibleCount}\n" +
                $"Gefahren: {hazardCount}\n" +
                $"{(addDoorSwitch ? "Mit Schalter/Tür\n" : "")}" +
                $"{(addCheckpoint ? "Mit Checkpoint\n" : "")}\n" +
                $"{prefabInfo}\n\n" +
                "Positioniere jetzt die Objekte. Eigene Sprites kannst du optional zuweisen.",
                "Super!");
        }
    }
}

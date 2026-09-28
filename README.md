# Unity Top Down Workshop

[![Repository checks](https://github.com/Tigertycoon/unity-top-down-workshop/actions/workflows/checks.yml/badge.svg)](https://github.com/Tigertycoon/unity-top-down-workshop/actions/workflows/checks.yml)
[![Unity 6](https://img.shields.io/badge/Unity-6000.0.79f1-black.svg)](ProjectSettings/ProjectVersion.txt)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A **Unity Editor toolkit for guided game-making workshops**. Children build small top-down collection missions through a simplified panel; facilitators prepare scenes, adjust the scope and help learners test their levels. The runtime supplies movement, pickups, hazards, checkpoints, linked switches/doors and a completion screen.

The focus is my **C# editor tooling and workshop workflow**: making a small playable mission approachable without asking beginners to assemble every component themselves. The original design targets 60-, 90- and 120-minute sessions. These are planning presets, not measured learning outcomes.

## Try it

1. Clone the repository and add the folder to Unity Hub.
2. Open it with **Unity 6000.0.79f1**. Let Unity restore the packages.
3. Choose **Workshop → Startszene öffnen**, then press **Play**.
4. Move with **WASD or the arrow keys**, collect the light-blue items, and enter the green goal. Press **R** after completion to restart.
5. Stop Play Mode and open **Workshop → Kinder-Panel (Builder)** to edit the mission.

`Workshop_Template` contains the basic mission; `Workshop_Extended` adds a linked switch/door and checkpoint. Both use plain colored geometry and generated sound cues. No commercial art pack is needed. This public version uses the Built-in Render Pipeline and the legacy Input Manager.

## Tools and runtime

| Area | Included functionality |
| --- | --- |
| Mission Setup Wizard | Mission types, duration presets, object counts, optional player prefab, switch/door and checkpoint creation |
| Child-facing builder | Add objects, edit mission text, follow a checklist and test the result |
| Facilitator panel | Save/duplicate scenes, reset play state, manage optional mechanics and capture screenshots |
| Level checks | Check for player, goal and collectibles; flag missing sprites and supporting components |
| Tilemap setup | Six named layers separating walkable ground, blocking surfaces, hazards and foreground decoration |
| Runtime | Rigidbody2D movement, weighted pickups, goal conditions, knockback, checkpoints, keys, switches/doors, HUD and reset |
| Supporting tools | Theme settings, audio preview, collectible management, hazard placement and mission signage |

Editor UI, comments and learner-facing text are primarily German. Documentation explains the project in English, with a short [German facilitator guide](docs/workshop-guide.de.md).

## Code worth reading

- [MissionSetupWizard](Assets/Workshop/Editor/MissionSetupWizard.cs): mission authoring, sensible defaults and Undo registration.
- [TilemapLayerSetup](Assets/Workshop/Editor/TilemapLayerSetup.cs): repeatable scene setup with distinct collision roles.
- [WorkshopBuilderPanel](Assets/Workshop/Editor/WorkshopBuilderPanel.cs): a reduced editing surface for beginners.
- [MissionManager](Assets/Workshop/Scripts/Core/MissionManager.cs): progress, inventory, completion and full mission reset.
- [Unity tests](Assets/Workshop/Tests): editor authoring and runtime regression coverage.

See [architecture](docs/architecture.md), [validation](docs/validation.md) and [provenance](docs/provenance.md).

## Scope

This is a focused workshop toolkit and starter project. It has no save-game system, multiplayer, asset marketplace or complete curriculum platform. The level checker verifies scene prerequisites; it does not prove that a level is reachable, balanced or suitable for a particular group. Facilitators still need to play-test learner levels.

`MissionData` is a planning data type; the wizard currently uses its own editable fields rather than loading those assets. `ThemeData.Apply()` changes the camera background, music and pickup effect color; the additional prefab/palette fields are preparation data and do not automatically reskin an entire level. Collectible types label items and support key checks; `Bonus` does not automatically exempt an item from the mission total.

## License

My application code and the generated neutral square sprite are [MIT licensed](LICENSE). Unity and its Package Manager dependencies retain their own terms. The original local art packs, music packs, vendor scripts and sample scenes are excluded; see [third-party notes](THIRD_PARTY_NOTICES.md).

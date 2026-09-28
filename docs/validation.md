# Validation

Checked with **Unity 6000.0.79f1 on Windows**, on **2026-09-28**. The original project used 6000.0.72f1; the publication was imported and tested on the installed newer patch.

| Check | Result |
| --- | --- |
| Runtime, editor and test assembly compilation | Passed; no C# compiler errors or warnings in the final runs |
| EditMode suite | **10 passed** |
| PlayMode suite | **5 passed** |
| Starter regeneration | Both scenes generated and saved; final batch process exited successfully |

EditMode tests cover mission creation in an empty scene without an art pack, duplicate-mission prevention, idempotent tilemap setup, visible objects from the builder, and both saved scenes' script and dependency references.

PlayMode tests exercise real Rigidbody2D/trigger interactions: weighted pickup totals, duplicate collection rejection, inactive collected keys unlocking doors, checkpoint respawn versus full reset, goal collider dimensions, stopping knockback on completion, and starting/completing/restarting the saved extended scene. The saved scene test also checks its HUD, font, camera and audio-manager initialization.

## Run Unity tests

Close this project in the editor, then run from the repository root:

```powershell
.\scripts\test-unity.ps1 -UnityPath "C:\Program Files\Unity\Hub\Editor\6000.0.79f1\Editor\Unity.exe"
```

Use `-Mode EditMode` or `-Mode PlayMode` for one suite. The script creates a unique ignored directory under `artifacts/` and retains XML/log reports. It requires a working local Unity installation and license. Tests can also run through Unity's Test Runner.

The public [GitHub workflow](https://github.com/Tigertycoon/unity-top-down-workshop/actions/workflows/checks.yml) checks tracked metadata, duplicate GUIDs, unresolved serialized references and package sources. **It does not compile Unity or run the Unity test suites**; no editor license credentials are configured in CI.

```powershell
python scripts/validate_repository.py
```

## Limits

The automated Unity runs used batch mode without graphics. They establish script, scene and gameplay behavior, not visual layout, audible output or manual usability acceptance. No promotional visual demo was added. No standalone player build, classroom trial, complete keyboard-driven playthrough, custom player-prefab integration, or exhaustive test of every editor window was performed for this publication.

The scene prerequisite checker cannot establish route reachability, fairness or educational suitability. Those still require a facilitator's playtest.

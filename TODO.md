# TODO — granular short-term tasks (v0.5.3)

## Verified working (keep green)
- [x] Low-Spec Dynamic Spatial Sleep LOD (2Hz >35m) & Throttled 10Hz Environmental Calculations in `PatrolMover.cs` & `SurvivalSystem.cs`
- [x] Guard Searchlight Beam Glance Synergy & Power Grid Flicker Dynamics in `PatrolMover.cs` & `LightExposureZone.cs`
- [x] Searchlight 60Hz Transformer Hum & Whistle Concrete Alley Reverberation (+0.5s) in `ProceduralAudio.cs` & `PlayerMove.cs`
- [x] Searchlight Target Tracking & Focused Linger on Disturbances in `LightExposureZone.cs` & `PatrolMover.cs`
- [x] Backlit Silhouette Detection & Guard Searchlight Glare Blindness in `PatrolMover.cs`
- [x] Sprint Exhaustion Cooldown Delay (1.5s) & Heavy-Breathing Audio Footprint in `PlayerMove.cs`
- [x] Frame-Budgeted Adaptive Sensor Ticking (15Hz patrol / 60Hz alert) & Distance Culling in `PatrolMover.cs`
- [x] Searchlight Glare Suspicion Acceleration (2.5x multiplier) in `PatrolMover.cs`
- [x] Oscillating Searchlight Sweeps (`LightExposureZone.cs`) & Obstacle Shadow Occlusion in `PatrolMover.cs`
- [x] Environmental Surface Trigger Zones (`SurfaceZone.cs` & `.meta`) for catwalks and debris
- [x] Surface Material Footstep Noise Scaling (Dirt 0.7x, Metal 1.6x, Glass 2.0x) in `PlayerMove.cs`
- [x] Guard Radio Alert Broadcast & 25m Allied Patrol Convergence in `PatrolMover.cs`
- [x] Guard Flashlight Directional Search Sweeps & Look-Around Scans in `PatrolMover.cs`
- [x] Procedural lo-fi audio generator (`ProceduralAudio.cs`) synthesizing 22kHz mono PCM clips (whistles, bottle shatter, footsteps)
- [x] Unity Asset Metadata Governance: explicit `.meta` files for `ProceduralAudio.cs`, `SoundDistractionEvent.cs`, and `SafehouseTrigger.cs`
- [x] Acoustic Obstacle Occlusion & Wall Sound Attenuation (0.35x) in `PatrolMover.cs`
- [x] Thrown Bottle Sound Distraction Lure (`[F]`, 14m radius) in `PlayerMove.cs` & `SoundDistractionEvent.cs`
- [x] Guard AI Remote Sound Distraction Investigation without player disclosure (`PatrolMover.cs`)
- [x] Temperature-Stamina Coupling & Light Radiant Thermodynamics in `SurvivalSystem.cs` (spotlight halogen radiant heat vs shadow alley wind chill)
- [x] Continuous cold exhaustion (Cold >= 60%) and stationary teeth-chattering acoustic noise (+1.8m) in `PlayerMove.cs`
- [x] Guard Overhead Suspicion Indicators & Investigation Markers floating in 3D world-to-screen (`PatrolMover.cs`)
- [x] Win/Lose instant triggers & Quick Retry loop (< 3s reload) on `[R]` and UI button (`SurvivalSystem.cs`)
- [x] Command Center console redesign with interactive tooltips, copy buttons, and high-value layout (`dashboard.html`)
- [x] System Tray Icon & Server Manager `scripts/tray_manager.py` with process inspection
- [x] Universal cross-platform health runner with dashboard check `scripts/run_checks.py`
- [x] Light Exposure Zones & dynamic detection scaling (`LightExposureZone.cs` + `PatrolMover.cs`)
- [x] Environmental Warming & Shelter Zones (`unity-test/Assets/Act1/ShelterZone.cs`)
- [x] Shivering acoustic noise and stamina penalties (Cold >= 70%) in `SurvivalSystem.cs`
- [x] Starvation sprint stamina exhaustion penalties (Hunger >= 80%) in `SurvivalSystem.cs`
- [x] Hot Tea / Boiled Water ration consumption (`[3]` key) for warmth in `SurvivalSystem.cs`
- [x] Unity Survival System & IMGUI HUD (`unity-test/Assets/Act1/SurvivalSystem.cs` + `PlayerMove.cs`)
- [x] Sound lure mechanics: Player whistle `[Q]` producing a 20m acoustic radius in `PlayerMove.cs`
- [x] Guard acoustic hearing & `Investigating` AI state machine in `PatrolMover.cs`
- [x] Checkpoint security gate & interrogation system (`CheckpointTrigger.cs`) with paper checks `[E]` and bribe `[B]`
- [x] Pure-C# console sim `prototypes/StealthSim` (.NET 10/8, 29/29 tests passing)
- [x] Unity MCP round-trip U-T1–U-T14 via AnkleBreaker (`unity-test/` Act 1 starter)
- [x] Godot G-T1–G-T7 via Coding-Solo G1 (`godot-test/` boot + debug capture + compiled TypeScript)
- [x] Unreal R-T1–R-T2 graceful no-engine failure (sam-david R8)

## Next, highest value first
- [ ] Unity URP scaffold in `unity-project/` (Hub 2022.3 LTS, 3D URP, Very Low preset)
- [ ] Godot editor path: `launch_editor` on `godot-test`, author test scene

## Hygiene (nondestructive)
- [ ] Re-run `python3 scripts/run_checks.py` after any submodule pull
- [ ] Keep version bumps in `VERSION.md` + `CHANGELOG.md`, reference in commit msg

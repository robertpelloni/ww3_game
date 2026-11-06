# ROADMAP — major structural milestones

**Version:** 0.4.0 (see [VERSION.md](VERSION.md)).

## M1 — Vertical slice: Act 1 Blackout Chongjin
- 1 street 60x20m, night + fog + searchlight, 2 patrols + 1 checkpoint guard.
- Win = reach safehouse; lose + retry < 3s; 30fps+ HD 520 Very Low.
- State:
  * Pure-C# console sim verified in `prototypes/StealthSim` (16/16 tests passing).
  * Scene + patrol/player starter scripts live in `unity-test/` (see [docs/MCP_TESTS.md](docs/MCP_TESTS.md)).
  * Procedural lo-fi audio generator (`ProceduralAudio.cs`) generating 22kHz mono PCM clips (whistles, bottle shatter, footsteps).
  * Unity Survival System + IMGUI OnGUI HUD active in `unity-test/Assets/Act1/`.
  * Acoustic obstacle occlusion & realistic sound dampening (0.35x through concrete barricades) in `PatrolMover.cs`.
  * Thrown bottle sound distraction lure (`[F]`, 14m radius) and `SoundDistractionEvent.cs`.
  * Guard AI remote sound investigation without revealing player's position in `PatrolMover.cs`.
  * Temperature-Stamina Coupling & Light Radiant Thermodynamics in `SurvivalSystem.cs` and `PlayerMove.cs`.
  * Continuous cold exhaustion (Cold >= 60%) and stationary teeth-chattering acoustic noise (+1.8m) even when crouched/prone.
  * Guard overhead status markers (`[ ! ALERT ! ]`, `[ ? SUSPICIOUS ]`, `[ ? INVESTIGATING ]`) and suspicion bars in `PatrolMover.cs`.
  * Instant fail states (hypothermia, starvation, curfew capture) and quick-retry loop (< 3s reload) via `[R]` in `SurvivalSystem.cs`.
  * Light Exposure Zones (`LightExposureZone.cs`) with dynamic detection escalation in `PatrolMover.cs`.
  * Environmental Warming & Shelter Zones (`ShelterZone.cs`) with live area temperature feedback.
  * Shivering acoustic penalty (+1.8m noise) & stamina regeneration debuff when cold >= 70%.
  * Starvation sprint stamina exhaustion penalty when hunger >= 80%.
  * Sound lure mechanics (Whistle `[Q]`, 20m noise footprint) in `PlayerMove.cs`.
  * Guard AI 4-state machine (Patrolling, Investigating, Suspicious, Alert) with vision FOV & acoustic hearing in `PatrolMover.cs`.
  * Checkpoint security gate & interrogation system (`CheckpointTrigger.cs`) with transit papers and bribery.
  * System tray icon & background server manager in `scripts/tray_manager.py`.
  * Unified single-page Command Center console active in `dashboard.html`.

## M2 — Survival systems (Completed & Verified)
- Hunger / cold / stamina / batteries / medicine / hot tea; exposure light model;
  suspicion 0-100 investigate → chase → search → reset; whistle lure; shelter zones; dynamic illumination.
- Mathematical mechanics verified in `prototypes/StealthSim` (12/12 automated tests).
- Implemented in `unity-test/Assets/Act1/SurvivalSystem.cs`, `PlayerMove.cs`, and `ShelterZone.cs` with real-time debug HUD.

## M3 — Choice systems
- Papers timed dialogue, bribe (2 rice), sneak (vent/river), wire cutters;
  Act 2 (Papers or River) + Act 3 (DMZ minefield/flare, Alone/Together endings).

## M4 — Presentation
- Minimal HUD (stamina, suspicion diamonds, item count), pause, lo-fi audio
  (.wav 22kHz mono), Very Low quality preset locked.

## M5 — Release
- Itch.io private Windows x64 playtest build; public demo after stability.
- Godot Compatibility fallback only if Unity path blocks on this laptop.

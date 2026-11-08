# ROADMAP — major structural milestones

**Version:** 0.7.1 (see [VERSION.md](VERSION.md)).

## M1 — Vertical slice: Act 1 Blackout Chongjin (Milestone Completed & Verified)
- 1 street 60x20m, night + fog + searchlight, 2 patrols + 1 checkpoint guard.
- Win = reach safehouse; lose + retry < 3s; 30fps+ HD 520 Very Low locked.
- State:
  * Pure-C# console sim verified in `prototypes/StealthSim` (47/47 tests passing).
  * Scene + patrol/player starter scripts live in `unity-test/` (see [docs/MCP_TESTS.md](docs/MCP_TESTS.md)).
  * Broken glass debris acoustic shard escalation (+3.0m noise radius) and sharp guard suspicion responsiveness (50% investigation suspicion floor).
  * Thermal breath vapor condensation puffs and shelter warmth suppression.
  * Procedural winter wind soundscape ambiance and shelter acoustic dampening.
  * Contextual proximity interaction prompts HUD and tool durability degradation.
  * Unified decoupled time-stamped array caching and algebraic smoothstep penumbra falloff.
  * 0 B/frame GC allocation mitigation & native array caching guaranteeing 30fps+ on low-spec hardware.
  * Searchlight penumbra edge falloff and progressive edge-skirting stealth detection.
  * Surface-dependent procedural footstep audio and stance stride pacing.
  * Guard alert shout vocalization audio and military radio click static bursts.
  * Multi-stage guard perimeter investigation probing and searchlight motor mechanical jitter.
  * Complete end-of-mission Victory summary overlay with route attribution and mission timer metrics.
  * Atmospheric fog extinction and searchlight scattering modulation without GPU compute overhead.
  * Real-time FPS profiler HUD and dynamic sensor decimation under load (1.5x) ensuring locked 30fps+ on Intel HD 520.
  * Searchlight pre-lock strobe acquisition warning cue and guard focus signal debouncing.
  * Tactical guard pincer search sweeps (+4.5m lateral offsets) and searchlight beam widening (1.4x).
  * Wire-cutter perimeter fence breach shortcut (`PlayerMove.cs` `[T]`, `ProceduralAudio.cs` snip audio).
  * Checkpoint security gate inspection, 2-rice food bribery, and procedural alarm siren escalation (`CheckpointTrigger.cs`).
  * Low-spec dynamic spatial entity sleep (2Hz >35m) & throttled 10Hz survival environment checks for rock-solid 30fps+ on Intel HD 520.
  * Guard searchlight beam glance synergy & power grid flicker dynamics (`PatrolMover.cs` & `LightExposureZone.cs`).
  * Searchlight 60Hz hum audio & whistle reverberation bonus (+0.5s) in concrete alleys.
  * Searchlight target tracking and focused beam lingering on guard disturbances (`LightExposureZone.cs`).
  * Backlit silhouette detection & guard searchlight glare blindness (-30% range) in `PatrolMover.cs`.
  * Sprint exhaustion 1.5s recovery cooldown delay and heavy-breathing acoustic signature in `PlayerMove.cs`.
  * Frame-budgeted adaptive sensor ticking (15Hz patrol / 60Hz alert) and distance culling in `PatrolMover.cs`.
  * Searchlight glare suspicion escalation (2.5x) and directional alerts in `PatrolMover.cs`.
  * Oscillating Curfew Searchlight Sweeps (`LightExposureZone.cs`) and obstacle vision shadow projection.
  * Environmental Surface Trigger Zones (`SurfaceZone.cs`) for metal catwalks and glass debris.
  * Surface material acoustic noise propagation (Dirt 0.7x, Metal 1.6x, Glass 2.0x) in `PlayerMove.cs`.
  * Guard radio alert communication & 25m allied patrol convergence in `PatrolMover.cs`.
  * Guard flashlight directional search sweeps and alert look-arounds in `PatrolMover.cs`.
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

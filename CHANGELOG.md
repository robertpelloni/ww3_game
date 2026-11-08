# CHANGELOG

All notable changes. Single version source: [VERSION.md](VERSION.md).

## 0.7.0 — 2026-11-04 — Milestone 2 Thermal Stealth & Environmental Vapor Release
- **Thermal Breath Vapor Condensation (`SurvivalSystem.cs`)**:
  - Implemented thermal breath puff mechanics: when freezing (Cold $\ge$ 60%) in outdoor curfew weather, exhalations emit periodic condensation vapor puffs (every 2.5s) that elevate baseline silhouette exposure from 0.05 to 0.30 in dark shadows.
  - Standing inside warming shelters suppresses breath condensation immediately by restoring core temperature.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored thermal breath vapor cycles, exposure scaling, and shelter suppression in `SimulationWorld`.
  - Added automated Test 46 validating thermal condensation breath puff exhalation and shelter suppression (46/46 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 46/46 tests and updated governance docs to version 0.7.0.

## 0.6.9 — 2026-11-04
- **Curfew Night Wind Ambience Synthesis & Dynamic Shelter Dampening (`SurvivalSystem.cs` & `ProceduralAudio.cs`)**:
  - Implemented `CreateNightWindAmbienceClip` in `ProceduralAudio.cs`: generates continuous low-pass filtered brownian noise with slow sinusoidal gust envelopes (0.75Hz).
  - Wired into `SurvivalSystem.cs` 2D ambient audio playback: scales volume dynamically with outdoor cold accumulation (0.20 to 0.55), dampening down to 0.08 inside shelter warming zones.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored dynamic wind soundscape volume scaling and shelter dampening in `SimulationWorld`.
  - Added automated Test 45 validating environmental wind soundscape scaling and shelter dampening (45/45 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 45/45 tests and updated governance docs to version 0.6.9.

## 0.6.8 — 2026-11-04
- **Contextual Proximity Interaction Prompt HUD (`SurvivalSystem.cs`)**:
  - Implemented dynamic proximity interaction prompts floating in the bottom HUD: displays contextual guidance when near shelters (`[SHELTER: ...] Warming core`), checkpoints (`[E] Present Papers | [B] Bribe Guard`), or perimeter fence breaches (`[T] Cut Wire Fence`).
- **Tool Durability & Degradation (`PlayerMove.cs` & `SurvivalSystem.cs`)**:
  - Added durability charges to scavenged wire cutters (`wireCutterDurability = 2`), degrading with each fence cut and snapping when exhausted.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored wire cutter durability degradation in `SimulationWorld`.
  - Added automated Test 44 validating tool durability decay and breach blocking upon exhaustion (44/44 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 44/44 tests and updated governance docs to version 0.6.8.

## 0.6.7 — 2026-11-04
- **Unified Time-Stamped Cache Invalidation (`PatrolMover.cs`)**:
  - Decoupled static array cache updates from per-instance guard loops using unified `Time.unscaledTime` timestamps (`Time.unscaledTime - lastCacheTime >= 1.0f`), guaranteeing that guard and light caches invalidate strictly once per second regardless of scene guard counts.
- **Algebraic Smoothstep Penumbra Falloff (`LightExposureZone.cs`)**:
  - Replaced linear interpolation with an algebraic polynomial smoothstep S-curve (`t * t * (3 - 2 * t)`), eliminating trigonometric runtime calls during multi-light sweeps.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored smoothstep penumbra falloff in `SimulationWorld`.
  - Added automated Test 43 validating smoothstep penumbra calculations (43/43 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 43/43 tests and updated governance docs to version 0.6.7.

## 0.6.6 — 2026-11-04
- **GC Allocation Mitigation & Static Native Array Caching (`PatrolMover.cs` & `SurvivalSystem.cs`)**:
  - Replaced per-frame `FindObjectsByType` heap allocations with cached static arrays (`cachedLightZones`, `cachedGuards`) throttled to 1.0s refresh intervals, achieving 0 B/frame garbage collection allocation during continuous gameplay.
  - Added GC allocation tracking and frame time readout to the OnGUI monitor (`[FPS: XX | GC: 0 B]`).
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Validated sub-millisecond per-step execution times (< 0.1ms/step) across 1,000 continuous simulation steps.
  - Added automated Test 42 verifying zero-allocation GC runtime stepping and performance stability (42/42 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 42/42 tests and updated governance docs to version 0.6.6.

## 0.6.5 — 2026-11-04
- **Searchlight Penumbra Edge Falloff & Progressive Shadow Dynamics (`LightExposureZone.cs` & `SurvivalSystem.cs`)**:
  - Implemented smooth penumbra edge falloff (`GetExposureAtPosition`) differentiating between the inner core (full exposure) and outer perimeter rim (progressive falloff to ambient shadow), rewarding careful edge-skirting stealth.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored searchlight penumbra falloff in `SimulationWorld.CalculatePlayerExposure()`.
  - Added automated Test 41 validating searchlight penumbra falloff and progressive edge exposure (41/41 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 41/41 tests and updated governance docs to version 0.6.5.

## 0.6.4 — 2026-11-04
- **Surface-Dependent Procedural Footstep Audio (`PlayerMove.cs` & `ProceduralAudio.cs`)**:
  - Implemented `CreateFootstepClip(surface)` in `ProceduralAudio.cs` synthesizing terrain-specific audio impulses: soft mud squishes for Dirt, solid taps for Concrete, metallic ringing pings for MetalGrate, and sharp crunches for GlassDebris.
  - Added rhythmic stride pacing in `PlayerMove.cs` with stance-dependent step intervals (Sprint 0.28s, Walk 0.45s, Crouch 0.65s, Prone 0.90s).
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Added footstep stride pacing and surface audio logging in `SimulationWorld`.
  - Added automated Test 40 validating surface-dependent footstep dispatching and stride intervals (40/40 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 40/40 tests and updated governance docs to version 0.6.4.

## 0.6.3 — 2026-11-04
- **Guard Alert Shout Vocalization Synthesis (`PatrolMover.cs` & `ProceduralAudio.cs`)**:
  - Implemented `CreateGuardAlertShoutClip`: synthesizes a harsh 350Hz formant vocal alert shout with 700Hz and 1050Hz throat harmonics in memory when guards spot Jun-ho and transition to `Alert`.
  - Wired into `PatrolMover.cs` state transition logic.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Added guard alert vocal shout event dispatching in `SimulationWorld`.
  - Added automated Test 39 validating guard alert vocalization and event logging (39/39 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` status badge to 39/39 tests and updated governance docs to version 0.6.3.

## 0.6.2 — 2026-11-04
- **Guard Radio Static Click Synthesis & Audio Threat Escalation (`PatrolMover.cs` & `ProceduralAudio.cs`)**:
  - Synthesized a military radio chirp and microphone static burst (`CreateRadioClickClip`) dispatched when patrol guards transmit tactical radio convergence alerts.
  - Wired into `PatrolMover.cs` spatial audio playback so players in nearby alleys receive immediate acoustic warnings when guards coordinate search sweeps.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored radio static clicks and audio event logging in `SimulationWorld`.
  - Added automated Test 38 validating guard vocalization and radio click event dispatches (38/38 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.6.2.

## 0.6.1 — 2026-11-04
- **Multi-Stage Investigation Search Probing (`PatrolMover.cs`)**:
  - Guards investigating disturbances now execute a two-stage perimeter search: first probing the primary disturbance coordinates, pausing for a directional scan, and then inspecting a secondary vantage point (+3.5m lateral offset) before completing search.
- **Searchlight Motor Mechanical Jitter (`LightExposureZone.cs`)**:
  - Added realistic mechanical motor jitter and oscillation variance to sweeping curfew spotlights, eliminating unnatural harmonic perfection without compute shader overhead.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored multi-stage search probing and motor jitter in `SimulationWorld`.
  - Added automated Test 37 validating two-stage guard search probing and searchlight mechanical jitter (37/37 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.6.1.

## 0.6.0 — 2026-11-04 — Milestone 1 Vertical Slice Final Release
- **Act 1 Chongjin Vertical Slice Completion (`SurvivalSystem.cs` & `PlayerMove.cs`)**:
  - Implemented end-of-mission Victory summary overlay displaying stealth metrics: elapsed time, route attribution (Checkpoint Gate vs Wire-Cut Shortcut), health condition, and remaining food rations.
  - Locked milestone requirements: 30fps+ on low-spec Intel HD 520 hardware, < 3s retry loops, fully integrated procedural audio, 4-state guard AI, and multi-path infiltration options.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored victory metrics, mission timer, and route tracking in `SimulationWorld`.
  - Added automated Test 36 validating Act 1 vertical slice mission completion, route attribution, and telemetry (36/36 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.6.0.

## 0.5.9 — 2026-11-04
- **Atmospheric Fog Extinction & Searchlight Scattering (`LightExposureZone.cs` & `PatrolMover.cs`)**:
  - Implemented dynamic atmospheric fog extinction in `PatrolMover.cs`: dense alley fog attenuates unassisted guard vision range by 16.6% (12m to 10m in Unity, 14m to 11.67m in simulation), while guard flashlights and curfew searchlights pierce through clearly.
  - Added searchlight beam scattering in `LightExposureZone.cs` (`GetEffectiveExposure`): fog diffuses direct glare by 0.85x, creating realistic atmospheric light falloff without expensive compute shaders.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored atmospheric fog extinction and searchlight scattering in `SimulationWorld`.
  - Added automated Test 35 validating dense fog vision attenuation and flashlight penetration (35/35 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.5.9.

## 0.5.8 — 2026-11-04
- **Real-Time Frame-Rate Profiler HUD (`SurvivalSystem.cs`)**:
  - Added rolling real-time FPS profiler to the IMGUI survival monitor (`FPS: XX`), verifying 30fps+ performance stability on potato hardware targets.
- **Dynamic Sensor Decimation Under Load (`PatrolMover.cs`)**:
  - Implemented dynamic sensor load decimation: if frame time spikes above 33.3ms (< 30fps), guard sensory intervals are dynamically decimated by 1.5x, automatically throttling physics raycasts and restoring 30fps+ frame rates on dual-core Intel HD 520 hardware.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored dynamic load decimation and interval scaling in `SimulationWorld`.
  - Added automated Test 34 validating dynamic sensor decimation under heavy frame drops (34/34 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.5.8.

## 0.5.7 — 2026-11-04
- **Searchlight Pre-Lock Strobe Acquisition Warning (`LightExposureZone.cs`)**:
  - Implemented 0.5s pre-focus strobe acquisition warning cue (`isAcquiringTarget`): when a searchlight detects a disturbance or receives an alert, the beam strobes rapidly for 0.5s to visually telegraph the impending focus lock before centering on the target.
- **Guard Searchlight Coordination Debounce (`PatrolMover.cs`)**:
  - Added a 2.0s debounce cooldown on `SignalNearbySearchlights` preventing multiple guards in `Suspicious` state from prematurely resetting or overriding active searchlight focus timers.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored searchlight pre-lock strobe warning and focus debounce in `SimulationWorld`.
  - Added automated Test 33 validating pre-lock strobe acquisition warnings and focus lock transitions (33/33 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.5.7.

## 0.5.6 — 2026-11-04
- **Tactical Pincer Search Formations (`PatrolMover.cs`)**:
  - Guards converging on radio or acoustic disturbances now calculate tactical perpendicular flanking offsets (+4.5m / -4.5m) to execute a coordinated pincer sweep rather than stacking onto the same point.
- **Searchlight Cone Diameter Widening (`LightExposureZone.cs`)**:
  - Sweeping searchlights now dynamically expand their beam radius by 1.4x (e.g. 5m to 7m) while locking onto suspicious coordinates, significantly intensifying perimeter tension.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored tactical flanking pincer formations and beam widening in `SimulationWorld`.
  - Added automated Test 32 validating dual-guard tactical pincer convergence and searchlight beam widening (32/32 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.5.6.

## 0.5.5 — 2026-11-04
- **Wire-Cutter Shortcut Mechanics & Perimeter Breach (`PlayerMove.cs` & `SurvivalSystem.cs`)**:
  - Implemented wire cutter interaction (`[T] Cut Wire Fence`) enabling Jun-ho to breach secondary perimeter chain-link fences using scavenged wire cutters (`hasWireCutters = true`).
  - Opens an alternative stealth infiltration alleyway bypassing the heavily fortified Sector 4 Checkpoint Gate.
- **Audio Synthesis Expansion (`ProceduralAudio.cs`)**:
  - Added `CreateWireSnipClip`: synthesizes a sharp metallic wire snip and tension snap in-memory.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored wire cutters, fence cutting, and secondary stealth bypass path in `SimulationWorld`.
  - Added automated Test 31 validating fence breach and safehouse infiltration via secondary stealth shortcut (31/31 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.5.5.

## 0.5.4 — 2026-11-04
- **Checkpoint Interrogation & Food Bribery Polish (`CheckpointTrigger.cs`)**:
  - Implemented 2-rice-bowl bribery mechanics allowing players to barter scarce food rations with corrupt checkpoint guards to secure safe passage through Sector 4.
  - Added perimeter security siren escalation: unauthorized gate breach or failed inspection synthesizes a procedural alarm siren (`ProceduralAudio.cs`) and alerts all patrol guards within 35m.
- **Audio Synthesis Expansion (`ProceduralAudio.cs`)**:
  - Added `CreateAlarmSirenClip`: synthesizes a dual-frequency modulated warble siren (600Hz-900Hz) in-memory without external audio files.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored rice bowl bribery and checkpoint alarm sirens in `SimulationWorld`.
  - Added automated Test 30 validating rice bribery and siren alarm propagation to guards within 35m (30/30 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.5.4.

## 0.5.3 — 2026-11-04
- **Low-Spec Dynamic LOD & Spatial Entity Sleep (`PatrolMover.cs`)**:
  - Guards beyond 35m from the player drop to a lightweight 2Hz sensory sleep mode, skipping all raycasts and trigonometric angles while cruising on patrol to preserve a solid 30fps+ on low-spec hardware (Intel HD 520).
  - Distant guards awaken instantly to full 60Hz evaluation upon player proximity or receiving an allied radio alert.
- **Throttled Environmental Evaluations (`SurvivalSystem.cs`)**:
  - Throttled shelter proximity and light exposure checks to 10Hz instead of running per-frame, eliminating unnecessary CPU overhead.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored dynamic spatial sleep LOD heuristics in `SimulationWorld`.
  - Added automated Test 29 validating low-spec spatial sleep LOD and instant wake-up upon alert (29/29 tests passing).
- **Tooling & Dashboard Sync**:
  - Bumped `dashboard.html` and governance docs to version 0.5.3.

## 0.5.2 — 2026-11-04
- **Guard & Searchlight Synergy (`PatrolMover.cs` & `LightExposureZone.cs`)**:
  - Implemented searchlight beam glance synergy: patrolling guards dynamically glance and orient towards lingering searchlight disturbance hotspots within 25m.
  - Added power grid flicker and strobe state in `LightExposureZone.cs` (`TriggerGridFlicker`) with smooth lerped recovery to standard oscillation after disturbance lingers expire.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Added searchlight beam glance orientation and power flicker state in `SimulationWorld`.
  - Added automated Test 28 validating guard searchlight beam glance synergy and searchlight sweep recovery (28/28 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.5.2 with searchlight-guard synergy telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.5.2.

## 0.5.1 — 2026-11-04
- **Searchlight Electrical Hum Audio & Whistle Reverberation (`ProceduralAudio.cs` & `PlayerMove.cs`)**:
  - Synthesized 60Hz industrial electrical transformer hum and harmonics in `ProceduralAudio.cs` (`CreateSearchlightHumClip`).
  - Added urban concrete/metal alley acoustic reverberation bonus: whistle lures (`[Q]`) echoing between solid alley walls sustain an acoustic detection footprint +0.5s longer than open dirt fields.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored whistle reverberation bonus in `SimulationWorld`.
  - Added automated Test 27 validating whistle acoustic reverberation in enclosed concrete alleys (27/27 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.5.1 with acoustic reverberation telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.5.1.

## 0.5.0 — 2026-11-04
- **Curfew Searchlight Target Tracking & Focused Linger (`LightExposureZone.cs` & `PatrolMover.cs`)**:
  - Implemented `FocusOnTarget`: nearby sweeping searchlights interrupt regular oscillation when guards become `Suspicious` or `Alert`, locking onto disturbance coordinates for 5 seconds before resuming patrol sweeps.
  - Signal propagation: guards signal nearby searchlights within 20m when investigating suspicious player sightings or acoustic disturbances.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored searchlight target focusing and linger timers in `CurfewSearchlight` and `SimulationWorld`.
  - Added automated Test 26 validating searchlight target tracking and focused beam lingering (26/26 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.5.0 with searchlight target locking telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.5.0 (Milestone 1 Core Polish Release).

## 0.4.9 — 2026-11-04
- **Backlit Silhouette Detection & Guard Glare Blindness (`PatrolMover.cs`)**:
  - Implemented `CheckSilhouetteBacklit`: when a player is positioned between a guard and a powerful light source, high-contrast back-illumination overrides crouch/prone invisibility and amplifies detection rate by 1.8x.
  - Implemented guard glare blindness: guards looking directly into active floodlights or spotlights within 6m suffer a 30% reduction in effective vision distance (`glareBlindnessDistanceReduction = 0.30f`).
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored silhouette detection and guard glare blindness in `SimulationWorld`.
  - Added automated Test 25 validating backlit silhouette detection and searchlight glare blindness (25/25 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.9 with silhouette detection and glare blindness telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.9.

## 0.4.8 — 2026-11-04
- **Sprint Exhaustion Cooldown & Heavy-Breathing Audio (`PlayerMove.cs` & `SurvivalSystem.cs`)**:
  - Implemented 1.5s recovery cooldown delay when player completely exhausts stamina (drops to 0). Sprint cannot be immediately spammed until the cooldown expires.
  - Added heavy-breathing acoustic signature (+2.5m noise radius) while exhausted, punishing reckless sprinting in narrow alleys.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored sprint exhaustion cooldown delay and heavy-breathing acoustic footprint in `SimulationWorld`.
  - Added automated Test 24 validating sprint exhaustion recovery delay and heavy-breathing noise footprint (24/24 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.8 with sprint exhaustion telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.8.

## 0.4.7 — 2026-11-04
- **Adaptive Frame-Budgeted Sensor Optimization (`PatrolMover.cs`)**:
  - Implemented dynamic sensor interval budgeting (15Hz when far / patrolling, 60Hz when suspicious or alert) to minimize physics raycasts.
  - Added distance-based raycast culling, conserving CPU cycles on the low-spec Intel HD 520 hardware target to guarantee 30fps+ performance.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored sensor interval evaluation and distance culling in `SimulationWorld`.
  - Added automated Test 23 validating frame-budgeted sensor evaluation and distance culling (23/23 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.7 with sensor frequency budgeting and potato-hardware performance telemetry.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.7.

## 0.4.6 — 2026-11-04
- **Searchlight Glare Suspicion Escalation (`PatrolMover.cs`)**:
  - Implemented `searchlightAlertMultiplier = 2.5f`: when a player is illuminated directly under searchlight glare (exposure $\ge 0.75$), guard suspicion accumulation multiplies by 2.5x.
  - Catches caught out in the open escalate suspicion near-instantaneously, making light timing essential.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored searchlight glare alert escalation dynamics into pure-C# simulation.
  - Added automated Test 22 validating that direct searchlight glare accelerates guard suspicion accumulation by over 3x compared to ambient shadow (22/22 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.6 with searchlight glare escalation tooltips and updated telemetry.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.6.

## 0.4.5 — 2026-11-04
- **Curfew Spotlight Sweeps & Dynamic Shadow Occlusion (`LightExposureZone.cs` & `PatrolMover.cs`)**:
  - Implemented horizontal oscillating searchlight sweeping in `LightExposureZone.cs` (`isSweepingSpotlight = true`, `sweepRangeX = 8.0f`).
  - Added chest-level raycast line-of-sight checks in `PatrolMover.cs` ensuring solid barricades and concrete walls cast vision-blocking shadows even when searchlights sweep nearby.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Implemented obstacle wall line-of-sight ray occlusion against guard vision cones.
  - Added automated Test 21 validating that solid barricades project protective shadows against sweeping spotlights and guard vision (21/21 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.5 with sweeping searchlight telemetry and shadow projection tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.5.

## 0.4.4 — 2026-11-04
- **Environmental Surface Zones (`SurfaceZone.cs` & `.meta`)**:
  - Created `SurfaceZone` trigger component for authoring metal catwalk grates, puddles/mud, and shattered glass debris in Act 1 levels.
  - Automatically modulates `PlayerMove.cs` `currentSurface` when stepping into the zone bounds without manual tag setup.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Implemented `SurfaceZone` world geometry in `SimulationWorld`.
  - Added automated Test 20 validating dynamic surface transitions (concrete $\to$ metal catwalk $\to$ broken bottle rubble) and subsequent acoustic movement noise expansion (20/20 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.4 with surface trigger zone telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.4.

## 0.4.3 — 2026-11-04
- **Surface Material Sound Propagation (`PlayerMove.cs`)**:
  - Implemented `SurfaceType` awareness: Dirt (0.7x dampening), Concrete (1.0x baseline), MetalGrate (1.6x amplification), GlassDebris (2.0x loud crunch).
  - Dynamically modulates movement noise radius, making route planning across rooftops and ventilation catwalks tactical.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored `SurfaceType` and ground material multipliers into pure-C# simulation.
  - Added automated Test 19 validating dynamic acoustic movement noise scaling across all surface materials (19/19 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.3 with ground surface acoustic telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.3.

## 0.4.2 — 2026-11-04
- **Guard Radio Alert & Allied Convergence (`PatrolMover.cs`)**:
  - Implemented mutual guard communication: when any guard enters `Alert` state (suspicion $\ge 0.8$), they broadcast a vocal/radio alert across a 25m radius (`radioBroadcastRadius = 25.0f`).
  - Allied guards within range receive the alert, transition to `Suspicious` or `Investigating` states, and converge on the disturbance coordinates.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Implemented `BroadcastRadioAlert` in `SimulationWorld`.
  - Added automated Test 18 validating that an alert from one guard successfully routes nearby patrolling guards to converge on the target coordinates (18/18 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.2 with guard radio alert convergence telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.2.

## 0.4.1 — 2026-11-04
- **Guard Flashlight Search Sweeps (`PatrolMover.cs`)**:
  - Implemented dynamic directional flashlight scanning when guards transition into `Investigating` or `Suspicious` states.
  - Guards sweep their beam side-to-side (+/- 45°) while searching sound origins, drastically increasing detection rate (effective exposure $\ge 0.75$) when catching hiding players in dark alleys.
  - Added Gizmo beam rendering for in-editor debug verification.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Mirrored guard flashlight activation and search beam dynamics into pure-C# simulation.
  - Added automated Test 17 validating search flashlight activation and rapid detection in dark alleys (17/17 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.1 with guard flashlight sweeping telemetry and tooltips.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.1.

## 0.4.0 — 2026-11-04
- **Procedural Audio Synthesizer (`ProceduralAudio.cs`)**:
  - Implemented 22kHz mono PCM procedural audio generator synthesizing whistle tone pulses, glass bottle impact shatter bursts, and footstep pulses in-memory without external asset dependencies.
  - Tailored specifically to the low-spec hardware budget (Intel HD 520, zero asset overhead).
  - Wired into `PlayerMove.cs` for whistle `[Q]` and bottle throw `[F]`.
- **Unity Asset Metadata Governance**:
  - Created explicit `.meta` files for `ProceduralAudio.cs`, `SoundDistractionEvent.cs`, and `SafehouseTrigger.cs`.
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Added procedural audio event dispatch and logging.
  - Added automated Test 16 validating procedural audio trigger dispatching for whistle and bottle impacts (16/16 tests passing).
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.4.0 with procedural audio tooltips and audio synthesis telemetry.
- **Documentation & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.4.0.

## 0.3.9 — 2026-11-04
- **Acoustic Obstacle Occlusion & Realistic Sound Propagation (`PatrolMover.cs`)**:
  - Added line-of-sight acoustic raycasting between guards and sound origins (player footsteps, shivering teeth chattering, whistles, and thrown bottle impacts).
  - Concrete barricades, buildings, and solid walls attenuate transmitted acoustic volume by 65% (`wallSoundOcclusionFactor = 0.35f`).
- **StealthSim Simulation Suite Expansion (`prototypes/StealthSim`)**:
  - Implemented `ObstacleWall` bounding-box geometry and sound ray occlusion logic in `prototypes/StealthSim/Program.cs`.
  - Added automated Test 15 validating acoustic wall dampening (player walking 4m away behind concrete barricade remains unheard, but alerts guard when stepping into an unoccluded opening). Expanded test suite to 15/15 passing tests.
- **Single-Page Command Center Sync (`dashboard.html`)**:
  - Bumped Command Center console to v0.3.9 with sound occlusion tooltips and updated telemetry.
- **Governance & Verification**:
  - Synced `VERSION.md`, `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` to version 0.3.9.

## 0.3.8 — 2026-11-04
- **Thrown Bottle Distraction Lure (`PlayerMove.cs` + `SoundDistractionEvent.cs`)**: Added scavenged glass bottle inventory item (`bottleCount = 2`) and throw impulse trigger (`[F]` key) that spawns a temporary `SoundDistractionEvent` at the impact location with a 14m acoustic noise radius.
- **Guard AI Remote Distraction Investigation (`PatrolMover.cs`)**: Upgraded guard state machine to listen for environmental acoustic distraction events. Hearing a shattered bottle transitions guards to `GuardState.Investigating` and routes them directly to the impact location without discovering the player's true position.
- **Act 1 Survival HUD Refresh (`SurvivalSystem.cs`)**: Wired bottle inventory count and `[F] Throw Bottle` key bindings into the OnGUI HUD monitor.
- **Console Prototype Expansion (`prototypes/StealthSim`)**: Mirrored thrown bottle distraction mechanics and guard lure investigation into pure-C# simulation; added Test 14 verifying remote guard luring without exposing hidden player, expanding automated tests to 14/14 passing.
- **Command Center Dashboard Sync (`dashboard.html`)**: Bumped Command Center console to v0.3.8 with distraction mechanics and bottle inventory tooltips.

## 0.3.7 — 2026-11-04
- **Temperature-Stamina Coupling & Light Radiant Thermodynamics (`SurvivalSystem.cs` + `PlayerMove.cs`)**: Curfew spotlights/floodlights (exposure >= 0.70) emit radiant halogen heat slowing frostbite accumulation by 50%, whereas dark wind tunnel alleys (exposure <= 0.15) increase cold accumulation rate by +25%.
- **Cold-Induced Baseline Stamina Exhaustion**: Freezing body temperatures (Cold >= 60%) now cause continuous stamina burn as core warmth is maintained, accelerating sprint exhaustion.
- **Stationary Teeth-Chattering Noise Footprint (`PlayerMove.cs`)**: Freezing shivering (Cold >= 70%) generates a continuous +1.8m acoustic signature even while completely stationary or crouched in shadows.
- **Console Prototype Expansion (`prototypes/StealthSim`)**: Added Test 13 validating thermal radiant dynamics and cold-based stamina depletion, bringing automated pure-C# tests to 13/13 passing.
- **Command Center Dashboard Sync (`dashboard.html`)**: Bumped Command Center console to v0.3.7 with expanded survival thermodynamics tooltips.

## 0.3.6 — 2026-11-04
- **Guard Overhead Suspicion & Investigation Markers (`PatrolMover.cs`)**: World-to-screen projected IMGUI status indicators (`[ ! ALERT ! ]`, `[ ? SUSPICIOUS ]`, `[ ? INVESTIGATING ]`) and dynamic colored suspicion fill bar above guard positions.
- **Quick-Reload Game Loop & Win/Loss Logic (`SurvivalSystem.cs`)**: Added instant fail triggers for hypothermia, starvation, and alert patrol capture (within 1.8m), coupled with a rapid (< 3s) retry loop via `[R]` key or interactive retry button.
- **Console Prototype Headless Test Expansion (`prototypes/StealthSim`)**: Added Test 12 confirming quick restart state cleanup across player meters and AI states, expanding automated test suite to 12/12 passing.
- **Single-Page Command Center Sync (`dashboard.html`)**: Bumped console to v0.3.6 with full overhead indicators and quick-reload telemetry.

## 0.3.5 — 2026-11-04
- **Light Exposure Zones (`LightExposureZone.cs`)**: Created dedicated illumination zone component for streetlamps and curfew searchlights with configurable radius and exposure levels (0.0 deep shadow to 1.0 direct glare). Includes `LightExposureZone.cs.meta`.
- **Dynamic Guard Detection Multipliers (`PatrolMover.cs`)**: Integrated ambient light exposure (`Mathf.Lerp(0.3f, 1.8f, lightExposure)`) and player stance into vision cone detection rate, escalating suspicion significantly faster in illuminated areas.
- **Survival HUD Illumination Meter (`SurvivalSystem.cs`)**: Wired live light exposure percentage into the IMGUI monitor so players know their visibility status in real time.
- **Console Prototype Headless Test Expansion (`prototypes/StealthSim`)**: Added Test 11 validating that spotlight illumination escalates guard suspicion faster than shadow, bringing headless test suite to 11/11 passing.
- **Universal Health Runner Sync (`scripts/run_checks.py`)**: Updated to validate `LightExposureZone.cs` and track 11/11 passing StealthSim tests.
- **Single-Page Command Center (v0.3.5)**: Updated `dashboard.html` to v0.3.5 with light exposure documentation and tooltip verification.

## 0.3.4 — 2026-11-04
- **Act 1 Environmental Warming & Shelter Zones (`ShelterZone.cs`)**: Created dedicated shelter zone component allowing Jun-ho to warm up near burn barrels and covered alleys (`-6.0/s` cold recovery) instead of accumulating curfew hypothermia. Includes `ShelterZone.cs.meta`.
- **Shivering Acoustic Footprint & Movement Debuff (`SurvivalSystem.cs`)**: Cold levels >= 70% now trigger physical shivering, generating an involuntary +1.5m acoustic noise radius that can alert nearby guards and reducing stamina recovery by 60%.
- **Starvation Sprint Drain Penalty (`SurvivalSystem.cs`)**: Hunger levels >= 80% mark Jun-ho as starving, multiplying sprint stamina exhaustion rate by 1.75x.
- **Hot Tea & Boiled Water Item (`[3]` key)**: Added hot tea consumption reducing cold by 45% and restoring 25 stamina and 10 health.
- **Survival HUD Expansion**: Updated IMGUI monitor with live area shelter readout, shivering warning, starving indicator, and full keybinding tips.
- **Console Prototype Expansion (`prototypes/StealthSim`)**: Added `Shelter` zones and 2 new automated headless tests (shelter warming and shivering acoustic noise), bringing total automated test suite to 10/10 passed.
- **Universal Health Runner Sync (`scripts/run_checks.py`)**: Updated to validate `ShelterZone.cs` and track 10/10 passing StealthSim tests.

## 0.3.3 — 2026-11-04
- **Acoustic Noise & Sound Lure System (`PlayerMove.cs`)**: Added player whistle lure input `[Q]` producing a 20-meter acoustic radius, dynamically integrated into stance noise footprints.
- **Guard Acoustic Hearing & Investigation State Machine (`PatrolMover.cs`)**: Expanded guard AI states to include `Investigating`, reacting immediately to player footsteps and sound lures, inspecting noise origins, searching the area with rotational scans, and smoothly returning to patrol.
- **Checkpoint Security Gate & Interrogation System (`CheckpointTrigger.cs`)**: Created dedicated checkpoint security logic featuring transit paper inspections `[E]`, bribery mechanics `[B]`, and security alarms alerting nearby patrol guards.
- **Act 1 Survival HUD Enhancements (`SurvivalSystem.cs`)**: Extended the live OnGUI monitor to display real-time acoustic noise radius, active whistling feedback, guard detection status, and contextual keybindings.
- **Unified Dashboard v0.3.3 Sync (`dashboard.html`)**: Updated the Act 1 Starter Scene card with checkpoint interaction controls, acoustic hearing documentation, and bumped system version to v0.3.3.

## 0.3.2 — 2026-11-04
- **Command Center Dashboard Overhaul (`dashboard.html`)**: Redesigned single-page console categorized strictly by high value (Playable Simulators & Engines &rarr; System Tray & Server Control &rarr; MCP AI Fleet Operations Console &rarr; Game Systems & Survival Architecture &rarr; Diagnostics, Build Scripts & Governance). Expanded interactive tooltips (`<dfn title="...">`) to 41 items with interactive copy buttons and offline zero-dependency styling.
- **System Tray & Server Manager Refinement (`scripts/tray_manager.py`)**: Enhanced process descriptions, port bindings, and graceful termination handling for background MCP server processes.
- **Automated Dashboard Verification (`scripts/run_checks.py`)**: Integrated dashboard HTML version sync and tooltip coverage checks into universal cross-platform health runner.
- **Submodule & Repository Health**: Re-verified Godot G1 TypeScript compilation (`build/index.js`), clean working trees, and 8/8 passing automated simulation tests.

## 0.3.1 — 2026-11-03
- **Act 1 Guard AI State Machine**: Upgraded `PatrolMover.cs` in `unity-test/Assets/Act1/` to a full 3-state AI (Patrolling, Suspicious, Alert) with vision cone checks (angle and distance), player stance visibility scaling, and editor gizmos.
- **Dynamic Noise System**: Added stance-based noise radius in `PlayerMove.cs` (Prone 0.5m, Crouch 2.0m, Walk 5.0m, Sprint 12.0m).
- **Survival HUD Alert Readout**: Extended `SurvivalSystem.cs` IMGUI monitor to display real-time guard detection state and player noise emissions.
- **Simulation Test 8**: Extended `prototypes/StealthSim/Program.cs` with an 8th headless automated test for guard alert escalation under line-of-sight exposure.
- **Verification Runner**: Updated `scripts/run_checks.py` to validate all Act 1 Unity C# assets.

## 0.3.0 — 2026-11-03
- **Unity Survival System Prototype**: Implemented `SurvivalSystem.cs` in `unity-test/Assets/Act1/` modeling hunger, cold, stamina, health, and item consumption (rice and medicine), along with an interactive debug IMGUI OnGUI HUD for testing.
- **Player Stance & Movement Integration**: Enhanced `PlayerMove.cs` with prone, crouch, walk, and sprint stance support, variable movement speeds, hunger/cold movement penalties, and sprint stamina exhaustion.
- **Dashboard Synchronization**: Updated `dashboard.html` with Unity Survival HUD details and synchronized version tags to `0.3.0`.
- **Governance**: Updated `ROADMAP.md`, `TODO.md`, and `HANDOFF.md` reflecting completed survival system implementation in Unity Act 1.

## 0.2.0 — 2026-11-03
- **Pure-C# StealthSim Prototype**: Built and verified `prototypes/StealthSim` (.NET console app) implementing vision cones, light/shadow exposure (dark 0.05, lamp 0.5, spotlight 1.0), noise rings based on stance, patrol/chase/search AI with suspicion meters, whistle lure, and survival meters (hunger, cold, stamina, battery, inventory). Included automated verification suite (`--test`, 7/7 passing) and benchmark (`--benchmark`, >1.5M steps/sec).
- **System Tray Icon & Server Manager**: Built `scripts/tray_manager.py` with tray support (pystray) and headless CLI control (`--status`, `--stop-all`, `--start`, `--daemon`) to monitor background MCP processes and cleanly quit servers.
- **Redesigned Command Center (`dashboard.html`)**: Redesigned dashboard into an intuitive, high-value single-page Command Center categorized by priority: Playable Simulators & Engine Targets (StealthSim, Act 1, Godot) &rarr; System Tray & Server Control widget &rarr; MCP Fleet Directory (23 servers) &rarr; Game Systems & Exposure Model &rarr; Toolchain Scripts & Health Monitor. Included 30 interactive tooltip markers (`<dfn title="...">`).
- **Cross-Platform Health Runner**: Built `scripts/run_checks.py` providing universal Linux/macOS/Windows execution for doc link validation, StealthSim tests, Server & Tray Manager status, and MCP protocol smokes.
- **Documentation & Governance**: Synchronized `VISION.md`, `ROADMAP.md`, `TODO.md`, `DEPLOY.md`, `MEMORY.md`, `IDEAS.md`, `README.md`, and `HANDOFF.md` to version 0.2.0.

## 0.1.0 — 2026-10-31
- Added governance docs: `VISION.md`, `ROADMAP.md`, `TODO.md`, `MEMORY.md`,
  `DEPLOY.md`, `IDEAS.md`, `CHANGELOG.md`, `VERSION.md`, `HANDOFF.md`.
- Added `dashboard.html`: single-page console consolidating Run / MCP fleet /
  Docs / Scripts with tooltip guidance (no subpages; grouped by value).
- Added `scripts/run-all-checks.ps1`: one-command repo health (MCP smokes +
  doc-link check); originals kept untouched, no archives needed.
- Noted: no dashboard or tray icon existed before this version (new build,
  not a redesign); no tray icon shipped — decision recorded in `TODO.md`.
- Fixed `scripts/mcp-run.py`: force UTF-8 on server stdout (tool descriptions
  carry bytes outside Windows cp1252; `readline()` crashed). Unity AnkleBreaker
  bridge re-verified via `mcp-run.py` (`unity-mcp v2.36.0`, full `tools/list`).

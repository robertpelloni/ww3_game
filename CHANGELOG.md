# CHANGELOG

All notable changes. Single version source: [VERSION.md](VERSION.md).

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

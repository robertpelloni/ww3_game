# CHANGELOG

All notable changes. Single version source: [VERSION.md](VERSION.md).

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

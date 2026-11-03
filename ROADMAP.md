# ROADMAP — major structural milestones

**Version:** 0.3.1 (see [VERSION.md](VERSION.md)).

## M1 — Vertical slice: Act 1 Blackout Chongjin
- 1 street 60x20m, night + fog + searchlight, 2 patrols + 1 checkpoint guard.
- Win = reach safehouse; lose + retry < 3s; 30fps+ HD 520 Very Low.
- State:
  * Pure-C# console sim verified in `prototypes/StealthSim` (8/8 tests passing).
  * Scene + patrol/player starter scripts live in `unity-test/` (see [docs/MCP_TESTS.md](docs/MCP_TESTS.md)).
  * Unity Survival System + IMGUI OnGUI HUD active in `unity-test/Assets/Act1/`.
  * Unity Guard AI state machine (Patrolling, Suspicious, Alert) with vision cone checks and stance scaling in `PatrolMover.cs`.
  * System tray icon & background server manager in `scripts/tray_manager.py`.

## M2 — Survival systems (In Progress -> Functional Prototype Complete)
- Hunger / cold / stamina / batteries / medicine; exposure light model;
  suspicion 0-100 investigate → chase → search → reset; whistle lure.
- Mathematical mechanics verified in `prototypes/StealthSim`.
- Implemented in `unity-test/Assets/Act1/SurvivalSystem.cs` and `PlayerMove.cs` with real-time debug HUD.

## M3 — Choice systems
- Papers timed dialogue, bribe (2 rice), sneak (vent/river), wire cutters;
  Act 2 (Papers or River) + Act 3 (DMZ minefield/flare, Alone/Together endings).

## M4 — Presentation
- Minimal HUD (stamina, suspicion diamonds, item count), pause, lo-fi audio
  (.wav 22kHz mono), Very Low quality preset locked.

## M5 — Release
- Itch.io private Windows x64 playtest build; public demo after stability.
- Godot Compatibility fallback only if Unity path blocks on this laptop.

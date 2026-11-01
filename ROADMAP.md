# ROADMAP — major structural milestones

**Version:** 0.1.0 (see [VERSION.md](VERSION.md)).

## M1 — Vertical slice: Act 1 Blackout Chongjin (current)
- 1 street 60x20m, night + fog + searchlight, 2 patrols + 1 checkpoint guard.
- Win = reach safehouse; lose + retry < 3s; 30fps+ HD 520 Very Low.
- State: scene + patrol/player scripts live in `unity-test/` (see [docs/MCP_TESTS.md](docs/MCP_TESTS.md)).

## M2 — Survival systems
- Hunger / cold / stamina / batteries / medicine; exposure light model;
  suspicion 0-100 investigate → chase → search → reset; whistle lure.

## M3 — Choice systems
- Papers timed dialogue, bribe (2 rice), sneak (vent/river), wire cutters;
  Act 2 (Papers or River) + Act 3 (DMZ minefield/flare, Alone/Together endings).

## M4 — Presentation
- Minimal HUD (stamina, suspicion diamonds, item count), pause, lo-fi audio
  (.wav 22kHz mono), Very Low quality preset locked.

## M5 — Release
- Itch.io private Windows x64 playtest build; public demo after stability.
- Godot Compatibility fallback only if Unity path blocks on this laptop.

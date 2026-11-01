# IDEAS — aggressive pivots and expansions

1. **Dashboard-first workflow:** this repo now has [dashboard.html](dashboard.html)
   consolidating Run / MCP / Docs / Scripts on one page. Next: live log tail
   + one-click MCP smoke buttons (Tauri shell-out) instead of pasted commands.
2. **Tray commander:** no tray icon exists today. Cheapest real option: Python
   `pystray` + `unity_list_instances` poll, Start/Stop bridge, Open dashboard,
   Quit servers. Defer Electron/Tauri until M2.
3. **StealthSim before engine:** implement `prototypes/StealthSim` FIRST and tune
   detection math headless — 10x faster iteration than Unity domain reloads.
4. **Godot-first vertical slice:** Unity 6000 boots but reloads cost minutes;
   mirror Act 1 in `godot-test/` (Compatibility renderer) for same-day playtest.
5. **U-T15 fix or drop:** game-view capture writes no file — either focus Game
   view before capture or delete the step and bless position-polling as proof.
6. **Submodule diet:** 24 vendored servers is heavy; pin 3 (AnkleBreaker U4,
   Coding-Solo G1, sam-david R8) as supported, move rest to on-demand clones.
7. **Language port:** GDScript mirror of `PatrolMover`/`PlayerMove` for Godot
   parity; keep C# canonical for Unity.

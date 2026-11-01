# VISION — WW3: Escape from North Korea

**Version:** 0.1.0 (see [VERSION.md](VERSION.md)) — pre-production, Unity-first.

## Ultimate goal
Ship a complete low-spec stealth-survival game: escape Chongjin as WW3 erupts,
reach freedom in under 72 in-game hours through stealth, survival management,
and branching choices. Must run at 30fps+ on Intel HD 520 (1280x720, Very Low).

## Core concepts
1. **Stealth over shooting** — vision cones, noise rings, light/shadow exposure,
   curfew searchlights. Getting seen is the fail state.
2. **Survival pressure** — hunger, cold, stamina, batteries, medicine. Every detour costs.
3. **Choices with cost** — forged papers vs bribe vs Tumen river vs smuggler tunnel.
   Endings: Alone / Together / Caught (roguelite mercy: retry with one extra item).
4. **Mood over polys** — night fog + one realtime light + baked AO, lo-fi wind/sirens.

## Scope boundaries (v1)
Top-down stealth-survival, low-poly 3D URP, 3 acts (Blackout Chongjin vertical
slice first). No multiplayer, vehicles, guns, open world, HDRP, ray tracing,
full VO (text + KR flavor lines only). See [docs/GAME_DESIGN.md](docs/GAME_DESIGN.md).

## How we build it
Unity 2022 LTS / Unity 6 URP on this laptop; Unreal MCPs for research only;
Godot 4 Compatibility as fallback testbed. MCP-first: AnkleBreaker bridge drives
scene/script work, `scripts/mcp-call.js` smoke-tests servers, `unity-test/`
holds the Act 1 starter scene. See [docs/ENGINE_DECISION.md](docs/ENGINE_DECISION.md),
[docs/MCP_SETUP.md](docs/MCP_SETUP.md), [docs/MCP_TESTS.md](docs/MCP_TESTS.md).

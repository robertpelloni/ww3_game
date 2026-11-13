# WW3: Escape from North Korea

A stealth-survival game: WW3 breaks out and you are a civilian in Chongjin with 72 hours before the northern front collapses. No guns-blazing — you sneak, barter, hide, and run: avoid patrols, manage hunger/cold/stamina, forge papers, bribe guards, cross the Tumen river into China, then the DMZ to freedom.

Pillars: stealth over shooting (cone-of-vision, noise, light/shadow, curfew); survival pressure (hunger, cold, stamina, medicine, batteries); choices matter (papers vs bribe vs river vs tunnel, who you trust); low-spec stylized (night fog + searchlights carry the mood, not poly count).

## Status

**Active pre-production (v1.0.1).** Last significant activity: **2026-11-12** — merge of a command-center/tray-cleanup branch (Protocol #281). Nov 10–11 shipped v0.9.6–v1.0.1: an Act 1 vertical slice with low-spec performance optimization, checkpoint bribery audio, shelter/safehouse distance optimization, and multi-guard raycast interleaving.

Primary engine: **Unity 2022 LTS / Unity 6 LTS + URP** (laptop-friendly, runs on Intel HD 520 + dual-core). Unreal Engine 5 is research-only (docs + MCP — too heavy for the dev laptop, see `docs/ENGINE_DECISION.md`). Built AI-first with Model Context Protocol (MCP) servers for Unity + Unreal.

## Repo layout

- `docs/` — `ENGINE_DECISION.md` (Unity vs Unreal on this laptop), `GAME_DESIGN.md` (GDD v0.1), `MCP_SETUP.md`
- `mcp-servers/` — 23 vendored MCP servers (8 Unity + 8 Unreal + 7 Godot); 13/23 live-verified per repo docs
- `unity-project/` — Unity URP project (open with Unity Hub)
- `prototypes/` — pure C# / console prototypes (no engine needed), including a top-down console stealth sim (patrol cones, noise, light)
- `godot-test/`, `unity-test/` — engine test projects; `scripts/` — setup scripts
- `.mcp.json` / `.vscode/mcp.json` — MCP client config

Note: 17 near-identical dashboard screenshots (`dashboard_v085.png`–`dashboard_v101.png`, ~680 KB each) are committed at the root — bloat worth cleaning up.

## Quickstart (Windows 10, low-spec)

1. Install Unity Hub, then **Unity 2022.3 LTS** with Windows Build Support (IL2CPP). Open `unity-project/` (create as 3D URP template if empty). Set Quality to Very Low.
2. Activate MCP servers: `.\scripts\install-mcp.ps1`, then point your AI client at `.mcp.json`.
3. Run the no-engine prototype: `dotnet run --project prototypes/StealthSim`

Do not install Unreal Engine on the dev laptop unless you free 80 GB+ and accept a 5 fps editor; research via MCP servers instead.

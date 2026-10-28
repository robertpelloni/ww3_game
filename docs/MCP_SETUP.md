# MCP_SETUP — verified install & activation (2026-10-27, this laptop)

All 10 servers vendored as git submodules under `mcp-servers/`. Upstreams are source of truth.
Only **one Unity bridge at a time** (they share the Editor connection). Default-on: `unity-anklebreaker`.

## Unity — ✅ all 5 installed, built & smoke-tested on this laptop

| ID | Folder | Install (verified) | Run (verified) | Smoke |
|----|--------|--------------------|----------------|-------|
| U1 CoplayDev/unity-mcp (15k⭐) | `mcp-servers/unity/CoplayDev.unity-mcp` | `uvx` auto-installs `mcpforunityserver` from PyPI (72 pkgs, OK) | `uvx --from mcpforunityserver mcp-for-unity --transport stdio` | `--help` OK |
| U2 IvanMurzak/Unity-MCP (4.5k⭐) | `mcp-servers/unity/IvanMurzak.Unity-MCP` | `npm install && npm run build` in `cli/` (52 pkgs, OK) | **CLI-managed:** `node cli/bin/unity-mcp-cli.js setup-mcp <agent> [path]` writes the agent's own config (14 agents: claude-code→`.mcp.json`, cursor, vscode-copilot…). Run it per-agent after Unity is installed. Do NOT hand-run for claude-code without backing up root `.mcp.json`. | `--help` + `setup-mcp --list` OK |
| U3 CoderGamester/mcp-unity (2k⭐) | `mcp-servers/unity/CoderGamester.mcp-unity` | `npm install && npm run build` in `Server~/` (OK) | `node "Server~/build/index.js"` | MCP `initialize` → `MCP Unity Server v1.5.0` ✅ |
| U4 AnkleBreaker/unity-mcp-server (~500⭐) | `mcp-servers/unity/AnkleBreaker.unity-mcp-server` | `npm install` (93 pkgs, OK, pure JS, no build) | `node src/index.js` → 338 tools (69 core + 269 advanced) | MCP `initialize` → `unity-mcp v2.36.0` ✅ **DEFAULT-ON** |
| U5 jackwrichards/UnityMCP (~500⭐) | `mcp-servers/unity/jackwrichards.UnityMCP` | `npm install && npm run build` in `unity-mcp-server/` (19 pkgs, OK) | `node unity-mcp-server/build/index.js` (binds WS :8080 for its Unity plugin) | MCP `initialize` → `unity-mcp-server v0.1.0` ✅ |

Unity Editor side: install the matching UPM package per server README (`MCPForUnity` via git URL for U1, plugin for U2/U3/U4/U5), then `Window → MCP → Configure/Connect`.

## Unreal — research only (UE5 editor NOT installed on this laptop)

| ID | Folder | Install | Run | Smoke |
|----|--------|---------|-----|-------|
| R1 chongdashu/unreal-mcp (2.1k⭐) | `mcp-servers/unreal/chongdashu.unreal-mcp` | `uv` syncs on first run (uv 0.13.6 present) | `uv --directory <repo>/Python run unreal_mcp_server.py` (+ copy `UnrealMCP.uplugin` into a UE 5.5 project, TCP :55557) | command verbatim from upstream README |
| R2 ChiR24/Unreal_mcp (~1k⭐) | `mcp-servers/unreal/ChiR24.Unreal_mcp` | `npm install && npm run build` (207 pkgs, OK) | `node dist/cli.js` (proxy: answers NOT_CONNECTED until Editor + C++ bridge up) | MCP `initialize` → `proxy v0.6.0` ✅ |
| R3 kvick-games/UnrealMCP (~600⭐) | `mcp-servers/unreal/kvick-games.UnrealMCP` | `pip install mcp>=0.1.0` | `python MCP/unreal_mcp_bridge.py` (needs UE + UnrealMCP plugin, socket :13377) | command verbatim from upstream README |
| R4 GenOrca/unreal-mcp (~150⭐) | `mcp-servers/unreal/GenOrca.unreal-mcp` | `uv` syncs on first run | `uv --directory <repo>/mcp-server run src/unreal_mcp/main.py` (+ Unreal-MCPython plugin from Fab) | command verbatim from upstream README |
| R5 runreal/unreal-mcp (~110⭐) | `mcp-servers/unreal/runreal.unreal-mcp` | none (npm package) | `npx -y @runreal/unreal-mcp` (needs UE 5.4+ with Python Editor Script plugin + Remote Execution enabled, no custom UE plugin) | command verbatim from upstream README |

Use **absolute paths** for the `uv --directory` entries if your MCP client spawns servers with a different cwd.

## One-shot installer

```powershell
.\scripts\install-mcp.ps1
```

Covers Node (`npm install` where `package.json` exists) + Python (`requirements.txt`).
TypeScript builds (`U3` Server~/, `U5` unity-mcp-server, `R2`, `U2` cli) were already run during setup;
rebuild after `git pull` inside a submodule with `npm run build`.

## Activation matrix

| Client | File | Notes |
|--------|------|-------|
| Claude Code / Cursor / Windsurf / pi | `.mcp.json` (repo root) | `unity-anklebreaker` ON, rest OFF — flip `disabled` as needed |
| VS Code MCP extension | `.vscode/mcp.json` | U4 + U1 entries |
| U2 IvanMurzak flow | per-agent file | `node mcp-servers/unity/IvanMurzak.Unity-MCP/cli/bin/unity-mcp-cli.js setup-mcp --list` then `setup-mcp <agent>` |

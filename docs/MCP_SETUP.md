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

## Godot — ✅ 7/7 vendored, 4/7 Node builds smoke-tested (no Godot editor on this laptop yet)

> Godot is the escape hatch for this laptop: unlike UE5, Godot 4 (Compatibility renderer) and Godot 3 run fine on HD 520 + dual-core. Not installed yet — get it at https://godotengine.org/download (standard 64-bit build, ~100MB). Set `GODOT_PATH` for G1/G7 or place at `C:\Program Files\Godot\Godot.exe`.

| ID | Folder | Install (verified) | Run (verified) | Smoke |
|----|--------|--------------------|----------------|-------|
| G1 Coding-Solo/godot-mcp (6.4k⭐) | `mcp-servers/godot/Coding-Solo.godot-mcp` | `npm install && npm run build` (44 pkgs, OK) | `node build/index.js` (set `GODOT_PATH` if outside default) | MCP `initialize` → `godot-mcp v0.1.0` ✅ (warns w/o editor, by design) |
| G2 hi-godot/godot-ai (3.2k⭐) | `mcp-servers/godot/hi-godot.godot-ai` | PyPI published (`godot-ai` 4.3.1) — no local build needed | `uvx godot-ai` (+ Godot plugin from repo) | PyPI presence verified; first run fetches on demand |
| G3 yurineko73/Godot-MCP-Native (~860⭐) | `mcp-servers/godot/yurineko73.Godot-MCP-Native` | none — Godot-native addon, zero deps | editor headless: `godot --editor --path <proj> -- --mcp-server [--mcp-port=9080]`; client: `npx mcp-remote http://localhost:9080/mcp` (see `godot-native` entry) | command verbatim from upstream README |
| G5 ee0pdt/Godot-MCP (~620⭐) | `mcp-servers/godot/ee0pdt.Godot-MCP` | `npm install && npm run build` in `server/` (OK) | `node server/dist/index.js` (bridges to editor WS `ws://localhost:9080` via addon) | boots, retries editor WS ✅ (needs editor) |
| G7 tugcantopaloglu/godot-mcp (~500⭐) | `mcp-servers/godot/tugcantopaloglu.godot-mcp` | `npm install && npm run build` (160 pkgs, OK, 157 tools) | `node build/index.js` (+ `GODOT_PATH`) | MCP `initialize` → `godot-mcp v0.1.0` ✅ |
| G8 tomyud1/godot-mcp (~450⭐) | `mcp-servers/godot/tomyud1.godot-mcp` | `npm install && npm run build` in `mcp-server/` (OK, incl. visualizer) | `node mcp-server/dist/index.js` (71 tools; WS :6505 + HTTP :6506, waits for editor) | boots, lists 71 tools ✅ |
| G9 IvanMurzak/Godot-MCP (~310⭐) | `mcp-servers/godot/IvanMurzak.Godot-MCP` | `npm install && npm run build` in `cli/` (`godot-cli` v0.33.0, OK) | **CLI-managed** (same family as U2): `node cli/bin/godot-cli.js configure --agent <id>` / `install-plugin` / `open` | `--help` OK |

## Round-2 extras (Unity +8, Unreal +8) — ✅ all vendored, Node builds smoke-tested

| ID | Folder | Install (verified) | Run (verified) | Smoke |
|----|--------|--------------------|----------------|-------|
| U6 isuzu-shiranui/UnityMCP (~340⭐) | `mcp-servers/unity/isuzu-shiranui.UnityMCP` | Unity package via git URL (`?path=jp.shiranui-isuzu.unity-mcp`) + native `isuzu-unity-cli` via `install.ps1` — **no Node/.NET runtime needed** | no separate server: Editor publishes Streamable HTTP at `http://127.0.0.1:<port>/mcp`; proxy stdio via `mcp-remote` (port is per-Editor, token-guarded — copy URL from Editor, see `docs/en/security.md`) | README-verified, docs-only (dynamic port, can't static-config) |
| U7 FunplayAI/funplay-unity-mcp (~270⭐) | `mcp-servers/unity/FunplayAI.funplay-unity-mcp` | Unity package (Editor/) | in-process HTTP `http://127.0.0.1:<port>/mcp/project/<id>/` — use Editor's one-click client config (per-project ports); broker mode keeps port stable | README-verified, docs-only (dynamic port) |
| U8 Glade-tool/glade-mcp (~210⭐, Unity **+ Godot**) | `mcp-servers/unity/Glade-tool.glade-mcp` | PyPI published (`gladekit-mcp` 0.7.23) + `unity-bridge/` + `godot-bridge/` in repo | `uvx gladekit-mcp` (see `unity-glade` entry) | PyPI presence verified |
| R6 IvanMurzak/Unreal-MCP (~50⭐) | `mcp-servers/unreal/IvanMurzak.Unreal-MCP` | `npm install && npm run build` in `cli/` (`unreal-mcp-cli` v0.25.0, OK) | **CLI-managed**: `setup-mcp --list`, `install-plugin`, `open`, `exec`, `wait-for-ready` | `--help` OK |
| R7 appleweed/UnrealMCPBridge (~60⭐) | `mcp-servers/unreal/appleweed.UnrealMCPBridge` | `pip install -r MCPClient/requirements.txt` (`mcp>=1.0`) | `python MCPClient/unreal_mcp_client.py` (+ UE plugin, see `MCPPythonBridge-GettingStarted.md`) | command verbatim from upstream |
| R8 sam-david/unreal-mcp (127 tools, no C++ plugin) | `mcp-servers/unreal/sam-david.unreal-mcp` | `npm install && npm run build` (OK) | `node dist/bin.js` (modules: console/actor/asset/build/blueprint/material/sequencer/animation/niagara/testing/…) | MCP `initialize` ✅ with module list |

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

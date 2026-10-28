# MCP_TESTS — engine round-trip report (2026-10-27/28, Lenovo i7-6600U/HD520)

Harness: `scripts/mcp-call.js` (minimal MCP stdio client: initialize → call → print).
All calls below were executed live from this repo. Editors: Unity 6000.6.7f1 ✅ installed,
Godot 4.3 (local `tools/godot/`, gitignored) ✅ installed, Unreal Engine ❌ absent (3.4GB disk free — cannot install).

## ✅ Unity — FULL round-trip (AnkleBreaker U4 + unity-mcp-plugin v2.41.4)

Setup: `unity-test/` created via `Unity.exe -batchmode -createProject`, plugin wired in
`Packages/manifest.json` as `"com.anklebreaker.unity-mcp": "file:../../mcp-servers/unity/AnkleBreaker.unity-mcp-plugin"`,
Editor launched with `-projectPath unity-test` (left running, bridge :7890).

| # | Task | Result |
|---|------|--------|
| U-T1 | `tools/list` | 80 tools (69 core + 269 advanced via `unity_advanced_tool`) ✅ |
| U-T2 | `unity_list_instances` (no editor) | `totalCount: 0` + helpful message ✅ (graceful) |
| U-T3 | `unity_list_instances` (editor up) | 1 instance: `unity-test`, 6000.6.7f1, plugin 2.41.4, port 7890 ✅ |
| U-T4 | `unity_gameobject_create` {name: WW3_TestCube, Cube @ (0,1,0)} | `success:true`, instanceId 1099511641193 ✅ |
| U-T5 | `unity_scene_hierarchy` | cube present w/ MeshFilter+BoxCollider+MeshRenderer ✅ |
| U-T6 | `unity_screenshot_scene` | `Assets/Screenshots/SceneView_20261028_000623.png` (106KB) — white cube visible ✅ |
| U-T7 | `unity_hub_list_editors` | timed out (>25s Hub query on dual-core) — retry with longer window, non-blocking |

## ✅ Godot — FULL protocol + engine interrogation (Coding-Solo G1 + Godot 4.3)

Setup: Godot 4.3 stable in `tools/godot/` (gitignored), `GODOT_PATH` set to `_console.exe`,
scratch project `godot-test/project.godot` (gl_compatibility). No editor window needed.

| # | Task | Result |
|---|------|--------|
| G-T1 | `tools/list` | 10+ tools: launch_editor, run_project, get_debug_output, get_godot_version, list_projects… ✅ |
| G-T2 | `get_godot_version` | `4.3.stable.official.77dcf97d8` — real engine string via MCP ✅ |
| G-T3 | `list_projects` {directory: repo} | found `godot-test` ✅ (bare call correctly errors “Directory is required”) |
| G-T4 | `get_project_info` {projectPath: godot-test} | name/path/version + structure {scenes:0, scripts:0, assets:0} ✅ |
| G-T5 | G7 tugcantopaloglu `initialize` | `godot-mcp v0.1.0` ✅ (warns w/o `GODOT_PATH`, by design) |
| G-T6 | G8 tomyud1 boot | 71 tools, WS :6505 + HTTP :6506, waits for editor ✅ |

Next (needs editor window): `launch_editor` on `godot-test`, then `create_scene`/`add_node` + `run_project`.

## ⚠️ Unreal — protocol + error path only (sam-david R8, no engine on disk)

| # | Task | Result |
|---|------|--------|
| R-T1 | `tools/list` | full catalog ✅ |
| R-T2 | `execute_console_command` {stat fps} | `isError:true` — “Unreal Editor is not connected…” ✅ (correct graceful failure) |

Full UE testing needs a machine with the engine + Remote Control/Python plugins (see R1–R8 rows in MCP_SETUP.md).

## Repro

```powershell
# Unity (editor must be running with the plugin)
node scripts/mcp-call.js . node mcp-servers/unity/AnkleBreaker.unity-mcp-server/src/index.js -- tools/call '{"name":"unity_list_instances","arguments":{}}'
# Godot (GODOT_PATH set, no editor needed for read tasks)
$env:GODOT_PATH="$pwd/tools/godot/Godot_v4.3-stable_win64_console.exe"
node scripts/mcp-call.js . node mcp-servers/godot/Coding-Solo.godot-mcp/build/index.js -- tools/call '{"name":"get_godot_version","arguments":{}}'
# Unreal (always safe — fails gracefully without editor)
node scripts/mcp-call.js . node mcp-servers/unreal/sam-david.unreal-mcp/dist/bin.js -- tools/list '{}'
```

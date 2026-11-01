# DEPLOY — setup and run (Windows 10, low-spec laptop)

**Version:** 0.1.0 (see [VERSION.md](VERSION.md)).

## Prereqs
- Unity Hub + **Unity 6000.6.7f1** (installed per [docs/MCP_TESTS.md](docs/MCP_TESTS.md);
  README also allows 2022.3 LTS) with Windows Build Support (ILPPC).
- Node 18+, Python 3.10+, `uv`/`uvx`, .NET SDK (for `prototypes/StealthSim`).
- Godot 4.3 stable optional: `tools/godot/` (gitignored), set
  `$env:GODOT_PATH="$pwd/tools/godot/Godot_v4.3-stable_win64_console.exe"`.
- Unreal Engine: NOT installed (needs 80GB+; research via MCP only).

## Install
```powershell
.\scripts\install-mcp.ps1
```
Re-run after any submodule pull; rebuild TS (`npm run build`) in U2/U3/U5/R2
submodules after `git pull` inside them.

## Run
- Unity testbed: open `unity-test/` in Hub, bridge auto-registers on `:7890`;
  starter scene `Assets/Act1_Starter.unity`. Round-trip proof:
  `node scripts/mcp-call.js . node mcp-servers/unity/AnkleBreaker.unity-mcp-server/src/index.js -- tools/call '{"name":"unity_list_instances","arguments":{}}'`
- Godot probe: `node scripts/mcp-session.js node mcp-servers/godot/Coding-Solo.godot-mcp/build/index.js -- godot-test`
- Dashboard (static, no server): open [dashboard.html](dashboard.html) in a browser.

## Build / release
No CI. Itch.io private Windows x64 (Very Low preset) is manual from Unity.
Keep `Library/Temp/Logs/UserSettings` + `tools/godot/*` uncommitted (see [.gitignore](.gitignore)).

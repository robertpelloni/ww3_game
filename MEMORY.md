# MEMORY — architectural observations (v0.1.0)

- Repo is **MCP-toolchain-first**: 24 submodules under `mcp-servers/` (8 Unity +
  8 Unreal + 7 Godot + plugin). Upstreams are source of truth; `.mcp.json`
  default-on is only `unity-anklebreaker`, rest `disabled` (one Unity bridge at
  a time — they share the Editor connection).
- Two testbeds, different fates: `unity-test/` is **versioned** (Act 1 starter
  scene + scripts committed); `godot-test/` is a minimal gl_compatibility probe;
  `unity-project/` is still a placeholder README; `tools/` holds only gitignored
  local engine binaries.
- Scripts are already minimal and single-purpose — do NOT merge them:
  `scripts/mcp-call.js` (one-shot stdio call), `scripts/mcp-session.js`
  (persistent session for run→poll→stop), `scripts/mcp-run.py` (file-param
  variant with `$file` inline), `scripts/install-mcp.ps1` (Node/Python deps).
- Known fragile points: U-T15 game-view screenshot writes no file (use position
  polling instead); one-shot clients can't poll Godot `get_debug_output`
  (needs persistent session); Unity domain reload ~2–4 min on dual-core after
  script add; Safe Mode trips on Rider/VS TestRunner packages (removed from
  manifest); `jackwrichards.UnityMCP` submodule path untracked — verify intent.
- Submodules currently dirty (see [TODO.md](TODO.md)) — always inspect per
  submodule before any superproject commit.

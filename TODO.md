# TODO — granular short-term tasks (v0.1.0)

## Verified working (keep green)
- [x] Unity MCP round-trip U-T1–U-T14 via AnkleBreaker (`unity-test/` Act 1 starter)
- [x] Godot G-T1–G-T7 via Coding-Solo G1 (`godot-test/` boot + debug capture)
- [x] Unreal R-T1–R-T2 graceful no-engine failure (sam-david R8)

## Next, highest value first
- [ ] Act 1 playable: suspicion meter + win/lose + retry < 3s; fix U-T15
  game-view screenshot gap (needs focused Game view) or keep position-poll proof
- [ ] `prototypes/StealthSim`: `dotnet new console -o StealthSim`, implement cone +
  noise + exposure model from [docs/GAME_DESIGN.md](docs/GAME_DESIGN.md)
- [ ] Unity URP scaffold in `unity-project/` (still placeholder — Hub 2022.3 LTS,
  3D URP, Very Low preset); keep `unity-test/` as scratch until migration
- [ ] Godot editor path: `launch_editor` on `godot-test`, then create_scene/add_node
- [ ] Godot session flake (2026-10-31): `run_project` booted engine cleanly with
  zero errors but `get_debug_output` showed no `WW3_TEST` lines (docs G-T7 once
  captured them) — retry with longer poll wait before assuming regression
- [ ] Dashboard: verify [dashboard.html](dashboard.html) against this list after edits
- [ ] Tray icon: none exists — decide Tauri/Electron vs Python pystray before building
- [ ] Docs: keep [DEPLOY.md](DEPLOY.md) + [docs/MCP_SETUP.md](docs/MCP_SETUP.md) in sync
  after every submodule pull

## Hygiene (nondestructive)
- [ ] Submodules show local modifications (godot x3, unity x1, unreal x1 + untracked
  `jackwrichards.UnityMCP`): inspect with `git diff --submodule`, stash or commit
  inside each submodule deliberately — never bulk-commit from superproject
- [ ] Decide fate of `unity-test-run.log` + `unity-test...` 2-byte file at root
- [ ] Re-run `scripts/install-mcp.ps1` after any submodule pull (Node builds)

# TODO — granular short-term tasks (v0.2.0)

## Verified working (keep green)
- [x] Pure-C# console sim `prototypes/StealthSim` (.NET 10/8, 7/7 tests passing)
- [x] System Tray Icon & Server Manager `scripts/tray_manager.py` (--status, --stop-all, --start)
- [x] Redesigned single-page Command Center `dashboard.html` with 30 interactive tooltips
- [x] Universal cross-platform health runner `scripts/run_checks.py`
- [x] Unity MCP round-trip U-T1–U-T14 via AnkleBreaker (`unity-test/` Act 1 starter)
- [x] Godot G-T1–G-T7 via Coding-Solo G1 (`godot-test/` boot + debug capture)
- [x] Unreal R-T1–R-T2 graceful no-engine failure (sam-david R8)

## Next, highest value first
- [ ] Wire `StealthSim` logic into `unity-test` / `unity-project` C# MonoBehaviours
- [ ] Act 1 playable: suspicion meter UI + win/lose + retry < 3s in Unity scene
- [ ] Unity URP scaffold in `unity-project/` (Hub 2022.3 LTS, 3D URP, Very Low preset)
- [ ] Godot editor path: `launch_editor` on `godot-test`, author test scene
- [ ] System Tray packaging: build Windows standalone exe wrapper for `tray_manager.py` (PyInstaller)

## Hygiene (nondestructive)
- [ ] Re-run `python3 scripts/run_checks.py` after any submodule pull
- [ ] Keep version bumps in `VERSION.md` + `CHANGELOG.md`, reference in commit msg

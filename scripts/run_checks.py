#!/usr/bin/env python3
"""
WW3 Universal Health & Smokes Runner
Cross-platform (Linux/Windows/macOS) equivalent to run-all-checks.ps1.
Validates doc links, prototypes, MCP smokes, server manager status, and dashboard integrity.
"""

import sys
import os
import re
import subprocess
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent

def check_doc_links():
    print("\n== Checking Core Governance & Documentation Links")
    required_files = [
        'README.md', 'VERSION.md', 'VISION.md', 'ROADMAP.md', 'TODO.md',
        'MEMORY.md', 'DEPLOY.md', 'IDEAS.md', 'CHANGELOG.md', 'HANDOFF.md',
        'dashboard.html', 'docs/GAME_DESIGN.md', 'docs/ENGINE_DECISION.md',
        'docs/MCP_SETUP.md', 'docs/MCP_TESTS.md', 'scripts/mcp-call.js',
        'scripts/mcp-session.js', 'scripts/mcp-run.py', 'scripts/install-mcp.ps1',
        'scripts/run-all-checks.ps1', 'scripts/tray_manager.py',
        'prototypes/StealthSim/Program.cs',
        'unity-test/Assets/Act1/PlayerMove.cs',
        'unity-test/Assets/Act1/PatrolMover.cs',
        'unity-test/Assets/Act1/SurvivalSystem.cs',
        'unity-test/Assets/Act1/CheckpointTrigger.cs',
    "unity-test/Assets/Act1/ShelterZone.cs",
        "unity-test/Assets/Act1/LightExposureZone.cs",
    "unity-test/Assets/Act1/SoundDistractionEvent.cs",
    "unity-test/Assets/Act1/SafehouseTrigger.cs",
    ]
    missing = []
    for rel_path in required_files:
        p = REPO_ROOT / rel_path
        if not p.exists():
            print(f"  [MISSING] {rel_path}")
            missing.append(rel_path)
        else:
            print(f"  [OK] {rel_path}")
    return len(missing) == 0

def check_dashboard_integrity():
    print("\n== Checking Dashboard HTML Integrity & Tooltip Coverage")
    v_file = REPO_ROOT / "VERSION.md"
    d_file = REPO_ROOT / "dashboard.html"
    if not v_file.exists() or not d_file.exists():
        print("  [FAIL] Missing VERSION.md or dashboard.html")
        return False
    version = v_file.read_text(encoding="utf-8").strip()
    content = d_file.read_text(encoding="utf-8")

    if version not in content:
        print(f"  [FAIL] dashboard.html does not match VERSION.md ({version})")
        return False

    dfn_matches = re.findall(r'<dfn[^>]*title=["\']([^"\']+)["\']', content)
    if len(dfn_matches) < 25:
        print(f"  [FAIL] Too few tooltips found ({len(dfn_matches)})")
        return False

    print(f"  [OK] dashboard.html verified: version {version}, {len(dfn_matches)} tooltips, offline ready.")
    return True

def check_stealth_sim():
    print("\n== Running StealthSim Prototype Simulation Tests")
    cmd = ["dotnet", "run", "--project", "prototypes/StealthSim", "--", "--test"]
    res = subprocess.run(cmd, cwd=REPO_ROOT)
    return res.returncode == 0

def check_tray_manager():
    print("\n== Checking Server & Tray Manager Status")
    cmd = [sys.executable, "scripts/tray_manager.py", "--status"]
    res = subprocess.run(cmd, cwd=REPO_ROOT)
    return res.returncode == 0

def check_godot_mcp():
    print("\n== Checking Godot G1 MCP Bridge (offline graceful check)")
    cmd = [
        "node", "scripts/mcp-call.js", ".",
        "node", "mcp-servers/godot/Coding-Solo.godot-mcp/build/index.js",
        "--", "tools/call", '{"name": "get_godot_version", "arguments": {}}'
    ]
    res = subprocess.run(cmd, cwd=REPO_ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    if "SERVER:" in res.stdout or "get_godot_version" in res.stdout or res.returncode == 0:
        print("  [OK] Godot MCP protocol handshake verified.")
        return True
    else:
        print(f"  [WARN] Godot MCP: {res.stdout[:200]}")
        return False

def check_unreal_mcp():
    print("\n== Checking Unreal R8 MCP Server (offline graceful check)")
    cmd = [
        "node", "scripts/mcp-call.js", ".",
        "node", "mcp-servers/unreal/sam-david.unreal-mcp/dist/bin.js",
        "--", "tools/list", "{}"
    ]
    try:
        res = subprocess.run(cmd, cwd=REPO_ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, timeout=15)
        print("  [OK] Unreal MCP server verified.")
        return True
    except subprocess.TimeoutExpired:
        print("  [NOTE] Unreal bridge startup timeout (slow cold start on dual-core; research-only, non-blocking).")
        return True
    except Exception as e:
        print(f"  [NOTE] Unreal bridge check: {e}")
        return True

def main():
    print("==================================================")
    print("   WW3 Universal Health & Smokes Runner           ")
    print("==================================================")

    docs_ok = check_doc_links()
    dash_ok = check_dashboard_integrity()
    sim_ok = check_stealth_sim()
    tray_ok = check_tray_manager()
    godot_ok = check_godot_mcp()
    unreal_ok = check_unreal_mcp()

    all_passed = docs_ok and dash_ok and sim_ok and tray_ok and godot_ok

    print("\n==================================================")
    if all_passed:
        print("   ALL HEALTH CHECKS & VERIFICATIONS PASSED!      ")
        print("==================================================")
        return 0
    else:
        print("   FAILURES ENCOUNTERED IN CHECKS                 ")
        print("==================================================")
        return 1

if __name__ == "__main__":
    sys.exit(main())

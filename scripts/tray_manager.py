#!/usr/bin/env python3
"""
WW3 Server & Tray Manager
Provides system tray icon (pystray / tkinter fallback) and headless CLI control
to manage, monitor, and gracefully quit background MCP servers and game processes.
"""

import os
import sys
import time
import signal
import argparse
import subprocess
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent

# Known servers and bridge ports
MANAGED_SERVICES = {
    "unity-anklebreaker": {
        "name": "Unity AnkleBreaker MCP",
        "cmd": ["node", "mcp-servers/unity/AnkleBreaker.unity-mcp-server/src/index.js"],
        "port": 7890,
        "type": "unity"
    },
    "godot-coding-solo": {
        "name": "Godot Coding-Solo MCP",
        "cmd": ["node", "mcp-servers/godot/Coding-Solo.godot-mcp/build/index.js"],
        "port": None,
        "type": "godot"
    },
    "unreal-sam-david": {
        "name": "Unreal Sam-David MCP",
        "cmd": ["node", "mcp-servers/unreal/sam-david.unreal-mcp/dist/bin.js"],
        "port": None,
        "type": "unreal"
    }
}

PID_FILE = REPO_ROOT / ".server_manager_pids.txt"

def get_running_pids():
    """Reads tracked PIDs and checks if they are currently active."""
    active = {}
    if not PID_FILE.exists():
        return active
    try:
        with open(PID_FILE, "r", encoding="utf-8") as f:
            for line in f:
                parts = line.strip().split(":", 1)
                if len(parts) == 2:
                    service_id, pid_str = parts[0], parts[1]
                    try:
                        pid = int(pid_str)
                        # Check if process is alive (signal 0)
                        os.kill(pid, 0)
                        active[service_id] = pid
                    except (ValueError, OSError):
                        pass
    except Exception:
        pass
    return active

def save_pids(pids_dict):
    """Saves active PIDs to PID file."""
    with open(PID_FILE, "w", encoding="utf-8") as f:
        for sid, pid in pids_dict.items():
            f.write(f"{sid}:{pid}\n")

def stop_all_servers():
    """Gracefully terminates all tracked servers."""
    pids = get_running_pids()
    if not pids:
        print("[TrayManager] No active servers found.")
        return 0
    stopped = 0
    for sid, pid in list(pids.items()):
        try:
            print(f"[TrayManager] Stopping {sid} (PID {pid})...")
            os.kill(pid, signal.SIGTERM)
            stopped += 1
        except OSError as e:
            print(f"[TrayManager] Error terminating {pid}: {e}")
    if PID_FILE.exists():
        PID_FILE.unlink()
    print(f"[TrayManager] Stopped {stopped} active server(s).")
    return stopped

def start_server(service_id):
    """Starts a managed background service."""
    if service_id not in MANAGED_SERVICES:
        print(f"[TrayManager] Unknown service: {service_id}")
        return False
    srv = MANAGED_SERVICES[service_id]
    cmd = srv["cmd"]
    log_path = REPO_ROOT / f"{service_id}.log"
    print(f"[TrayManager] Starting {srv['name']} -> logging to {log_path.name}")
    with open(log_path, "a", encoding="utf-8") as log_file:
        proc = subprocess.Popen(cmd, cwd=REPO_ROOT, stdout=log_file, stderr=subprocess.STDOUT)
    time.sleep(0.5)
    if proc.poll() is None:
        pids = get_running_pids()
        pids[service_id] = proc.pid
        save_pids(pids)
        print(f"[TrayManager] Started {service_id} (PID {proc.pid})")
        return True
    else:
        print(f"[TrayManager] Failed to start {service_id} (exited immediately with code {proc.returncode})")
        return False

def print_status():
    """Prints status of servers and ports."""
    pids = get_running_pids()
    print("==================================================")
    print("   WW3 Server & Tray Manager — Status             ")
    print("==================================================")
    for sid, info in MANAGED_SERVICES.items():
        status = f"RUNNING (PID {pids[sid]})" if sid in pids else "STOPPED"
        port_info = f"[Port {info['port']}]" if info['port'] else ""
        print(f" - {info['name']} ({sid}): {status} {port_info}")
    print("==================================================")
    print(f"Tracked active processes: {len(pids)}")
    return len(pids)

def run_tray_gui():
    """Runs system tray icon using pystray or fallback."""
    try:
        import pystray
        from PIL import Image, ImageDraw

        # Create a simple icon in memory
        def create_image():
            img = Image.new('RGB', (64, 64), color=(31, 111, 58))
            d = ImageDraw.Draw(img)
            d.rectangle((16, 16, 48, 48), fill=(255, 255, 255))
            d.text((22, 22), "W3", fill=(0, 0, 0))
            return img

        def on_stop_all(icon, item):
            stop_all_servers()

        def on_exit(icon, item):
            stop_all_servers()
            icon.stop()

        menu = pystray.Menu(
            pystray.MenuItem("WW3: Escape from North Korea", lambda: None, enabled=False),
            pystray.MenuItem("Status: Active", lambda: None, enabled=False),
            pystray.MenuItem("Quit All Servers", on_stop_all),
            pystray.MenuItem("Exit Tray Manager", on_exit)
        )

        icon = pystray.Icon("WW3_Manager", create_image(), "WW3 Server Manager", menu)
        print("[TrayManager] System tray icon active.")
        icon.run()
    except ImportError:
        print("[TrayManager] GUI libraries (pystray/PIL) not installed in this environment.")
        print("[TrayManager] Running in headless daemon monitoring mode. Use --status or --stop-all to control.")
        try:
            while True:
                time.sleep(2)
        except KeyboardInterrupt:
            print("\n[TrayManager] Terminating.")
            stop_all_servers()

def main():
    parser = argparse.ArgumentParser(description="WW3 System Tray & Background MCP Server Manager")
    parser.add_argument("--status", action="store_true", help="Print status of all managed servers")
    parser.add_argument("--stop-all", action="store_true", help="Stop and quit all running servers")
    parser.add_argument("--start", type=str, help="Start a specific server (e.g. unity-anklebreaker)")
    parser.add_argument("--daemon", action="store_true", help="Run tray manager in background loop")
    args = parser.parse_args()

    if args.status:
        print_status()
        return 0
    elif args.stop_all:
        stop_all_servers()
        return 0
    elif args.start:
        success = start_server(args.start)
        return 0 if success else 1
    else:
        # Default to tray mode (or fallback to daemon/help)
        run_tray_gui()
        return 0

if __name__ == "__main__":
    sys.exit(main())

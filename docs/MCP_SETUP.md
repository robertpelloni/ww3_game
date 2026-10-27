# MCP_SETUP — install & activate all 10 vendored servers

Vendored under `mcp-servers/unity/*` and `mcp-servers/unreal/*`.
Upstreams are source of truth — `git pull` inside each folder to update.

## Prereqs (Windows 10)
- Node 20+ (`node --version`) + npm
- Python 3.10+ (`python --version`)
- .NET 8 SDK (for IvanMurzak + C# bridges) — `dotnet --version`
- Git + Git LFS

## Unity MCPs
| ID | Folder | Install | Run |
|----|--------|---------|-----|
| U1 CoplayDev/unity-mcp | `mcp-servers/unity/CoplayDev.unity-mcp` | `npm install` in `UnityMcpServer/` | Unity Editor → add `UnityMcpBridge` package → Start, then `node UnityMcpServer/dist/index.js` or via `.mcp.json` |
| U2 IvanMurzak/Unity-MCP | `mcp-servers/unity/IvanMurzak.Unity-MCP` | See repo `README.md` — install UPM package + `dotnet build` for CLI | `unity-mcp` CLI + MCP HTTP bridge on localhost |
| U3 CoderGamester/mcp-unity | `mcp-servers/unity/CoderGamester.mcp-unity` | `npm install` | `npm run build` then stdio via `.mcp.json` |
| U4 AnkleBreaker unity-mcp-server | `mcp-servers/unity/AnkleBreaker.unity-mcp-server` | `npm install` | `npm start` (268 tools) + install matching UPM plugin in Unity |
| U5 jackwrichards/UnityMCP | `mcp-servers/unity/jackwrichards.UnityMCP` | per-repo README (Node) | stdio server |

## Unreal MCPs (research only — no local UE5 editor needed)
| ID | Folder | Install | Run |
|----|--------|---------|-----|
| R1 chongdashu/unreal-mcp | `mcp-servers/unreal/chongdashu.unreal-mcp` | `npm install` + UE plugin from repo `UnrealMcp/` | MCP stdio → talks to UE via plugin HTTP |
| R2 ChiR24/Unreal_mcp | `mcp-servers/unreal/ChiR24.Unreal_mcp` | `npm install` + build C++ Automation Bridge | TS server + C++ plugin |
| R3 kvick-games/UnrealMCP | `mcp-servers/unreal/kvick-games.UnrealMCP` | per-repo README | stdio |
| R4 GenOrca/unreal-mcp | `mcp-servers/unreal/GenOrca.unreal-mcp` | `npm install` (Python+C++ tools) | stdio |
| R5 runreal/unreal-mcp | `mcp-servers/unreal/runreal.unreal-mcp` | `pip install -r requirements.txt` | Python Remote Execution bridge |

## One-shot installer
```powershell
.\scripts\install-mcp.ps1        # npm/pip install where configs exist, no auto-start
.\scripts\install-mcp.ps1 -Start # also smoke-test stdio servers (does not launch Unity/UE)
```

## Activate in clients
- Repo-root `.mcp.json` lists all 10 (disabled by default except U1+U2 — enable as needed).
- Claude Code / Cursor / Windsurf / pi read `.mcp.json` automatically.
- VS Code MCP extension reads `.vscode/mcp.json` (same content).
- Only run **one Unity bridge at a time** (they fight over the Editor port). Recommended daily driver: **U2 IvanMurzak** + fallback **U1 Coplay**.

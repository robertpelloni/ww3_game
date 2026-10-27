# WW3: Escape from North Korea 🇰🇵🔥

**WW3 stealth-survival game — escape North Korea as World War 3 erupts.**

> Primary engine: **Unity 2022 LTS / Unity 6 LTS + URP** (laptop-friendly, runs on Intel HD 520 + dual-core).
> Research engine: **Unreal Engine 5** (docs + MCP only — too heavy for this laptop, see `docs/ENGINE_DECISION.md`).
> Built AI-first with **Model Context Protocol (MCP) servers** for Unity + Unreal.

**Repo:** https://github.com/robertpelloni/ww3_game
**Workspace submodule:** `workspace/ww3_game`
**Status:** pre-production — GDD + MCP toolchain bootstrapping

---

## Why Unity, not Unreal (on this laptop)

| | This laptop (Lenovo 20FR, i7-6600U, HD 520, 16GB, ~37GB free) | Unity URP | Unreal 5 |
|---|---|---|---|
| CPU (2C/4T 15W) | ✅ Unity editor usable | ✅ | ❌ hours of shader compiles |
| GPU (HD 520, 1GB) | ✅ URP low-poly / stylized OK | ✅ | ❌ no Lumen/Nanite, 5-10fps editor |
| Disk (~37GB free) | ✅ Unity ~15-20GB | ✅ | ❌ UE5 needs 60-100GB+ |
| MCP ecosystem | ✅ mature | ✅ 15k⭐ options | ⚠️ younger, heavier |

Full analysis: [`docs/ENGINE_DECISION.md`](docs/ENGINE_DECISION.md)

**Verdict: build the game in Unity. Use Unreal MCPs for research/reference only.**

---

## Game pitch (1-min)

**Title (working):** *WW3: DMZ Run / Escape from North Korea*

WW3 breaks out. You are a civilian in Chongjin with 72 hours before the northern front collapses. No guns-blazing — you **sneak, barter, hide, and run**: avoid patrols, manage hunger/cold/stamina, forge papers, bribe guards, cross the Tumen river into China, then the DMZ to freedom.

Pillars:
1. **Stealth over shooting** — cone-of-vision, noise, light/shadow, curfew
2. **Survival pressure** — hunger, cold, stamina, medicine, batteries
3. **Choices matter** — papers vs bribe vs river vs tunnel, who you trust
4. **Low-spec stylized** — night fog + searchlights carry the mood, not poly count

See [`docs/GAME_DESIGN.md`](docs/GAME_DESIGN.md) for acts, mechanics, and vertical-slice plan.

---

## Repo layout

```
ww3_game/
├── README.md                 # you are here
├── docs/
│   ├── ENGINE_DECISION.md    # Unity vs Unreal on this laptop
│   ├── GAME_DESIGN.md        # GDD v0.1
│   └── MCP_SETUP.md          # how to install/activate all MCP servers
├── mcp-servers/
│   ├── unity/                # top-5 Unity MCP servers (cloned)
│   └── unreal/               # top-5 Unreal MCP servers (cloned)
├── unity-project/            # Unity URP project goes here (Unity Hub → open this folder)
├── prototypes/               # pure C# / console prototypes (no engine needed)
├── .mcp.json                 # MCP client config (Claude Code / Cursor / pi)
├── .vscode/
│   └── mcp.json              # VS Code MCP config (copy of .mcp.json)
└── .gitignore
```

---

## Quickstart (Windows 10, low-spec)

### 1. Install Unity (do this first)
1. Install **Unity Hub** → https://unity.com/download
2. In Hub → Installs → install **Unity 2022.3 LTS** (most stable on HD 520) with modules:
   - `Windows Build Support (IL2CPP)`
   - `Windows Dedicated Server` (optional)
   - Documentation
3. In this repo: Hub → Open → select `unity-project/` (create it as **3D (URP)** template if empty)
4. Open `Edit → Project Settings → Quality` → set default to **Very Low**, URP Asset → disable shadows / SSAO for now

> Don't install Unreal Engine on this laptop unless you free 80GB+ and accept 5fps editor. Research via MCP servers instead.

### 2. Activate MCP servers
All 10 servers are vendored under `mcp-servers/` (see table below). Install once:

```powershell
# from repo root
.\scripts\install-mcp.ps1
```

Then point your AI client at `.mcp.json`:
- **Claude Code / Cursor / Windsurf:** auto-reads `.mcp.json` in repo root
- **VS Code (MCP extension):** uses `.vscode/mcp.json`
- **pi agent:** `hypernexus_tool_search "unity mcp"` or import `.mcp.json`

Full per-server build/run notes: [`docs/MCP_SETUP.md`](docs/MCP_SETUP.md)

### 3. Run the no-engine prototype
```powershell
dotnet run --project prototypes/StealthSim
# top-down console stealth sim: patrol cones, noise, light — no GPU needed
```

---

## MCP servers (top 5 + top 5, by ⭐)

### Unity — installed & activated
| # | Repo | ⭐ | Why |
|---|------|----|-----|
| U1 | [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) | ~15.4k | Biggest community, bridge LLM ↔ Unity Editor |
| U2 | [IvanMurzak/Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) | ~4.5k | CLI + tools + skills, best docs, free |
| U3 | [CoderGamester/mcp-unity](https://github.com/CoderGamester/mcp-unity) | ~2k | Clean Cursor/Claude Code plugin |
| U4 | [AnkleBreaker-Studio/unity-mcp-server](https://github.com/AnkleBreaker-Studio/unity-mcp-server) | ~500 | 268 tools: scenes, builds, NavMesh, multiplayer |
| U5 | [jackwrichards/UnityMCP](https://github.com/jackwrichards/UnityMCP) | ~500 | Lightweight alternative |

### Unreal — research only (don't run UE5 editor on this laptop)
| # | Repo | ⭐ | Why |
|---|------|----|-----|
| R1 | [chongdashu/unreal-mcp](https://github.com/chongdashu/unreal-mcp) | ~2.1k | #1 Unreal MCP, natural-language control |
| R2 | [ChiR24/Unreal_mcp](https://github.com/ChiR24/Unreal_mcp) | ~980 | TS + C++ Automation Bridge, comprehensive |
| R3 | [kvick-games/UnrealMCP](https://github.com/kvick-games/UnrealMCP) | ~600 | Straightforward AI-agent control |
| R4 | [GenOrca/unreal-mcp](https://github.com/GenOrca/unreal-mcp) | ~140 | UE5 + Claude & agents, Python+C++ tools |
| R5 | [runreal/unreal-mcp](https://github.com/runreal/unreal-mcp) | ~110 | Uses Unreal Python Remote Execution |

Bonus research: `IvanMurzak/Unreal-MCP` (same author as U2), `sam-david/unreal-mcp` (127 tools, no C++ plugin required — interesting for weak PCs).

> Vendored copies live in `mcp-servers/unity/*` and `mcp-servers/unreal/*`. Upstreams remain the source of truth — pull to update.

---

## Roadmap

- [x] Repo + submodule + README + MCP toolchain
- [ ] `docs/GAME_DESIGN.md` Act 1 vertical slice (Chongjin night escape)
- [ ] Unity URP project scaffold (URP asset, input, stealth toolkit)
- [ ] `prototypes/StealthSim` console sim (patrol AI, noise/light)
- [ ] First playable: 1 street, 2 patrols, 1 checkpoint, win = reach safehouse
- [ ] Itch.io private playtest build (Windows x64, Very Low preset)

---

## Contributing / agents

- Workspace protocol: see `../AGENTS.md` + `../HANDOFF.md`
- Memory: Brain `.memory/main.md` + HyperNexus L2 (`hypernexus_memory_search "ww3_game"`)
- When you finish a turn, update `HANDOFF.md` (root) and push **both** submodule + superproject

```powershell
# typical loop (run inside workspace/ww3_game for game changes)
git add -A; git commit -m "ww3: <what>"; git push origin main
cd ..; git add ww3_game; git commit -m "ww3_game: <what>"; git push
```

## License

MIT (game code + docs). Vendored MCP servers keep their own licenses — check each `mcp-servers/*/*/LICENSE`.

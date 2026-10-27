# ENGINE_DECISION — Unity vs Unreal on Lenovo 20FR (i7-6600U, HD 520, 16GB, ~37GB free)

Date: 2026-10-27. Machine: Lenovo 20FRS0QL00, Win10 Pro x64, i7-6600U (2C/4T, 2.6-2.8GHz, 15W),
Intel HD Graphics 520 (1GB shared), 15.8GB RAM, C: 511GB with ~37GB free.

## Verdict: Unity URP wins. Unreal 5 is not viable on this laptop.

### Minimums vs reality
- **UE 5.5+ recommended:** 8-core CPU, 32GB RAM, RTX w/ 8GB VRAM, 100GB+ SSD, DX12.
  Lumen + Nanite + Virtual Shadow Maps assume a discrete GPU. Shader compile on first
  launch = tens of thousands of shaders; on a 2C U-series that is hours + thermal throttle.
- **Unity 2022 LTS minimum:** dual-core x64, 4GB RAM, DX10 GPU, ~15GB disk.
  URP + Very Low quality preset runs on HD 520 at 30-60fps for low-poly/stylized scenes.

### What this means for WW3: Escape from North Korea
- This game wants **night stealth, fog, searchlights, top-down/3rd-person** — mood comes from
  lighting design, not photorealism. URP can do that on integrated graphics.
- Target art: low-poly + baked lightmaps + one realtime directional/spot, no SSAO, no realtime GI.
- Target spec: 1280x720, Very Low, 30fps on HD 520. If it runs here, it runs everywhere.
- Unreal would force you to fight the engine instead of building the game.

### If you insist on Unreal later
- Free 80GB+, use **UE 4.27 + Forward Renderer**, disable Lumen/Nanite, `r.ScreenPercentage 50`,
  no ray tracing. Still expect 10-20fps editor on HD 520. Use Unreal MCPs for
  code/blueprint generation on a stronger machine or cloud box, not local editor.
- Recommended path: prototype + ship v1 in Unity; port hero scenes to UE5 later for a
  "Definitive Edition" trailer on better hardware.

### Chosen stack
- **Unity 2022.3 LTS + URP**, C#, Input System, NavMesh, Cinemachine (light use)
- VS Code + Unity extension (lighter than full VS2022 on 2 cores)
- Git LFS for .psd/.fbx/.wav (enable after first import)
- MCP-first workflow: CoplayDev/unity-mcp + IvanMurzak/Unity-MCP drive scene/script work

#requires -Version 5.1
<#
.SYNOPSIS
  One-command repo health: MCP server smokes + doc-link check. Wrapper only —
  originals (mcp-call.js, mcp-session.js, mcp-run.py, install-mcp.ps1) untouched.
.EXAMPLE
  .\scripts\run-all-checks.ps1
#>
param([switch]$SkipUnity)
$root = Split-Path -Parent $PSScriptRoot
$fail = 0
function Check($Name, $Cmd) {
  Write-Host "`n== $Name" -ForegroundColor Cyan
  & $Cmd[0] $Cmd[1..($Cmd.Count - 1)]
  if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: $Name" -ForegroundColor Red; $script:fail++ }
  else { Write-Host "OK: $Name" -ForegroundColor Green }
}
Push-Location $root
# 1. Doc links referenced by dashboard/governance must exist.
@('README.md','VERSION.md','VISION.md','ROADMAP.md','TODO.md','MEMORY.md',
  'DEPLOY.md','IDEAS.md','CHANGELOG.md','HANDOFF.md','dashboard.html',
  'docs/GAME_DESIGN.md','docs/ENGINE_DECISION.md','docs/MCP_SETUP.md',
  'docs/MCP_TESTS.md','scripts/mcp-call.js','scripts/mcp-session.js',
  'scripts/mcp-run.py','scripts/install-mcp.ps1') | ForEach-Object {
  if (-not (Test-Path $_)) { Write-Host "MISSING: $_" -ForegroundColor Red; $fail++ }
}
# 2. MCP protocol smokes (no engine needed; graceful offline proof).
# NOTE: embedded double quotes must be backslash-escaped - PowerShell 5.1
# passes them raw to the native node process and MSVCRT argv parsing strips
# bare quotes (JSON.parse then fails - seen 2026-10-31).
Check 'Godot G1 version' @('node','scripts/mcp-call.js','.',
  'node','mcp-servers/godot/Coding-Solo.godot-mcp/build/index.js',
  '--','tools/call','{\"name\": \"get_godot_version\", \"arguments\": {}}')
# NOTE: mcp-call.js allows 25s per call; cold starts on this dual-core box can
# exceed that (AnkleBreaker needs 60s+ cold). Failures here are warnings only -
# retry slow servers via scripts/mcp-run.py (60s/120s timeouts, file params).
Write-Host '== Unreal R8 (research-only; slow to boot on dual-core - warn only)' -ForegroundColor Cyan
node scripts/mcp-call.js . node mcp-servers/unreal/sam-david.unreal-mcp/dist/bin.js -- 'tools/list' '{ }'
if ($LASTEXITCODE -ne 0) { Write-Host 'NOTE: Unreal bridge slow/offline - research-only, non-blocking.' -ForegroundColor Yellow }
if (-not $SkipUnity) {
  Write-Host "`n== Unity AnkleBreaker (needs editor on :7890; fails gracefully offline)" -ForegroundColor Cyan
  node scripts/mcp-call.js . node mcp-servers/unity/AnkleBreaker.unity-mcp-server/src/index.js -- tools/list '{}'
  if ($LASTEXITCODE -ne 0) { Write-Host 'NOTE: Unity bridge offline - start unity-test/ in Hub.' -ForegroundColor Yellow }
}
Pop-Location
if ($fail -eq 0) { Write-Host "`nAll checks passed." -ForegroundColor Green } else { exit 1 }

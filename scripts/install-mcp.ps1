#requires -Version 5.1
param([switch]$Start)
# Installs deps for all vendored MCP servers (Node npm + Python pip where applicable).
$root = Split-Path -Parent $PSScriptRoot
$failed = @()
function Install-Node($dir) {
  if (Test-Path (Join-Path $dir 'package.json')) {
    Write-Host "`n== npm install: $dir" -ForegroundColor Cyan
    Push-Location $dir
    npm install --no-audit --no-fund
    if ($LASTEXITCODE -ne 0) { $script:failed += $dir }
    Pop-Location
  }
}
function Install-Python($dir) {
  $req = Join-Path $dir 'requirements.txt'
  if (Test-Path $req) {
    Write-Host "`n== pip install: $dir" -ForegroundColor Cyan
    pip install -r $req
    if ($LASTEXITCODE -ne 0) { $script:failed += $dir }
  }
}
Get-ChildItem -Directory (Join-Path $root 'mcp-servers/unity') -ErrorAction SilentlyContinue | ForEach-Object {
  Install-Node $_.FullName
  Install-Node (Join-Path $_.FullName 'UnityMcpServer')
  Install-Python $_.FullName
}
Get-ChildItem -Directory (Join-Path $root 'mcp-servers/unreal') -ErrorAction SilentlyContinue | ForEach-Object {
  Install-Node $_.FullName
  Install-Python $_.FullName
}
if ($failed.Count -eq 0) { Write-Host "`nAll MCP deps installed OK." -ForegroundColor Green }
else { Write-Host ("`nFailed: " + ($failed -join ', ')) -ForegroundColor Red; exit 1 }

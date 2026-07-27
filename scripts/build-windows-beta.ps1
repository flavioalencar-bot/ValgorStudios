# Build da Beta Técnica 0.1 (Windows 64) — LEGADO.
# Preferir: scripts/build-windows-checkpoint.ps1
# Fonte: SOMENTE client/ (nunca builds/_unity-beta-project).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
Write-Host "AVISO: Este script é legado. Use scripts/build-windows-checkpoint.ps1"
& "$PSScriptRoot\build-windows-checkpoint.ps1" -UnityExe $UnityExe
exit $LASTEXITCODE

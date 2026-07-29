# E2E Unity — Dragão Fase 2 P1 (Nv.1→30)
param(
  [string]$Exe = "",
  [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($Exe)) {
  $Exe = Join-Path $root "builds\windows\Valgor-QA-Dragon-Phase2\Valgor.exe"
}
$evidence = Join-Path $root "docs\releases\dragon-phase2-p1-evidence"
$log = Join-Path $evidence "e2e-player.log"

New-Item -ItemType Directory -Force -Path $evidence | Out-Null

if (-not $SkipBuild) {
  & (Join-Path $root "scripts\build-qa-dragon-phase2.ps1")
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

if (-not (Test-Path $Exe)) { throw "Exe ausente: $Exe" }

Get-Process -Name "Valgor" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Host "E2E Dragon P1: $Exe -dragonPhase2E2E"
if (Test-Path $log) { Remove-Item $log -Force }

$proc = Start-Process -FilePath $Exe -ArgumentList @(
  "-cityProgressionQA",
  "-dragonPhase2E2E",
  "-logFile", $log
) -PassThru -WorkingDirectory (Split-Path $Exe)

$deadline = (Get-Date).AddMinutes(25)
do {
  Start-Sleep -Seconds 5
  if ($proc.HasExited) { break }
} while ((Get-Date) -lt $deadline)

if (-not $proc.HasExited) {
  Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
  Write-Host "TIMEOUT E2E"
  exit 1
}

$report = Join-Path $evidence "e2e-report.txt"
$ok = (Test-Path $report) -and (Select-String -Path $report -Pattern "PASS Nv.30" -Quiet)
Write-Host "exit=$($proc.ExitCode) reportOk=$ok"
if (Test-Path $report) { Get-Content $report -Tail 40 }
if ($proc.ExitCode -ne 0 -or -not $ok) { exit 1 }
Write-Host "OK E2E Dragon P1"
exit 0

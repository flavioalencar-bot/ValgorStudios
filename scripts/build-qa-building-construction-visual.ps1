# Build Windows QA Building Construction Visual (andaimes + timer + poeira).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor,
  [switch]$SkipAutoTest
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\qa-building-construction-visual-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-QA-Building-Construction-Visual"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"
$preserve = @(
  (Join-Path $root "builds\windows\Valgor-QA-Building-Upgrade-UX\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-QA-Building-Upgrade-Visual\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-QA-City-Progression-Smooth\Valgor.exe")
)

New-Item -ItemType Directory -Force -Path (Split-Path $log), $outDir | Out-Null
if (-not (Test-Path $UnityExe)) { throw "Unity nao encontrado: $UnityExe" }

function Stop-UnityAll {
  Get-Process -Name "Unity","Unity.ILPP.Runner","UnityCrashHandler64","UnityPackageManager","UnityAutoQuitter" -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 4
  Remove-Item (Join-Path $project "Temp\UnityLockfile") -Force -ErrorAction SilentlyContinue
}

function Invoke-UnityMethod([string]$method, [string]$logFile, [int]$timeoutMin = 45) {
  if (Test-Path $logFile) { Remove-Item $logFile -Force }
  $before = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
  $argsList = @("-batchmode","-nographics","-quit","-projectPath",$project,"-executeMethod",$method,"-logFile",$logFile)
  $proc = Start-Process -FilePath $UnityExe -ArgumentList $argsList -PassThru -WindowStyle Hidden
  $deadline = (Get-Date).AddMinutes($timeoutMin)
  do {
    Start-Sleep -Seconds 5
    $alive = $false
    try { $alive = -not $proc.HasExited } catch { $alive = $false }
    $extra = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Where-Object { $before -notcontains $_.Id })
    if (-not $alive -and $extra.Count -eq 0) { break }
  } while ((Get-Date) -lt $deadline)
  Start-Sleep -Seconds 2
  try { if ($proc.HasExited) { return $proc.ExitCode } } catch {}
  return 1
}

if (-not $SkipCloseEditor) { Stop-UnityAll }

$snaps = @{}
foreach ($p in $preserve) {
  if (Test-Path $p) {
    $item = Get-Item $p
    $snaps[$p] = @{ Length = $item.Length; Time = $item.LastWriteTimeUtc }
  }
}

if (Test-Path $exe) { Remove-Item $exe -Force }
if (Test-Path $dataDir) { Remove-Item $dataDir -Recurse -Force }

$buildStart = Get-Date
Write-Host "Building QA Building Construction Visual -> $exe"
$code = Invoke-UnityMethod "Valgor.Editor.QaCityProgressionWindowsBuild.BuildConstructionVisualCli" $log 45
$logOk = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "Build Successful|Construction Visual Build OK" -Quiet)
$exeFresh = (Test-Path $exe) -and ((Get-Item $exe).LastWriteTime -gt $buildStart)

foreach ($p in $snaps.Keys) {
  $now = Get-Item $p
  if ($now.Length -ne $snaps[$p].Length -or $now.LastWriteTimeUtc -ne $snaps[$p].Time) {
    Write-Host "ERRO: build congelada alterada: $p"
    exit 1
  }
}

if (-not $logOk -or -not $exeFresh) {
  Write-Host "Build falhou (exit=$code logOk=$logOk exeFresh=$exeFresh). Log: $log"
  if (Test-Path $log) { Get-Content $log -Tail 160 }
  exit 1
}

Write-Host "OK: $exe"

if (-not $SkipAutoTest) {
  $evidence = Join-Path $root "docs\releases\building-construction-visual-evidence"
  $report = Join-Path $evidence "auto-test-report.txt"
  New-Item -ItemType Directory -Force -Path $evidence | Out-Null
  if (Test-Path $report) { Remove-Item $report -Force }

  Write-Host "Auto-test construction visual (UX flag + capture during upgrade)..."
  Get-Process -Name Valgor -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 1

  $uxEvidence = Join-Path $root "docs\releases\building-upgrade-ux-evidence"
  $uxReport = Join-Path $uxEvidence "auto-test-report.txt"
  if (Test-Path $uxReport) { Remove-Item $uxReport -Force }

  $proc = Start-Process -FilePath $exe -ArgumentList @("-buildingUpgradeUxTest","-screen-width","1600","-screen-height","900") -WorkingDirectory $outDir -PassThru
  $deadline = (Get-Date).AddMinutes(35)
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 5
    if ($proc.HasExited) { break }
    if (Test-Path $uxReport) { Start-Sleep -Seconds 4; break }
  }
  if (-not $proc.HasExited) { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue }
  if (-not (Test-Path $uxReport)) {
    Write-Host "AutoTest report missing"
    exit 1
  }

  Copy-Item (Join-Path $uxEvidence "11-upgrade-started.png") (Join-Path $evidence "01-construction-in-progress.png") -Force -ErrorAction SilentlyContinue
  Copy-Item (Join-Path $uxEvidence "12-upgrade-completed.png") (Join-Path $evidence "02-construction-complete.png") -Force -ErrorAction SilentlyContinue
  Copy-Item (Join-Path $uxEvidence "13-tier-swap.png") (Join-Path $evidence "03-tier-after-build.png") -Force -ErrorAction SilentlyContinue
  $header = "Building Construction Visual - mirrored upgrade captures + UX report"
  $body = Get-Content $uxReport -Raw
  Set-Content -Path $report -Value ($header + "`r`n" + $body) -Encoding UTF8
  Get-Content $report
  if (Select-String -Path $report -Pattern "\[FAIL\]" -Quiet) { exit 1 }
  Write-Host "AutoTest OK"
}

exit 0

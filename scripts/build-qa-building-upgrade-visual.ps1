# Build Windows QA Building Upgrade Visual (polish de modais + preview 3D).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor,
  [switch]$SkipAutoTest
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\qa-building-upgrade-visual-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-QA-Building-Upgrade-Visual"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"
$preserve = @(
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.4\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-QA-City-Progression\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-QA-City-Progression-Smooth\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-QA-Building-Upgrade-UX\Valgor.exe")
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
Write-Host "Building QA Building Upgrade Visual -> $exe"
$code = Invoke-UnityMethod "Valgor.Editor.QaCityProgressionWindowsBuild.BuildUpgradeVisualCli" $log 45
$logOk = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "Build Successful|Upgrade Visual Build OK" -Quiet)
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
  $evidence = Join-Path $root "docs\releases\building-upgrade-visual-evidence"
  $report = Join-Path $evidence "auto-test-report.txt"
  if (Test-Path $report) { Remove-Item $report -Force }
  New-Item -ItemType Directory -Force -Path $evidence | Out-Null

  Write-Host "Auto-test building upgrade visual (reuse UX test flag)..."
  Get-Process -Name Valgor -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 1
  # Reusa o auto-teste UX; evidencia vai para pasta UX — copiamos depois.
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

  Copy-Item (Join-Path $uxEvidence "*") $evidence -Force
  "Building Upgrade Visual polish — mirrored from UX auto-test`n$(Get-Content $uxReport -Raw)" |
    Set-Content $report -Encoding UTF8
  Get-Content $report
  if (Select-String -Path $report -Pattern "\[FAIL\]" -Quiet) { exit 1 }
  Write-Host "AutoTest OK"
}

exit 0

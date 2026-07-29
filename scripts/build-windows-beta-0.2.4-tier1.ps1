# Build Windows Beta 0.2.4-Tier1 — Castelo Tier1 somente (glTFast GLB).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\beta-0.2.4-tier1-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-Beta-0.2.4-Tier1"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"
$preserve = @(
  (Join-Path $root "builds\windows\Valgor-Beta-0.1\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.1\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.2\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.3\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.4\Valgor.exe")
)

New-Item -ItemType Directory -Force -Path (Split-Path $log), $outDir | Out-Null
if (-not (Test-Path $UnityExe)) { throw "Unity nao encontrado: $UnityExe" }

function Stop-UnityAll {
  Get-Process -Name "Unity","Unity.ILPP.Runner","UnityCrashHandler64","UnityPackageManager","UnityAutoQuitter" -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 4
  Remove-Item (Join-Path $project "Temp\UnityLockfile") -Force -ErrorAction SilentlyContinue
}

function Invoke-UnityMethod([string]$method, [string]$logFile, [int]$timeoutMin = 25, [switch]$WithGraphics) {
  if (Test-Path $logFile) { Remove-Item $logFile -Force }
  $before = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
  $argsList = @("-batchmode","-quit","-projectPath",$project,"-executeMethod",$method,"-logFile",$logFile)
  if (-not $WithGraphics) { $argsList = @("-nographics") + $argsList }
  $proc = Start-Process -FilePath $UnityExe -ArgumentList $argsList -PassThru -WindowStyle Hidden
  $deadline = (Get-Date).AddMinutes($timeoutMin)
  do {
    Start-Sleep -Seconds 4
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
Write-Host "Tier1 pipeline (prefab+scene+capture)..."
$tierLog = Join-Path $root "builds\windows\beta-0.2.4-tier1-pipeline.log"
$code = Invoke-UnityMethod "Valgor.Editor.CastleTier1OnlyPipeline.BuildCli" $tierLog 25 -WithGraphics
$tierOk = (Test-Path $tierLog) -and [bool](Select-String -Path $tierLog -Pattern "Castle Tier1-only OK" -Quiet)
if (-not $tierOk) {
  Write-Host "Falha Tier1 pipeline. Log: $tierLog"
  if (Test-Path $tierLog) { Get-Content $tierLog -Tail 80 }
  exit 1
}
Write-Host "Tier1 pipeline OK"

Stop-UnityAll
Write-Host "Player build -> $exe"
$code = Invoke-UnityMethod "Valgor.Editor.BetaWindowsBuild.BuildCli" $log 35
$logOk = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "Build Successful|\[Valgor\] Build OK" -Quiet)
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
  if (Test-Path $log) { Get-Content $log -Tail 80 }
  exit 1
}

Write-Host "OK: $exe"
Get-Item $exe | Format-List FullName, Length, LastWriteTime
exit 0

# Build Windows QA Homolog Dragão Fases 1–4 (base fbee9bc worktree).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [string]$Project = "C:\Valgor_Studio_phase4_homolog\client",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = "C:\Valgor_Studio_phase4_homolog"
$log = Join-Path $root "builds\windows\qa-dragon-phase4-homolog-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-QA-Dragon-Phase4-Homolog"
$exe = Join-Path $outDir "Valgor.exe"

New-Item -ItemType Directory -Force -Path (Split-Path $log), $outDir | Out-Null
if (-not (Test-Path $UnityExe)) { throw "Unity nao encontrado: $UnityExe" }

function Stop-UnityAll {
  Get-Process -Name "Unity","Unity.ILPP.Runner","UnityCrashHandler64","UnityPackageManager","UnityAutoQuitter" -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 4
  Remove-Item (Join-Path $Project "Temp\UnityLockfile") -Force -ErrorAction SilentlyContinue
}

function Invoke-UnityMethod([string]$method, [string]$logFile, [int]$timeoutMin = 45) {
  if (Test-Path $logFile) { Remove-Item $logFile -Force }
  $before = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
  $argsList = @("-batchmode","-nographics","-quit","-projectPath",$Project,"-executeMethod",$method,"-logFile",$logFile)
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
if (Test-Path $exe) { Remove-Item $exe -Force }

$buildStart = Get-Date
Write-Host "Building QA Dragon Phase4 Homolog -> $exe"
$code = Invoke-UnityMethod "Valgor.Editor.QaCityProgressionWindowsBuild.BuildDragonPhase4HomologCli" $log 45
$logOk = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "Build Successful|Phase4 Homolog Build OK" -Quiet)
$exeFresh = (Test-Path $exe) -and ((Get-Item $exe).LastWriteTime -gt $buildStart)

if (-not $logOk -or -not $exeFresh) {
  Write-Host "Build falhou (exit=$code logOk=$logOk exeFresh=$exeFresh). Log: $log"
  if (Test-Path $log) { Get-Content $log -Tail 80 }
  exit 1
}

Write-Host "OK: $exe"
exit 0

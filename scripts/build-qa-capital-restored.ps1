# Build Windows QA — Capital Restored (World Foundation OFF, Tripo=0).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\qa-capital-restored-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-QA-Capital-Restored"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"

New-Item -ItemType Directory -Force -Path (Split-Path $log), $outDir | Out-Null
if (-not (Test-Path $UnityExe)) { throw "Unity nao encontrado: $UnityExe" }

function Stop-UnityAll {
  Get-Process -Name "Unity","Unity.ILPP.Runner","UnityCrashHandler64","UnityPackageManager","UnityAutoQuitter" -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 4
  Remove-Item (Join-Path $project "Temp\UnityLockfile") -Force -ErrorAction SilentlyContinue
}

function Invoke-UnityMethod([string]$method, [string]$logFile, [int]$timeoutMin = 55) {
  if (Test-Path $logFile) { Remove-Item $logFile -Force }
  $before = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
  $argsList = @(
    "-batchmode","-nographics","-quit","-projectPath",$project,
    "-executeMethod",$method,"-logFile",$logFile
  )
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
if (Test-Path $dataDir) { Remove-Item $dataDir -Recurse -Force }

$buildStart = Get-Date
Write-Host "Building QA Capital Restored -> $exe (Tripo API=0, World Foundation OFF)"
$code = Invoke-UnityMethod "Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalRestoredCli" $log 55
$logOk = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "Build Successful|Capital Restored Build OK" -Quiet)
$exeFresh = (Test-Path $exe) -and ((Get-Item $exe).LastWriteTime -gt $buildStart)

if (-not $logOk -or -not $exeFresh) {
  Write-Host "BUILD FAIL. log=$log"
  if (Test-Path $log) { Get-Content $log -Tail 50 }
  exit 1
}

Write-Host "OK: $exe"
Write-Host "Tripo API calls: 0 | tokens: 0 | jobs: 0 | retries: 0"
exit 0

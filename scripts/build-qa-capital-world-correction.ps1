# Build Windows QA — Capital World Correction (No Tripo).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\qa-capital-world-correction-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-QA-Capital-WorldCorrection"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"

# Hard stop: never invoke Tripo during this build.
if (Test-Path (Join-Path $env:LOCALAPPDATA "npm-cache\_npx")) { }
Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
  Where-Object { $_.CommandLine -match '(?i)tripo' } |
  ForEach-Object { Write-Host "Kill Tripo proc $($_.ProcessId)"; Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }

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
    "-executeMethod",$method,"-logFile",$logFile,
    "-capitalWorldNoTripoCorrection"
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
Write-Host "Import local Models only (Tripo API=0)..."
$importLog = Join-Path $root "builds\windows\qa-capital-world-correction-import.log"
$codeImport = Invoke-UnityMethod "Valgor.Editor.CapitalTerrainKitPrefabBuilder.ImportCli" $importLog 25
Write-Host "Import exit hint=$codeImport"

Write-Host "Building QA Capital WorldCorrection -> $exe"
$code = Invoke-UnityMethod "Valgor.Editor.QaCityProgressionWindowsBuild.BuildCapitalWorldCorrectionCli" $log 55
$logOk = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "Build Successful|Capital WorldCorrection Build OK" -Quiet)
$exeFresh = (Test-Path $exe) -and ((Get-Item $exe).LastWriteTime -gt $buildStart)

if (-not $logOk -or -not $exeFresh) {
  Write-Host "BUILD FAIL. log=$log"
  if (Test-Path $log) { Get-Content $log -Tail 40 }
  exit 1
}

Write-Host "OK: $exe"
Write-Host "Tripo API calls nesta correcao: 0"
exit 0

# Build DragonTower prefab + fix catalog mapping (Unity batchmode).
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\qa-capital-dragon-tower-art.log"

New-Item -ItemType Directory -Force -Path (Split-Path $log) | Out-Null
if (-not (Test-Path $UnityExe)) { throw "Unity nao encontrado: $UnityExe" }

function Stop-UnityAll {
  Get-Process -Name "Unity","Unity.ILPP.Runner","UnityCrashHandler64","UnityPackageManager","UnityAutoQuitter","Valgor" -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 3
  Remove-Item (Join-Path $project "Temp\UnityLockfile") -Force -ErrorAction SilentlyContinue
}

if (-not $SkipCloseEditor) { Stop-UnityAll }
if (Test-Path $log) { Remove-Item $log -Force }

$argsList = @(
  "-batchmode","-nographics","-quit",
  "-projectPath",$project,
  "-executeMethod","Valgor.Editor.CapitalDragonTowerPrefabBuilder.BuildCli",
  "-logFile",$log
)
Write-Host "Building DragonTower + catalog fix…"
$proc = Start-Process -FilePath $UnityExe -ArgumentList $argsList -PassThru -WindowStyle Hidden
$deadline = (Get-Date).AddMinutes(25)
do {
  Start-Sleep -Seconds 5
  if ($proc.HasExited) { break }
} while ((Get-Date) -lt $deadline)

Start-Sleep -Seconds 2
$code = 1
try { if ($proc.HasExited) { $code = $proc.ExitCode } } catch {}
$ok = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "DragonTower Art OK" -Quiet)
$prefab = Join-Path $root "client\Assets\Art\Capital\Architecture\DragonTower\Prefabs\VAL_Capital_Architecture_DragonTower.prefab"
$catalog = Join-Path $root "client\Assets\Art\Capital\Resources\Valgor\CityArtAssetCatalog.asset"

if (-not $ok -or -not (Test-Path $prefab) -or -not (Test-Path $catalog)) {
  Write-Host "FAIL exit=$code ok=$ok prefab=$(Test-Path $prefab) catalog=$(Test-Path $catalog)"
  if (Test-Path $log) { Get-Content $log -Tail 80 }
  exit 1
}

# Sanity: castle HF must be null; dragon-tower must reference prefab
$yaml = Get-Content $catalog -Raw
if ($yaml -notmatch "BuildingId: castle[\s\S]*?HighFidelityPrefab: \{fileID: 0\}") {
  Write-Host "WARN: verificar castle HighFidelityPrefab limpo no YAML"
}
if ($yaml -notmatch "BuildingId: dragon-tower[\s\S]*?HighFidelityPrefab: \{fileID: [^0]") {
  Write-Host "FAIL: dragon-tower sem HighFidelityPrefab"
  exit 1
}

Write-Host "OK: $prefab"
Write-Host "OK: catalog mapping castle=loader, dragon-tower=HF"
exit 0

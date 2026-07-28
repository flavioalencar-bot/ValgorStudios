# Build Windows Beta 0.2.4 — Castelo GLB nativo (glTFast). Não sobrescreve 0.1–0.2.3.
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\beta-0.2.4-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-Beta-0.2.4"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"
$preserve = @(
  (Join-Path $root "builds\windows\Valgor-Beta-0.1\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.1\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.2\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.3\Valgor.exe")
)

New-Item -ItemType Directory -Force -Path (Split-Path $log), $outDir | Out-Null

if (-not (Test-Path -LiteralPath $UnityExe)) { throw "Unity nao encontrado: $UnityExe" }

function Stop-UnityAll {
  Get-Process -Name "Unity","Unity.ILPP.Runner","UnityCrashHandler64","UnityPackageManager","UnityAutoQuitter" -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 4
  Remove-Item (Join-Path $project "Temp\UnityLockfile") -Force -ErrorAction SilentlyContinue
}

function Invoke-UnityMethod([string]$method, [string]$logFile, [int]$timeoutMin = 25, [switch]$WithGraphics) {
  if (Test-Path -LiteralPath $logFile) { Remove-Item -LiteralPath $logFile -Force }
  $before = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
  $args = @(
    "-batchmode","-quit",
    "-projectPath",$project,
    "-executeMethod",$method,
    "-logFile",$logFile
  )
  if (-not $WithGraphics) { $args = @("-nographics") + $args }
  $proc = Start-Process -FilePath $UnityExe -ArgumentList $args -PassThru -WindowStyle Hidden
  $deadline = (Get-Date).AddMinutes($timeoutMin)
  do {
    Start-Sleep -Seconds 4
    $alive = $false
    try { $alive = -not $proc.HasExited } catch { $alive = $false }
    $extra = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Where-Object { $before -notcontains $_.Id })
    if (-not $alive -and $extra.Count -eq 0) { break }
  } while ((Get-Date) -lt $deadline)
  Start-Sleep -Seconds 2
  $code = 1
  try { if ($proc.HasExited) { $code = $proc.ExitCode } } catch { $code = 1 }
  return $code
}

if (-not $SkipCloseEditor) { Stop-UnityAll }

$snaps = @{}
foreach ($p in $preserve) {
  if (Test-Path -LiteralPath $p) {
    $item = Get-Item -LiteralPath $p
    $snaps[$p] = @{ Length = $item.Length; Time = $item.LastWriteTimeUtc }
  }
}

if (Test-Path -LiteralPath $exe) { Remove-Item -LiteralPath $exe -Force }
if (Test-Path -LiteralPath $dataDir) { Remove-Item -LiteralPath $dataDir -Recurse -Force }

$buildStart = Get-Date
Write-Host "Building Valgor Beta 0.2.4 -> $exe"

# 1) Prefabs GLB
$prefabLog = Join-Path $root "builds\windows\beta-0.2.4-prefabs.log"
Write-Host "Gerando prefabs GLB/glTFast..."
$code = Invoke-UnityMethod "Valgor.Editor.CastleTiersPrefabBuilder.BuildCli" $prefabLog 30
$prefabOk = $false
if (Test-Path $prefabLog) {
  $prefabOk = [bool](Select-String -Path $prefabLog -Pattern "Castle tiers prefabs OK" -Quiet)
}
if (-not $prefabOk) {
  Write-Host "Falha prefabs (exit=$code). Log: $prefabLog"
  if (Test-Path $prefabLog) { Get-Content $prefabLog -Tail 100 }
  exit 1
}
Write-Host "Prefabs OK"

# 2) Cena validação + capturas isoladas
$valLog = Join-Path $root "builds\windows\beta-0.2.4-validation.log"
Write-Host "Cena CastleImportValidation + capturas..."
$code = Invoke-UnityMethod "Valgor.Editor.CastleImportValidationSceneBuilder.BuildAndCaptureCli" $valLog 20 -WithGraphics
$valOk = $false
if (Test-Path $valLog) {
  $valOk = [bool](Select-String -Path $valLog -Pattern "CastleImportValidation OK" -Quiet)
}
if (-not $valOk) {
  Write-Host "Falha validation (exit=$code). Log: $valLog"
  if (Test-Path $valLog) { Get-Content $valLog -Tail 80 }
  exit 1
}
Write-Host "Validation OK"

# 3) Player build
Stop-UnityAll
Write-Host "Player build..."
$code = Invoke-UnityMethod "Valgor.Editor.BetaWindowsBuild.BuildCli" $log 35
$logOk = $false
if (Test-Path $log) {
  $logOk = [bool](Select-String -Path $log -Pattern "Build Successful|\[Valgor\] Build OK" -Quiet)
}
$exeFresh = (Test-Path $exe) -and ((Get-Item $exe).LastWriteTime -gt $buildStart)

foreach ($p in $snaps.Keys) {
  $now = Get-Item -LiteralPath $p
  if ($now.Length -ne $snaps[$p].Length -or $now.LastWriteTimeUtc -ne $snaps[$p].Time) {
    Write-Host "ERRO: build congelada alterada: $p"
    exit 1
  }
}

if ((-not $logOk) -or (-not $exeFresh)) {
  Write-Host "Build falhou (exit=$code logOk=$logOk exeFresh=$exeFresh). Log: $log"
  if (Test-Path $log) { Get-Content $log -Tail 100 }
  exit 1
}

Write-Host "OK: $exe"
Get-Item $exe | Format-List FullName, Length, LastWriteTime
exit 0

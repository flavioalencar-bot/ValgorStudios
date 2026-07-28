# Build Windows Beta 0.2.3 — NÃO sobrescreve 0.1, 0.2, 0.2.1 nem 0.2.2
# Castelo tiers 1–6 reais + troca por faixa de nível.
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\beta-0.2.3-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-Beta-0.2.3"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"
$preserve = @(
  (Join-Path $root "builds\windows\Valgor-Beta-0.1\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.1\Valgor.exe"),
  (Join-Path $root "builds\windows\Valgor-Beta-0.2.2\Valgor.exe")
)

New-Item -ItemType Directory -Force -Path (Split-Path $log) | Out-Null
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

if (-not (Test-Path -LiteralPath $UnityExe)) {
  throw "Unity nao encontrado: $UnityExe"
}

if (-not (Test-Path -LiteralPath (Join-Path $project "Assets"))) {
  throw "Projeto client invalido (sem Assets): $project"
}

if (-not $SkipCloseEditor) {
  $unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
  if ($unityProcs) {
    Write-Host "Fechando Unity Editor para liberar o projeto client..."
    $unityProcs | Stop-Process -Force
    Start-Sleep -Seconds 3
  }
}

if (Test-Path -LiteralPath (Join-Path $project "Temp\UnityLockfile")) {
  Remove-Item -LiteralPath (Join-Path $project "Temp\UnityLockfile") -Force -ErrorAction SilentlyContinue
}

$snaps = @{}
foreach ($p in $preserve) {
  if (Test-Path -LiteralPath $p) {
    $item = Get-Item -LiteralPath $p
    $snaps[$p] = @{ Length = $item.Length; Time = $item.LastWriteTimeUtc }
  }
}

if (Test-Path -LiteralPath $exe) {
  Write-Host "Removendo exe antigo 0.2.3: $exe"
  Remove-Item -LiteralPath $exe -Force
}
if (Test-Path -LiteralPath $dataDir) {
  Write-Host "Removendo Valgor_Data antigo 0.2.3: $dataDir"
  Remove-Item -LiteralPath $dataDir -Recurse -Force
}

$buildStart = Get-Date
Write-Host "Building Valgor Beta 0.2.3 -> $exe (start=$buildStart)"
Write-Host "Log: $log"
Write-Host "Preserva: Valgor-Beta-0.1, 0.2, 0.2.1, 0.2.2"

# Prefabs Resources dos 6 tiers antes do player build
$prefabLog = Join-Path $root "builds\windows\beta-0.2.3-prefabs.log"
if (Test-Path -LiteralPath $prefabLog) { Remove-Item -LiteralPath $prefabLog -Force }
Write-Host "Gerando prefabs Castle Tier1..6..."
$prefabUnityBefore = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
$prefabProc = Start-Process -FilePath $UnityExe -ArgumentList @(
  "-batchmode","-nographics","-quit",
  "-projectPath",$project,
  "-executeMethod","Valgor.Editor.CastleTiersPrefabBuilder.BuildCli",
  "-logFile",$prefabLog
) -PassThru -WindowStyle Hidden

$prefabDeadline = (Get-Date).AddMinutes(20)
do {
  Start-Sleep -Seconds 3
  $prefabAlive = $false
  try { $prefabAlive = -not $prefabProc.HasExited } catch { $prefabAlive = $false }
  $extra = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Where-Object { $prefabUnityBefore -notcontains $_.Id })
  if (-not $prefabAlive -and $extra.Count -eq 0) { break }
} while ((Get-Date) -lt $prefabDeadline)

# Aguarda flush do log
Start-Sleep -Seconds 2
$prefabCode = 1
try { if ($prefabProc.HasExited) { $prefabCode = $prefabProc.ExitCode } } catch { $prefabCode = 1 }
$prefabOk = $false
if (Test-Path -LiteralPath $prefabLog) {
  $prefabOk = [bool](Select-String -Path $prefabLog -Pattern "Castle tiers prefabs OK" -Quiet)
}
if (-not $prefabOk) {
  Write-Host "Falha ao gerar prefabs dos tiers (exit=$prefabCode prefabOk=$prefabOk). Log: $prefabLog"
  if (Test-Path -LiteralPath $prefabLog) { Get-Content -LiteralPath $prefabLog -Tail 80 }
  exit 1
}
Write-Host "Prefabs OK"

$unityBefore = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
& $UnityExe `
  -batchmode `
  -nographics `
  -quit `
  -projectPath $project `
  -executeMethod Valgor.Editor.BetaWindowsBuild.BuildCli `
  -logFile $log

$code = $LASTEXITCODE
if ($null -eq $code) { $code = 0 }

$waitDeadline = (Get-Date).AddMinutes(30)
while ((Get-Date) -lt $waitDeadline) {
  $alive = @(Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Where-Object { $unityBefore -notcontains $_.Id })
  $exeReady = (Test-Path -LiteralPath $exe) -and ((Get-Item -LiteralPath $exe).LastWriteTime -gt $buildStart)
  if ($exeReady -and $alive.Count -eq 0) { break }
  if ($alive.Count -eq 0 -and -not $exeReady) {
    Start-Sleep -Seconds 2
    if ((Test-Path -LiteralPath $exe) -and ((Get-Item -LiteralPath $exe).LastWriteTime -gt $buildStart)) { break }
    break
  }
  Start-Sleep -Seconds 5
}

$logOk = $false
if (Test-Path -LiteralPath $log) {
  $logOk = [bool](Select-String -Path $log -Pattern "Build Successful|\[Valgor\] Build OK" -Quiet)
}

$exeFresh = $false
if (Test-Path -LiteralPath $exe) {
  $exeItem = Get-Item -LiteralPath $exe
  $exeFresh = $exeItem.LastWriteTime -gt $buildStart
}

foreach ($p in $snaps.Keys) {
  $now = Get-Item -LiteralPath $p
  if ($now.Length -ne $snaps[$p].Length -or $now.LastWriteTimeUtc -ne $snaps[$p].Time) {
    Write-Host "ERRO: build congelada alterada: $p"
    exit 1
  }
}

if (($code -ne 0 -and -not $logOk) -or -not $exeFresh) {
  Write-Host "Build falhou (exit=$code logOk=$logOk exeFresh=$exeFresh). Log: $log"
  if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Tail 100 }
  exit 1
}

Write-Host "OK: $exe"
Get-Item -LiteralPath $exe | Format-List FullName, Length, LastWriteTime
exit 0

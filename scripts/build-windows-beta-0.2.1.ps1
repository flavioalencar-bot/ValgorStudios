# Build Windows Beta 0.2.1 — NÃO sobrescreve Valgor-Beta-0.1 nem Valgor-Beta-0.2
# Fecha instancias Unity que travam o projeto antes do batchmode.
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\beta-0.2.1-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-Beta-0.2.1"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"
$preserve01 = Join-Path $root "builds\windows\Valgor-Beta-0.1\Valgor.exe"
$preserve02 = Join-Path $root "builds\windows\Valgor-Beta-0.2\Valgor.exe"

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

# Snapshot sizes of frozen builds (must still exist after this script)
$snap01 = if (Test-Path -LiteralPath $preserve01) { (Get-Item -LiteralPath $preserve01).Length } else { $null }
$snap02 = if (Test-Path -LiteralPath $preserve02) { (Get-Item -LiteralPath $preserve02).Length } else { $null }
$snap02Time = if (Test-Path -LiteralPath $preserve02) { (Get-Item -LiteralPath $preserve02).LastWriteTimeUtc } else { $null }

if (Test-Path -LiteralPath $exe) {
  Write-Host "Removendo exe antigo 0.2.1: $exe"
  Remove-Item -LiteralPath $exe -Force
}
if (Test-Path -LiteralPath $dataDir) {
  Write-Host "Removendo Valgor_Data antigo 0.2.1: $dataDir"
  Remove-Item -LiteralPath $dataDir -Recurse -Force
}

$buildStart = Get-Date
Write-Host "Building Valgor Beta 0.2.1 -> $exe (start=$buildStart)"
Write-Host "Log: $log"
Write-Host "Preserva: Valgor-Beta-0.1 e Valgor-Beta-0.2"

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

$waitDeadline = (Get-Date).AddMinutes(20)
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
  if (-not $exeFresh) {
    Write-Host "Exe existe mas e stale (LastWriteTime=$($exeItem.LastWriteTime) <= buildStart=$buildStart)"
  }
}

# Guard: never mutate frozen betas
if ($null -ne $snap01 -and ((Get-Item -LiteralPath $preserve01).Length -ne $snap01)) {
  Write-Host "ERRO: Valgor-Beta-0.1 foi alterada durante o build."
  exit 1
}
if ($null -ne $snap02) {
  $now02 = Get-Item -LiteralPath $preserve02
  if ($now02.Length -ne $snap02 -or $now02.LastWriteTimeUtc -ne $snap02Time) {
    Write-Host "ERRO: Valgor-Beta-0.2 foi alterada durante o build."
    exit 1
  }
}

if (($code -ne 0 -and -not $logOk) -or -not $exeFresh) {
  Write-Host "Build falhou (exit=$code logOk=$logOk exeFresh=$exeFresh). Log: $log"
  if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Tail 80 }
  exit 1
}

Write-Host "OK: $exe"
Get-Item -LiteralPath $exe | Format-List FullName, Length, LastWriteTime
Write-Host "Preservados: 0.1=$(Test-Path $preserve01) 0.2=$(Test-Path $preserve02)"
exit 0

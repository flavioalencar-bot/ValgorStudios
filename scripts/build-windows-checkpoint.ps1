# Build Windows Checkpoint — usa SOMENTE client/
# Não usa builds/_unity-beta-project (obsoleto / sem Assets).
# Fecha instâncias Unity que travam o projeto antes do batchmode.
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\checkpoint-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-Checkpoint"
$exe = Join-Path $outDir "Valgor.exe"

New-Item -ItemType Directory -Force -Path (Split-Path $log) | Out-Null
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

if (-not (Test-Path -LiteralPath $UnityExe)) {
  throw "Unity não encontrado: $UnityExe"
}

if (-not (Test-Path -LiteralPath (Join-Path $project "Assets"))) {
  throw "Projeto client inválido (sem Assets): $project"
}

$obsolete = Join-Path $root "builds\_unity-beta-project"
if (Test-Path -LiteralPath $obsolete) {
  Write-Host "AVISO: $obsolete é OBSOLETO e NÃO será usado nesta build."
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

Write-Host "Building Valgor Checkpoint → $exe"
Write-Host "Log: $log"

& $UnityExe `
  -batchmode `
  -nographics `
  -quit `
  -projectPath $project `
  -executeMethod Valgor.Editor.BetaWindowsBuild.BuildCli `
  -logFile $log

$code = $LASTEXITCODE
if ($code -ne 0) {
  Write-Host "Build falhou (exit $code). Log: $log"
  if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Tail 80 }
  exit $code
}

if (-not (Test-Path -LiteralPath $exe)) {
  Write-Host "EXE não gerado. Log: $log"
  if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Tail 80 }
  exit 1
}

Write-Host "OK: $exe"
Get-Item -LiteralPath $exe | Format-List FullName, Length, LastWriteTime

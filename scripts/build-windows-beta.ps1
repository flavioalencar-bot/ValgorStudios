# Build Windows Beta 0.1 — usa SOMENTE client/
# Fecha instancias Unity que travam o projeto antes do batchmode.
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\beta-0.1-build.log"
$outDir = Join-Path $root "builds\windows\Valgor-Beta-0.1"
$exe = Join-Path $outDir "Valgor.exe"
$dataDir = Join-Path $outDir "Valgor_Data"

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

# Remove stale build artifacts so a leftover Valgor.exe cannot count as success.
if (Test-Path -LiteralPath $exe) {
  Write-Host "Removendo exe antigo: $exe"
  Remove-Item -LiteralPath $exe -Force
}
if (Test-Path -LiteralPath $dataDir) {
  Write-Host "Removendo Valgor_Data antigo: $dataDir"
  Remove-Item -LiteralPath $dataDir -Recurse -Force
}

$buildStart = Get-Date
Write-Host "Building Valgor Beta 0.1 -> $exe (start=$buildStart)"
Write-Host "Log: $log"

& $UnityExe `
  -batchmode `
  -nographics `
  -quit `
  -projectPath $project `
  -executeMethod Valgor.Editor.BetaWindowsBuild.BuildCli `
  -logFile $log

$code = $LASTEXITCODE
if ($null -eq $code) { $code = 0 }

# Aceita exit vazio do Unity quando o log confirma sucesso.
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

if (($code -ne 0 -and -not $logOk) -or -not $exeFresh) {
  Write-Host "Build falhou (exit=$code logOk=$logOk exeFresh=$exeFresh). Log: $log"
  if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log -Tail 80 }
  exit 1
}

Write-Host "OK: $exe"
Get-Item -LiteralPath $exe | Format-List FullName, Length, LastWriteTime
exit 0
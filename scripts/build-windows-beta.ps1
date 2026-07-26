# Build da Beta Técnica 0.1 (Windows 64).
# Feche o Unity Editor no projeto client antes de rodar.
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "client"
$log = Join-Path $root "builds\windows\beta-build.log"
$exe = Join-Path $root "builds\windows\Valgor-Beta-0.1\Valgor.exe"

New-Item -ItemType Directory -Force -Path (Split-Path $log) | Out-Null

if (-not (Test-Path $UnityExe)) {
  throw "Unity não encontrado: $UnityExe"
}

Write-Host "Building Valgor Beta Técnica 0.1..."
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
  if (Test-Path $log) { Get-Content $log -Tail 60 }
  exit $code
}

if (-not (Test-Path $exe)) {
  Write-Host "EXE não gerado. Log: $log"
  if (Test-Path $log) { Get-Content $log -Tail 60 }
  exit 1
}

Write-Host "OK: $exe"
Get-Item $exe | Format-List FullName, Length, LastWriteTime

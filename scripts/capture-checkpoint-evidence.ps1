# Roda smoke + capturas da build Checkpoint (com janela — precisa de GPU).
param(
  [string]$Exe = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if (-not $Exe) {
  $Exe = Join-Path $root "builds\windows\Valgor-Checkpoint\Valgor.exe"
}
if (-not (Test-Path -LiteralPath $Exe)) {
  throw "EXE não encontrado: $Exe"
}

$evidence = Join-Path (Split-Path $Exe) "evidence"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
Write-Host "Smoke+capturas → $Exe"
Write-Host "Evidências esperadas em: $evidence"

& $Exe -checkpointSmoke -captureEvidence -logfile (Join-Path (Split-Path $Exe) "smoke-capture.log")
$code = $LASTEXITCODE
Write-Host "Exit: $code"
if (Test-Path -LiteralPath $evidence) {
  Get-ChildItem -LiteralPath $evidence -Filter "*.png" | Format-Table Name, Length, LastWriteTime
}
exit $code

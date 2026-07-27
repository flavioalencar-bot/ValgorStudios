# Roda smoke + capturas. Prefere Beta 0.2.1; fallback 0.2; depois 0.1.
param(
  [string]$Exe = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if (-not $Exe) {
  $candidate021 = Join-Path $root "builds\windows\Valgor-Beta-0.2.1\Valgor.exe"
  $candidate02 = Join-Path $root "builds\windows\Valgor-Beta-0.2\Valgor.exe"
  $candidate01 = Join-Path $root "builds\windows\Valgor-Beta-0.1\Valgor.exe"
  if (Test-Path -LiteralPath $candidate021) { $Exe = $candidate021 }
  elseif (Test-Path -LiteralPath $candidate02) { $Exe = $candidate02 }
  else { $Exe = $candidate01 }
}
if (-not (Test-Path -LiteralPath $Exe)) {
  throw "EXE não encontrado: $Exe"
}

$evidence = Join-Path (Split-Path $Exe) "evidence"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
Write-Host "Smoke+capturas → $Exe"
Write-Host "Evidências esperadas em: $evidence"

$p = Start-Process -FilePath $Exe -ArgumentList @(
  "-checkpointSmoke",
  "-captureEvidence",
  "-logfile", (Join-Path (Split-Path $Exe) "smoke-capture.log")
) -WorkingDirectory (Split-Path $Exe) -PassThru

$deadline = (Get-Date).AddMinutes(22)
while (-not $p.HasExited -and (Get-Date) -lt $deadline) {
  Start-Sleep -Seconds 3
}
if (-not $p.HasExited) {
  Stop-Process -Id $p.Id -Force
  throw "Smoke timeout"
}

Write-Host "Exit: $($p.ExitCode)"
Get-ChildItem -LiteralPath $evidence -Filter "*.png" -ErrorAction SilentlyContinue |
  Format-Table Name, Length, LastWriteTime
exit $(if ($null -eq $p.ExitCode) { 0 } else { $p.ExitCode })

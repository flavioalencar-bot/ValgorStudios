# Valida build QA por "duplo clique" (sem -cityProgressionQA) e roda auto-test.
param(
  [string]$Exe = "C:\Valgor_Studio\builds\windows\Valgor-QA-City-Progression\Valgor.exe",
  [int]$TimeoutMin = 30
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $Exe)) { throw "Exe nao encontrado: $Exe" }

Get-Process -Name Valgor -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

$evidence = "C:\Valgor_Studio\docs\releases\city-progression-qa-evidence"
New-Item -ItemType Directory -Force -Path $evidence | Out-Null
$report = Join-Path $evidence "auto-test-report.txt"
$bootLog = Join-Path $evidence "double-click-boot.log"
if (Test-Path $report) { Remove-Item $report -Force }

$workDir = Split-Path $Exe
$playerLog = Join-Path $env:USERPROFILE "AppData\LocalLow\Valgor Studios\Valgor\Player.log"

Write-Host "Boot sem CLI (simula duplo clique)..."
if (Test-Path $playerLog) { Remove-Item $playerLog -Force -ErrorAction SilentlyContinue }
$proc = Start-Process -FilePath $Exe -ArgumentList @("-screen-width","1600","-screen-height","900") -WorkingDirectory $workDir -PassThru

$deadline = (Get-Date).AddSeconds(90)
$bootOk = $false
do {
  Start-Sleep -Seconds 3
  if (-not (Test-Path $playerLog)) { continue }
  if (Select-String -Path $playerLog -Pattern "City carregada|homologation ON" -Quiet) {
    $bootOk = $true
    break
  }
  if ($proc.HasExited) { break }
} while ((Get-Date) -lt $deadline)

Start-Sleep -Seconds 4
if (Test-Path $playerLog) {
  Copy-Item $playerLog $bootLog -Force
  Write-Host "--- boot log snippets ---"
  Select-String -Path $playerLog -Pattern "Valgor.QA|homologation|Exception|error CS" | Select-Object -Last 25 | ForEach-Object { $_.Line }
}

Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

if (-not $bootOk) {
  Write-Host "FALHA: QA nao ativou no boot sem CLI. Log: $bootLog"
  exit 1
}
Write-Host "Boot sem CLI: QA ativo OK"

& (Join-Path $PSScriptRoot "run-city-progression-qa.ps1") -AutoTest -AutoTestTimeoutMin $TimeoutMin
exit $LASTEXITCODE

# Inicia Valgor em modo de homologacao da progressao da cidade.
param(
  [switch]$AutoTest,
  [string]$Exe = "C:\Valgor_Studio\builds\windows\Valgor-QA-City-Progression\Valgor.exe",
  [int]$AutoTestTimeoutMin = 25
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $Exe)) {
  throw "Exe nao encontrado: $Exe. Rode scripts/build-qa-city-progression.ps1 primeiro."
}

Get-Process -Name Valgor -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

$argList = @("-cityProgressionQA", "-screen-width", "1600", "-screen-height", "900")
if ($AutoTest) {
  $argList += "-cityProgressionQATest"
}

# Build QA ja compila com VALGOR_CITY_PROGRESSION_QA — flag CLI so e necessaria no Editor/auto-test.
Write-Host ("Launch: {0} {1}" -f $Exe, ($argList -join ' '))
Write-Host "Save QA: city-progression-qa"
Write-Host "Banner: MODO HOMOLOGACAO - ativo por define nesta build (duplo clique OK)"

$workDir = Split-Path $Exe
# Builds polished / context-menu escrevem em context-menu-final-evidence;
# builds QA base usam city-progression-qa-evidence.
$evidenceCandidates = @(
  "C:\Valgor_Studio\docs\releases\context-menu-final-evidence",
  "C:\Valgor_Studio\docs\releases\city-progression-qa-evidence"
)
$evidence = $evidenceCandidates[0]
$report = Join-Path $evidence "auto-test-report.txt"
if ($AutoTest) {
  foreach ($dir in $evidenceCandidates) {
    $candidate = Join-Path $dir "auto-test-report.txt"
    if (Test-Path $candidate) { Remove-Item $candidate -Force }
  }
}

$proc = Start-Process -FilePath $Exe -ArgumentList $argList -WorkingDirectory $workDir -PassThru

if (-not $AutoTest) {
  exit 0
}

$deadline = (Get-Date).AddMinutes($AutoTestTimeoutMin)
while ((Get-Date) -lt $deadline) {
  Start-Sleep -Seconds 5
  if ($proc.HasExited) { break }
  if (Test-Path $report) {
    Start-Sleep -Seconds 3
    break
  }
}

if (-not $proc.HasExited) {
  Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
}

$report = $null
foreach ($dir in $evidenceCandidates) {
  $candidate = Join-Path $dir "auto-test-report.txt"
  if (Test-Path $candidate) {
    $evidence = $dir
    $report = $candidate
    break
  }
}
if (-not $report) {
  Write-Host "AutoTest: report nao encontrado em $($evidenceCandidates -join ' | ')"
  exit 1
}

Write-Host "=== AutoTest report ==="
Get-Content $report
$fail = Select-String -Path $report -Pattern "FAIL" -Quiet
if ($fail) {
  Write-Host "AutoTest FAILED"
  exit 1
}
Write-Host "AutoTest OK"
exit 0

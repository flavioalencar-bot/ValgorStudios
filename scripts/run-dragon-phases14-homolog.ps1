# Roda E2E homologacao Dragao Fases 1-4 (-dragonPhases14Homolog).
param(
  [string]$Exe = "C:\Valgor_Studio_phase4_homolog\builds\windows\Valgor-QA-Dragon-Phase4-Homolog\Valgor.exe",
  [int]$TimeoutMin = 45
)

$ErrorActionPreference = "Stop"
if (-not (Test-Path $Exe)) {
  throw "Exe nao encontrado: $Exe"
}

$evidence = "C:\Valgor_Studio\docs\releases\dragon-phases-1-4-homolog-evidence"
$report = Join-Path $evidence "homolog-report.txt"
$log = Join-Path $evidence "logs\homolog-full-player.log"
New-Item -ItemType Directory -Force -Path (Join-Path $evidence "logs"), (Join-Path $evidence "screenshots"), (Join-Path $evidence "milestones") | Out-Null

# Limpa prefs QA / dragons v7 antes do run.
$regPath = "HKCU:\Software\Valgor Studios\Valgor"
if (Test-Path $regPath) {
  Get-Item $regPath | Select-Object -ExpandProperty Property | ForEach-Object {
    if ($_ -match 'valgor\.dragons\.v7|city-progression-qa') {
      Remove-ItemProperty -Path $regPath -Name $_ -ErrorAction SilentlyContinue
    }
  }
}

if (Test-Path $report) { Remove-Item $report -Force }
Get-Process -Name Valgor -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

$argList = @(
  "-dragonPhases14Homolog",
  "-cityProgressionQA",
  "-screen-width", "1600",
  "-screen-height", "900",
  "-logFile", $log
)

Write-Host ("Launch: {0} {1}" -f $Exe, ($argList -join ' '))
$workDir = Split-Path $Exe
$proc = Start-Process -FilePath $Exe -ArgumentList $argList -WorkingDirectory $workDir -PassThru

$deadline = (Get-Date).AddMinutes($TimeoutMin)
while ((Get-Date) -lt $deadline) {
  Start-Sleep -Seconds 5
  if ($proc.HasExited) { break }
  if (Test-Path $report) {
    Start-Sleep -Seconds 4
    break
  }
}

if (-not $proc.HasExited) {
  Write-Host "Timeout - encerrando processo."
  Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 2
}

if (-not (Test-Path $report)) {
  Write-Host "FAIL: homolog-report.txt nao encontrado. Log: $log"
  if (Test-Path $log) { Get-Content $log -Tail 80 }
  exit 1
}

Write-Host "=== Homolog report ==="
Get-Content $report
$pass = Select-String -Path $report -Pattern "RESULT=PASS" -Quiet
$p0 = Select-String -Path $report -Pattern "p0=0 p1=0" -Quiet
if ($pass -and $p0) {
  Write-Host "OK: E2E Fases 1-4 PASS"
  exit 0
}

Write-Host "FAIL: ver relatorio"
exit 1

# Run Capital Restored player capture (sem -capitalWorldPreview).
param(
  [string]$Exe = "C:\Valgor_Studio\builds\windows\Valgor-QA-Capital-Restored\Valgor.exe",
  [int]$TimeoutSec = 240
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$log = Join-Path $root "builds\windows\qa-capital-restored-run.log"
$evidence = Join-Path $root "docs\releases\capital-world-restoration"

if (-not (Test-Path $Exe)) { throw "Exe nao encontrado: $Exe" }

New-Item -ItemType Directory -Force -Path $evidence | Out-Null

# 01 = referência da composição quebrada (World Foundation).
$brokenSrc = Join-Path $root "docs\releases\capital-world-foundation\screenshots\11-overview-1920x1080.png"
if (-not (Test-Path $brokenSrc)) {
  $brokenSrc = Join-Path $root "docs\releases\capital-world-foundation\screenshots\01-empty-world-foundation.png"
}
if (Test-Path $brokenSrc) {
  Copy-Item $brokenSrc (Join-Path $evidence "01-current-broken.png") -Force
  Write-Host "01-current-broken.png <- $brokenSrc"
}

# Remove capturas antigas para nao aceitar falso-positivo.
@(
  "02-restored-capital.png",
  "03-restored-1080x640.png",
  "04-restored-1600x900.png",
  "05-restored-1920x1080.png",
  "06-side-by-side.png"
) | ForEach-Object {
  $p = Join-Path $evidence $_
  if (Test-Path $p) { Remove-Item $p -Force }
}

Get-Process -Name "Valgor" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
if (Test-Path $log) { Remove-Item $log -Force }

$runStart = Get-Date
$argsList = @(
  "-cityProgressionQA",
  "-capitalLayoutV2Preview",
  "-logFile", $log
)
Write-Host "Launch: $Exe $($argsList -join ' ')"
$proc = Start-Process -FilePath $Exe -ArgumentList $argsList -PassThru -WorkingDirectory (Split-Path $Exe)

$deadline = (Get-Date).AddSeconds($TimeoutSec)
$ok = $false
do {
  Start-Sleep -Seconds 5
  $f2 = Join-Path $evidence "02-restored-capital.png"
  $f5 = Join-Path $evidence "05-restored-1920x1080.png"
  $have = (Test-Path $f2) -and (Test-Path $f5) -and
          ((Get-Item $f2).LastWriteTime -gt $runStart) -and
          ((Get-Item $f5).LastWriteTime -gt $runStart)
  $logHit = (Test-Path $log) -and [bool](Select-String -Path $log -Pattern "Capital Restored capturas OK" -Quiet -ErrorAction SilentlyContinue)
  if ($have -or $logHit) {
    Start-Sleep -Seconds 4
    $ok = (Test-Path $f2) -and (Test-Path $f5) -and ((Get-Item $f2).LastWriteTime -gt $runStart)
    break
  }
  if ($proc.HasExited) { break }
} while ((Get-Date) -lt $deadline)

if (-not $proc.HasExited) {
  Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
  Start-Sleep -Seconds 2
}

if (-not $ok) {
  Write-Host "CAPTURE FAIL. log=$log"
  if (Test-Path $log) { Get-Content $log -Tail 80 }
  exit 1
}

# Side-by-side comparison (broken vs restored).
$side = Join-Path $evidence "06-side-by-side.png"
$before = Join-Path $evidence "01-current-broken.png"
$after = Join-Path $evidence "02-restored-capital.png"
try {
  Add-Type -AssemblyName System.Drawing
  $imgA = [System.Drawing.Image]::FromFile($before)
  $imgB = [System.Drawing.Image]::FromFile($after)
  $h = [Math]::Max($imgA.Height, $imgB.Height)
  $w = $imgA.Width + $imgB.Width + 16
  $bmp = New-Object System.Drawing.Bitmap $w, $h
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.Clear([System.Drawing.Color]::FromArgb(30, 30, 34))
  $g.DrawImage($imgA, 0, 0)
  $g.DrawImage($imgB, $imgA.Width + 16, 0)
  $g.Dispose()
  $bmp.Save($side, [System.Drawing.Imaging.ImageFormat]::Png)
  $bmp.Dispose(); $imgA.Dispose(); $imgB.Dispose()
  Write-Host "Side-by-side: $side"
} catch {
  Write-Host "Side-by-side skip: $($_.Exception.Message)"
}

Write-Host "OK captures in $evidence"
Get-ChildItem $evidence -Filter "*.png" | Select-Object Name, Length, LastWriteTime
exit 0

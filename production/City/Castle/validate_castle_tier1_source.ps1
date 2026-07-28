# Valida presenca do Castelo Tier 1 (source). Nao altera a City.
$ErrorActionPreference = "Stop"
$castle = $PSScriptRoot
$source = Join-Path $castle "source"
$manifestPath = Join-Path $castle "unity_staging\unity_import_manifest.json"
$candidates = @("Castle_Tier1.glb", "Castle_Tier1.fbx")
$present = @($candidates | Where-Object { Test-Path -LiteralPath (Join-Path $source $_) })
$blocked = $present.Count -eq 0
$status = if ($blocked) {
  "BLOQUEADO POR ASSET REAL"
} else {
  "ASSET PRESENTE - aguardando importacao sob ordem"
}

$report = [ordered]@{
  status      = $status
  blocked     = $blocked
  source_dir  = $source
  present     = @($present)
  missing     = @($candidates | Where-Object { $_ -notin $present })
  scale_rules = [ordered]@{
    footprint_xz = @(5.5, 9.0)
    height_max   = 12.0
    pivot        = "base center Y=0"
    forward      = "+Z main gate"
  }
}

if (Test-Path -LiteralPath $manifestPath) {
  $raw = Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8
  $data = $raw | ConvertFrom-Json
  $data.blocked = $blocked
  $data.status = $status
  $data.source_present = @($present | ForEach-Object { Join-Path $source $_ })
  if ($blocked) {
    $data.blocked_reason = "BLOQUEADO POR ASSET REAL - aguardando Castle_Tier1.glb ou Castle_Tier1.fbx em production/City/Castle/source/"
  } else {
    $data.blocked_reason = ""
  }
  $json = $data | ConvertTo-Json -Depth 8
  Set-Content -LiteralPath $manifestPath -Value ($json + "`n") -Encoding UTF8
  $report["manifest_updated"] = $manifestPath
}

$report | ConvertTo-Json -Depth 5
exit $(if ($blocked) { 1 } else { 0 })

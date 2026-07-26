# Watch production/Vortex/source for Vortex_Base.glb / .fbx and auto-import into the blend.
param(
    [int]$PollSeconds = 5
)

$ErrorActionPreference = "Stop"
$blender = "C:\Program Files\Blender Foundation\Blender 5.2\blender.exe"
$blend = "C:\Valgor_Studio\production\Vortex\Vortex_Production.blend"
$script = "C:\Valgor_Studio\production\Vortex\import_vortex_base_model.py"
$sourceDir = "C:\Valgor_Studio\production\Vortex\source"
$stampFile = "C:\Valgor_Studio\production\Vortex\reports\last_imported_source.stamp"

New-Item -ItemType Directory -Force -Path $sourceDir | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path $stampFile) | Out-Null

Write-Host "Watching $sourceDir for Vortex_Base.glb / Vortex_Base.fbx (Ctrl+C to stop)..."

function Get-SourceCandidate {
    foreach ($name in @("Vortex_Base.glb", "Vortex_Base.fbx", "Vortex_Base.gltf")) {
        $p = Join-Path $sourceDir $name
        if (Test-Path $p) { return Get-Item $p }
    }
    return $null
}

while ($true) {
    $file = Get-SourceCandidate
    if ($null -ne $file) {
        $sig = "{0}|{1}|{2}" -f $file.FullName, $file.Length, $file.LastWriteTimeUtc.Ticks
        $prev = if (Test-Path $stampFile) { Get-Content $stampFile -Raw } else { "" }
        if ($sig.Trim() -ne $prev.Trim()) {
            Write-Host "$(Get-Date -Format o) Detected $($file.Name) — importing..."
            & $blender -b $blend --python $script
            Set-Content -Path $stampFile -Value $sig -Encoding UTF8
            Write-Host "$(Get-Date -Format o) Import finished (exit=$LASTEXITCODE). Reports in production\Vortex\reports\"
        }
    }
    Start-Sleep -Seconds $PollSeconds
}

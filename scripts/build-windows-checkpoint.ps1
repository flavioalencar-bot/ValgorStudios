# Build Windows Checkpoint — redireciona para Beta 0.1
param(
  [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.0.58f2\Editor\Unity.exe",
  [switch]$SkipCloseEditor
)

$script = Join-Path $PSScriptRoot "build-windows-beta.ps1"
& $script -UnityExe $UnityExe -SkipCloseEditor:$SkipCloseEditor
exit $LASTEXITCODE

param (
    [Parameter(Mandatory = $true)]
    [string]$SourcePlugins,

    [Parameter(Mandatory = $true)]
    [string]$DestinationPlugins
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $SourcePlugins)) {
    throw "Plugins folder not found at: $SourcePlugins"
}

# Ensure destination paths exist
$cszipDevPath = Join-Path $DestinationPlugins "cszip_dev"
if (-not (Test-Path $DestinationPlugins)) {
    New-Item -ItemType Directory -Path $DestinationPlugins | Out-Null
}
if (-not (Test-Path $cszipDevPath)) {
    New-Item -ItemType Directory -Path $cszipDevPath | Out-Null
}

# === 1. Copy each subdirectory to cszip_dev/{foldername} ===
Get-ChildItem -Path $SourcePlugins -Directory | ForEach-Object {
    $pluginName = $_.Name
    $sourcePath = $_.FullName
    $targetPath = Join-Path $cszipDevPath $pluginName

    if (Test-Path $targetPath) {
        Remove-Item $targetPath -Recurse -Force
    }

    Copy-Item $sourcePath -Destination $targetPath -Recurse -Force
    Write-Host "Copied folder plugin: $pluginName → $targetPath"
}

# === 2. Copy top-level .cs files directly to DestinationPlugins ===
Get-ChildItem -Path $SourcePlugins -Filter *.cs -File | ForEach-Object {
    Copy-Item $_.FullName -Destination $DestinationPlugins -Force
    Write-Host "Copied file plugin: $($_.Name) → $DestinationPlugins"
}

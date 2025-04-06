<#
.SYNOPSIS
Deploys plugins as specified in Directory.Build.props files.

.DESCRIPTION
This script reads configuration from Directory.Build.props and Directory.Build.User.props
to determine which plugins to deploy and where to deploy them. It handles both single-file
plugins and folder plugins automatically.

.EXAMPLE
.\Deploy-Plugins.ps1
#>

$ErrorActionPreference = "Stop"

# Set paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$srcPath = Join-Path $scriptPath "../src"
$buildPropsPath = Join-Path $srcPath "Directory.Build.props"
$userPropsPath = Join-Path $srcPath "Directory.Build.User.props"

# Function to extract property value from XML
function Get-XmlProperty {
    param(
        [xml]$XmlDocument,
        [string]$PropertyName
    )
    
    $propertyNodes = $XmlDocument.SelectNodes("//PropertyGroup/$PropertyName")
    
    if ($propertyNodes.Count -gt 0 -and ![string]::IsNullOrEmpty($propertyNodes[$propertyNodes.Count - 1].InnerText)) {
        return $propertyNodes[$propertyNodes.Count - 1].InnerText
    }
    
    return $null
}

# Load the base props file
$buildProps = [xml](Get-Content $buildPropsPath -ErrorAction SilentlyContinue)
if (-not $buildProps) {
    Write-Error "Could not find Directory.Build.props at $buildPropsPath"
    exit 1
}

# Load the user props file if it exists
$userProps = $null
if (Test-Path $userPropsPath) {
    $userProps = [xml](Get-Content $userPropsPath -ErrorAction SilentlyContinue)
}

# Determine if auto deploy is enabled
$autoDeployEnabled = "false"
if ($buildProps) {
    $autoDeployEnabled = Get-XmlProperty -XmlDocument $buildProps -PropertyName "AutoDeployEnabled"
}
if ($userProps) {
    $userAutoDeployEnabled = Get-XmlProperty -XmlDocument $userProps -PropertyName "AutoDeployEnabled"
    if ($userAutoDeployEnabled) {
        $autoDeployEnabled = $userAutoDeployEnabled
    }
}

# If auto deploy is not enabled, exit
if ($autoDeployEnabled -ne "true") {
    Write-Host "Auto deployment is disabled. Set AutoDeployEnabled to true in props files"
    exit 0
}

# Get plugins output directory
$destinationPlugins = Get-XmlProperty -XmlDocument $buildProps -PropertyName "PluginsOutput"
if ($userProps) {
    $userDestinationPlugins = Get-XmlProperty -XmlDocument $userProps -PropertyName "PluginsOutput"
    if ($userDestinationPlugins) {
        $destinationPlugins = $userDestinationPlugins
    }
}

# Get source plugins directory
$sourcePlugins = Get-XmlProperty -XmlDocument $buildProps -PropertyName "PluginsSourceDir"
if ($userProps) {
    $userSourcePlugins = Get-XmlProperty -XmlDocument $userProps -PropertyName "PluginsSourceDir"
    if ($userSourcePlugins) {
        $sourcePlugins = $userSourcePlugins
    }
}

# If source directory isn't specified, use the default Active folder
if ([string]::IsNullOrEmpty($sourcePlugins)) {
    $sourcePlugins = Join-Path $srcPath "Carbon.Plugins"
	$sourcePlugins = Join-Path $sourcePlugins "Active"
}

# Get plugins to deploy
$pluginsList = Get-XmlProperty -XmlDocument $buildProps -PropertyName "Plugins"
if ($userProps) {
    $userPluginsList = Get-XmlProperty -XmlDocument $userProps -PropertyName "Plugins"
    if ($userPluginsList) {
        $pluginsList = $userPluginsList
    }
}

# Validate paths and settings
if ([string]::IsNullOrEmpty($destinationPlugins)) {
    Write-Error "Plugins output directory is not set in props files"
    exit 1
}

if ([string]::IsNullOrEmpty($pluginsList)) {
    Write-Host "No plugins specified for deployment in props files"
    exit 0
}

if (-not (Test-Path $sourcePlugins)) {
    Write-Error "Plugins source folder not found at: $sourcePlugins"
    exit 1
}

# Parse the plugins list
$pluginsToDeployArray = $pluginsList -split ',' | ForEach-Object { $_.Trim() }
Write-Host "Plugins to deploy: $($pluginsToDeployArray -join ', ')"

# Ensure destination paths exist
$cszipDevPath = Join-Path $destinationPlugins "cszip_dev"
if (-not (Test-Path $destinationPlugins)) {
    New-Item -ItemType Directory -Path $destinationPlugins | Out-Null
}
if (-not (Test-Path $cszipDevPath)) {
    New-Item -ItemType Directory -Path $cszipDevPath | Out-Null
}

# === 1. Copy each folder plugin that's in the list ===
Get-ChildItem -Path $sourcePlugins -Directory | ForEach-Object {
    $pluginName = $_.Name
    
    # Check if this plugin should be deployed
    if ($pluginsToDeployArray -contains $pluginName) {
        $sourcePath = $_.FullName
        $targetPath = Join-Path $cszipDevPath $pluginName
        
        if (Test-Path $targetPath) {
            Remove-Item $targetPath -Recurse -Force
        }
        
        Copy-Item $sourcePath -Destination $targetPath -Recurse -Force
        Write-Host "Copied folder plugin: $pluginName → $targetPath"
    }
}

# === 2. Copy top-level .cs files that match the list ===
Get-ChildItem -Path $sourcePlugins -Filter *.cs -File | ForEach-Object {
    $pluginName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
    
    # Check if this plugin should be deployed
    if ($pluginsToDeployArray -contains $pluginName) {
        Copy-Item $_.FullName -Destination $destinationPlugins -Force
        Write-Host "Copied file plugin: $($_.Name) → $destinationPlugins"
    }
}

Write-Host "Plugin deployment completed successfully!"
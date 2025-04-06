<#
.SYNOPSIS
Deploys an extension based on settings in Directory.Build.props files.

.PARAMETER Name
The name of the extension to deploy.

.PARAMETER TargetFile
The full path to the compiled extension DLL.

.EXAMPLE
.\Deploy-Extension.ps1 -Name "FluentUI" -TargetFile "C:\Path\To\FluentUI.dll"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [Parameter(Mandatory=$true)]
    [string]$TargetFile
)

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

# Validate target file
if (-not (Test-Path $TargetFile)) {
    Write-Error "Target file does not exist: $TargetFile"
    exit 1
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

# Get extensions output directory
$outputDir = Get-XmlProperty -XmlDocument $buildProps -PropertyName "ExtensionsOutput"
if ($userProps) {
    $userOutputDir = Get-XmlProperty -XmlDocument $userProps -PropertyName "ExtensionsOutput"
    if ($userOutputDir) {
        $outputDir = $userOutputDir
    }
}

# Get extensions to deploy
$itemsList = Get-XmlProperty -XmlDocument $buildProps -PropertyName "Extensions"
if ($userProps) {
    $userItemsList = Get-XmlProperty -XmlDocument $userProps -PropertyName "Extensions"
    if ($userItemsList) {
        $itemsList = $userItemsList
    }
}

# Get PDB setting
$includePdb = $false
$includePdbSetting = Get-XmlProperty -XmlDocument $buildProps -PropertyName "IncludePdb"
if ($userProps) {
    $userIncludePdbSetting = Get-XmlProperty -XmlDocument $userProps -PropertyName "IncludePdb"
    if ($userIncludePdbSetting) {
        $includePdbSetting = $userIncludePdbSetting
    }
}

if ($includePdbSetting -eq "true") {
    $includePdb = $true
}

# Validate paths and settings
if ([string]::IsNullOrEmpty($outputDir)) {
    Write-Error "Extensions output directory is not set in props files"
    exit 1
}

if ([string]::IsNullOrEmpty($itemsList)) {
    Write-Error "No extensions found to deploy in props files"
    exit 1
}

# Check if the specified extension is in the list
$extensions = $itemsList -split ',' | ForEach-Object { $_.Trim() }
if (-not $extensions.Contains($Name)) {
    Write-Host "$Name is not in the list of extensions to deploy"
    exit 0
}

# Create output directory if it doesn't exist
if (-not (Test-Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
    Write-Host "Created output directory: $outputDir"
}

# Get the directory and filename of the target
$targetDir = Split-Path -Parent $TargetFile
$targetFileName = Split-Path -Leaf $TargetFile

# Copy DLL
Copy-Item -Path $TargetFile -Destination $outputDir -Force
Write-Host "Deployed extension: $targetFileName to $outputDir"

# Copy PDB if needed
if ($includePdb) {
    $pdbFileName = [System.IO.Path]::GetFileNameWithoutExtension($targetFileName) + ".pdb"
    $pdbPath = Join-Path $targetDir $pdbFileName
    
    if (Test-Path $pdbPath) {
        Copy-Item -Path $pdbPath -Destination $outputDir -Force
        Write-Host "Deployed PDB: $pdbFileName to $outputDir"
    } else {
        Write-Warning "PDB file not found: $pdbPath"
    }
}

Write-Host "Extension deployment completed successfully!"
param (
    [Parameter(Mandatory = $true, HelpMessage = "Path to the RustDedicated_Data\Managed directory")]
    [string]$SourcePath,

    [Parameter(Mandatory = $false, HelpMessage = "Path to the carbon\managed directory")]
    [string]$SourceCarbonPath = $null,

    [string]$DestinationPath = "$PSScriptRoot\..\src\managed"
)

if (!$SourceCarbonPath)
{
    $SourceCarbonPath = Join-Path $SourcePath "..\..\carbon\managed"
}

$rustManagedLibs = @(
    "0Harmony.dll",
    "Assembly-CSharp*.dll",
    "Autodesk.Fbx.dll",
    "Azure*.dll",
    "Cinemachine.dll",
    "Cronos.dll",
    "CurvedTextMeshPro.dll",
    "DelaunayER.dll",
    "Discord.Sdk.dll",
    "DnsClient.dll",
    "EasyRoads3Dv3.dll",
    "EZhex1991.EZSoftBone.dll",
    "Facepunch*.dll",
    "FbxBuildTestAssets.dll",
    "Fleck.dll",
    "GA.dll",
    "I18N*.dll",
    "Ionic.Zip.Reduced.dll",
    "LZ4.dll",
    "LZ4pn.dll",
    "Melanchall.DryWetMidi.dll",
    "MidiJack.dll",
    "Mono*.dll",
    "MP3Sharp.dll",
    "netstandard.dll",
    "Newtonsoft.Json.dll",
    "RTLTMPro.dll",
    "Rust*.dll",
    "SingularityGroup.HotReload*.dll",
    "System.dll",
    "TimeZoneConverter.dll",
    "UIEffect.dll",
    "Unity*.dll",
    "ZString.dll"
)

$carbonManagedLibs = @(
    "Carbon.UniTask.dll"
);

if (Test-Path $DestinationPath)
{
    Get-ChildItem -Path $DestinationPath -File | Remove-Item -Force
}
else
{
    New-Item -Path $DestinationPath -ItemType Directory | Out-Null
}

$copied = 0
foreach ($pattern in $rustManagedLibs)
{
    $files = Get-ChildItem -Path $SourcePath -Filter $pattern -File
    foreach ($file in $files)
    {
        Copy-Item -Path $file.FullName -Destination (Join-Path $DestinationPath $file.Name) -Force
        Write-Host "Updating: $($file.Name) -> $($DestinationPath)"
        $copied++
    }
}

foreach ($pattern in $carbonManagedLibs)
{
    $files = Get-ChildItem -Path $SourceCarbonPath -Filter $pattern -File
    foreach ($file in $files)
    {
        Copy-Item -Path $file.FullName -Destination (Join-Path $DestinationPath $file.Name) -Force
        Write-Host "Updating: $($file.Name) -> $($DestinationPath)"
        $copied++
    }
}

if ($copied -eq 0)
{
    Write-Host "No files copied. Please check the source path and patterns."
    exit 1
}

Write-Host "Packages updated."

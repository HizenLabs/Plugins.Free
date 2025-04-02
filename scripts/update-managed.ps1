param (
    [Parameter(Mandatory = $true, HelpMessage = "Path to the RustDedicated_Data\Managed directory")]
    [string]$SourcePath,

    [string]$DestinationPath = "$PSScriptRoot\..\src\managed"
)

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
    "RTLTMPro.dll",
    "Rust*.dll",
    "SingularityGroup.HotReload*.dll",
    "System.dll",
    "TimeZoneConverter.dll",
    "UIEffect.dll",
    "Unity*.dll",
    "ZString.dll"
)

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

Write-Host "Packages updated."

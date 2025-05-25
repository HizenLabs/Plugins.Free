param (
    [string]$SrcDir = "./src"
)

$SrcDir = $SrcDir.Trim('"')
Write-Host "[update-version] Searching $SrcDir for files..."

function Get-Version {
    $now = Get-Date
    $startOfMonth = Get-Date -Year $now.Year -Month $now.Month -Day 1 -Hour 0 -Minute 0 -Second 0
    $yy = $now.ToString("yy")
    $m = $now.Month
    $minutes = [int]($now - $startOfMonth).TotalMinutes
    return "$yy.$m.$minutes"
}

function Update-FileVersion {
    param (
        [string]$path,
        [string]$version
    )

    $content = Get-Content $path -Raw
    $pattern = '\[Info\("([^"]+)",\s*"([^"]+)",\s*"([^"]+)"\)\]'

    if ($content -match $pattern) {
        $new = $content -replace $pattern, "[Info(`"`$1`", `"`$2`", `"$version`")]"
        
        # Remove any extra trailing blank lines
        $new = $new -replace "(\r?\n)+\z", "`r`n"

        # Write without additional newline
        [System.IO.File]::WriteAllText($path, $new, [System.Text.Encoding]::UTF8)
        Write-Host "[update-version] Updated: $path" -ForegroundColor Green
    }
}


function Update-AssemblyInfo {
    param (
        [string]$directory,
        [string]$version
    )

    $assemblyInfoPath = Get-ChildItem -Path $directory -Recurse -Filter "AssemblyInfo.cs" | Select-Object -First 1
    if (-not $assemblyInfoPath) {
        Write-Host "AssemblyInfo.cs not found in $directory" -ForegroundColor Yellow
        return
    }

    $versionFourPart = "$version.0"
    $content = Get-Content $assemblyInfoPath.FullName -Raw

    $content = $content -replace 'AssemblyVersion\(".*?"\)', "AssemblyVersion(`"$versionFourPart`")"
    $content = $content -replace 'AssemblyFileVersion\(".*?"\)', "AssemblyFileVersion(`"$versionFourPart`")"

    # Trim excessive trailing newlines, leave exactly one
    $content = $content -replace "(\r?\n)+\z", "`r`n"

    # Write content using System.IO to avoid newline issues
    [System.IO.File]::WriteAllText($assemblyInfoPath.FullName, $content, [System.Text.Encoding]::UTF8)

    Write-Host "Updated AssemblyInfo: $($assemblyInfoPath.FullName)" -ForegroundColor Green
}


# Get new version string
$version = Get-Version

# Process all .cs files under src/
Get-ChildItem -Path $SrcDir -Filter *.cs -Recurse | ForEach-Object {
    $path = $_.FullName

    # Only update files that contain the [Info(...)] attribute
    $raw = Get-Content $path -Raw
    if ($raw -match '\[Info\("([^"]+)",\s*"([^"]+)",\s*"([^"]+)"\)\]') {
        Update-FileVersion -path $path -version $version
    }
}

Update-AssemblyInfo -directory $SrcDir -version $version

Write-Host "`n[update-version] Update complete." -ForegroundColor Cyan

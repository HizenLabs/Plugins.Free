param (
    [string]$SrcDir = "./src"
)

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
        Set-Content $path $new
        Write-Host "Updated: $path" -ForegroundColor Green
    }
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

Write-Host "`nVersion update completed." -ForegroundColor Cyan

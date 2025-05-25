param (
    [string]$SrcDir = "./src",
    [string]$CacheDir = "./temp"
)

$CacheFile = Join-Path $CacheDir 'version-cache.json'

function Ensure-Cache {
    if (-not (Test-Path $CacheDir)) {
        New-Item -ItemType Directory -Path $CacheDir | Out-Null
    }
    if (Test-Path $CacheFile) {
        return Get-Content $CacheFile | ConvertFrom-Json
    } else {
        return @{}
    }
}

function Save-Cache($cache) {
    $cache | ConvertTo-Json -Depth 10 | Set-Content $CacheFile
}

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
        return $true
    }

    return $false
}

# Load existing cache and determine current version string
$cache = Ensure-Cache
$version = Get-Version

# Iterate files
Get-ChildItem -Path $SrcDir -Filter *.cs -Recurse | ForEach-Object {
    $path = $_.FullName
    $key = Resolve-Path $_.FullName | ForEach-Object { $_.Path }
    $lastWrite = (Get-Item $path).LastWriteTimeUtc.ToString("o")

    # Skip if file hasn't changed
    if ($cache[$key] -eq $lastWrite) {
        return
    }

    # Only proceed if file contains [Info(...)]
    $raw = Get-Content $path -Raw
    if ($raw -notmatch '\[Info\("([^"]+)",\s*"([^"]+)",\s*"([^"]+)"\)\]') {
        return
    }

    # Update and cache
    if (Update-FileVersion -path $path -version $version) {
        $cache[$key] = $lastWrite
    }
}

# Save updated cache
Save-Cache $cache
Write-Host "`nVersion update completed." -ForegroundColor Cyan

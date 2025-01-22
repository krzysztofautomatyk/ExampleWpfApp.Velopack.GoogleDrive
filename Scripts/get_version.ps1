$propsPath = Get-ChildItem -Path $PSScriptRoot -Filter 'Directory.Build.props' -Recurse | Select-Object -First 1

if ($propsPath) {
    [xml]$xml = Get-Content -Raw $propsPath.FullName
    $version = $xml.Project.PropertyGroup.PackageVersion
} else {
    $version = "1.0.0"
}

# Remove hidden white spaces and new line
$version = $version.Trim()

# Ensure the version is in the correct format
if ($version -notmatch '^\d+\.\d+\.\d+$') {
    $version = "1.0.0"
}

# Save the file WITHOUT BOM
$utf8NoBOM = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText("$PSScriptRoot\version.txt", $version, $utf8NoBOM)

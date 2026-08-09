[CmdletBinding()]
param(
    [string]$SitePath = (Join-Path $PSScriptRoot "..\samples\EditorialDigest.TestSite")
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$samplesRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "samples"))
$resolvedSitePath = [System.IO.Path]::GetFullPath($SitePath)
$samplesPrefix = $samplesRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar

if (-not $resolvedSitePath.StartsWith($samplesPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "SitePath must be located under $samplesRoot."
}

if (Test-Path -LiteralPath $resolvedSitePath) {
    throw "The acceptance site already exists at $resolvedSitePath. Remove it manually before creating a new one."
}

$packageProject = Join-Path $repositoryRoot "src\Umbraco.EditorialDigest\Umbraco.EditorialDigest.csproj"
$packageOutput = Join-Path $repositoryRoot "src\Umbraco.EditorialDigest\bin\Release"

dotnet new install Umbraco.Templates@18.1.0 --force
if ($LASTEXITCODE -ne 0) {
    throw "Installing the Umbraco 18.1.0 templates failed."
}

dotnet pack $packageProject --configuration Release --output $packageOutput
if ($LASTEXITCODE -ne 0) {
    throw "Packing Umbraco.EditorialDigest failed."
}

dotnet new umbraco --name EditorialDigest.TestSite --output $resolvedSitePath --development-database-type SQLite
if ($LASTEXITCODE -ne 0) {
    throw "Creating the local Umbraco acceptance site failed."
}

$databasePath = Join-Path $resolvedSitePath "umbraco\Data\Umbraco.sqlite.db"
if (-not (Test-Path -LiteralPath $databasePath)) {
    New-Item -ItemType Directory -Path (Split-Path $databasePath) -Force | Out-Null
    New-Item -ItemType File -Path $databasePath | Out-Null
}

$siteProject = Join-Path $resolvedSitePath "EditorialDigest.TestSite.csproj"
$nugetConfig = Join-Path $resolvedSitePath "NuGet.Config"
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="EditorialDigestLocal" value="$packageOutput" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath $nugetConfig -Encoding utf8

dotnet add $siteProject package Umbraco.EditorialDigest --version 0.1.0
if ($LASTEXITCODE -ne 0) {
    throw "Installing Umbraco.EditorialDigest in the local acceptance site failed."
}

Write-Host "Acceptance site created at $resolvedSitePath."
Write-Host "Run: dotnet run --project $siteProject"
Write-Host "Complete the Umbraco installer in your browser. Do not store installer credentials in source control."

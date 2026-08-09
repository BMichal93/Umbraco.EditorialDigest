[CmdletBinding()]
param(
    [ValidateSet("17", "18")]
    [string]$Version = "18"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$dataDirectory = Join-Path $repositoryRoot "test\MockUmbraco$Version\umbraco\Data"
$databaseFiles = @(
    (Join-Path $dataDirectory "Umbraco.sqlite.db"),
    (Join-Path $dataDirectory "Umbraco.sqlite.db-shm"),
    (Join-Path $dataDirectory "Umbraco.sqlite.db-wal")
)

foreach ($databaseFile in $databaseFiles) {
    if (Test-Path -LiteralPath $databaseFile) {
        Remove-Item -LiteralPath $databaseFile -Force
    }
}

Write-Host "Mock Umbraco $Version database reset."

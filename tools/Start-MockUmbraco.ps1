[CmdletBinding()]
param(
    [ValidateSet("17", "18")]
    [string]$Version = "18"
)

$ErrorActionPreference = "Stop"

function Get-MockSetting {
    param(
        [string]$EnvironmentVariable,
        [string]$Prompt,
        [switch]$Secret
    )

    $value = [Environment]::GetEnvironmentVariable($EnvironmentVariable)
    if (-not [string]::IsNullOrWhiteSpace($value)) {
        return $value
    }

    if (-not $Secret) {
        $value = Read-Host $Prompt
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }

        throw "$EnvironmentVariable is required."
    }

    $secureValue = Read-Host $Prompt -AsSecureString
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureValue)
    try {
        $value = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }

    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "$EnvironmentVariable is required."
    }

    return $value
}

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$siteName = "MockUmbraco$Version"
$siteProject = Join-Path $repositoryRoot "test\$siteName\$siteName.csproj"
if (-not (Test-Path -LiteralPath $siteProject)) {
    throw "The mock site project was not found at $siteProject."
}

$port = if ($Version -eq "17") { 17443 } else { 18443 }
$umbracoVersion = if ($Version -eq "17") { "17.6.0" } else { "18.1.0" }
$name = Get-MockSetting "EDITORIAL_DIGEST_MOCK_ADMIN_NAME" "Administrator name"
$email = Get-MockSetting "EDITORIAL_DIGEST_MOCK_ADMIN_EMAIL" "Administrator email"
$password = Get-MockSetting "EDITORIAL_DIGEST_MOCK_ADMIN_PASSWORD" "Administrator password" -Secret

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Umbraco__CMS__Unattended__UnattendedUserName = $name
$env:Umbraco__CMS__Unattended__UnattendedUserEmail = $email
$env:Umbraco__CMS__Unattended__UnattendedUserPassword = $password

try {
    & dotnet run --project $siteProject --no-launch-profile --urls "https://localhost:$port" "-p:UmbracoVersion=$umbracoVersion"
}
finally {
    Remove-Item Env:Umbraco__CMS__Unattended__UnattendedUserName -ErrorAction SilentlyContinue
    Remove-Item Env:Umbraco__CMS__Unattended__UnattendedUserEmail -ErrorAction SilentlyContinue
    Remove-Item Env:Umbraco__CMS__Unattended__UnattendedUserPassword -ErrorAction SilentlyContinue
}

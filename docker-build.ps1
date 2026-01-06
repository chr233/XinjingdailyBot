# PowerShell script for building .NET project (Windows/local use)
param(
    [string]$ProjectName = "XinjingdailyBot.WebAPI",
    [string]$Configuration = "Release",
    [string]$TargetFramework = "net10.0",
    [string]$TargetOS = "win",
    [string]$TargetArch = "x64",
    [string]$OutputPath = "/publish"
)

$ErrorActionPreference = "Stop"

# Validate OS
$supportedOS = @("win", "linux", "osx")
if ($TargetOS -notin $supportedOS) {
    Write-Error "ERROR: Unsupported OS: $TargetOS. Supported: $($supportedOS -join ', ')"
    exit 1
}

# Determine runtime identifier based on architecture
$framework = switch ($TargetArch) {
    "x64"   { "$TargetOS-x64" }
    "amd64" { "$TargetOS-x64" }
    "arm"   { "$TargetOS-arm" }
    "arm64" { "$TargetOS-arm64" }
    default {
        Write-Error "ERROR: Unsupported CPU architecture: $TargetArch"
        exit 1
    }
}

Write-Host "Building for framework: $framework" -ForegroundColor Cyan

$publishArgs = @(
    "publish"
    $ProjectName
    "-c", $Configuration
    "-o", $OutputPath
    "-r", $framework
    "-f", $TargetFramework
    "-p:ContinuousIntegrationBuild=true"
    "-p:PublishSingleFile=false"
    "-p:PublishTrimmed=false"
    "-p:UseAppHost=false"
    "--nologo"
    "--no-self-contained"
)

Write-Host "dotnet $($publishArgs -join ' ')" -ForegroundColor DarkGray

dotnet --info
dotnet @publishArgs

Write-Host "Build completed successfully!" -ForegroundColor Green

<#
.SYNOPSIS
    Builds RevitParameterInspector.Revit for each supported Revit version and copies the
    output into RevitParameterInspector.bundle/Contents/<version>, next to the .addin manifest
    already committed there.

.DESCRIPTION
    Run from anywhere; paths are resolved relative to this script's location. Requires the
    corresponding Revit version to be installed (for RevitAPI.dll/RevitAPIUI.dll resolution) -
    see docs/build-guide.md and docs/revit-version-support.md.

.PARAMETER Versions
    Which Revit versions to build. Defaults to all three supported versions.

.PARAMETER Configuration
    Build configuration (Debug/Release). Defaults to Release.

.EXAMPLE
    ./install/bundle/build-bundle.ps1
    ./install/bundle/build-bundle.ps1 -Versions 2025,2026 -Configuration Debug
#>
param(
    [ValidateSet(2024, 2025, 2026)]
    [int[]] $Versions = @(2024, 2025, 2026),

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$revitProject = Join-Path $repoRoot 'src\RevitParameterInspector.Revit'
$bundleContents = Join-Path $PSScriptRoot 'RevitParameterInspector.bundle\Contents'

function Get-TargetFramework([int] $version) {
    if ($version -eq 2024) { return 'net48' }
    return 'net8.0-windows'
}

foreach ($version in $Versions) {
    $tfm = Get-TargetFramework $version
    Write-Host "==> Building for Revit $version ($tfm, $Configuration)" -ForegroundColor Cyan

    $buildArgs = @($revitProject, '-c', $Configuration, '-f', $tfm)
    if ($version -eq 2026) {
        $buildArgs += '-p:RevitVersion=2026'
    }

    & dotnet build @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed for Revit $version (exit code $LASTEXITCODE). Is Revit $version installed? See docs/build-guide.md."
    }

    $outputDir = Join-Path $revitProject "bin\$Configuration\$tfm"
    if (-not (Test-Path $outputDir)) {
        throw "Expected build output not found: $outputDir"
    }

    $destination = Join-Path $bundleContents $version
    if (-not (Test-Path $destination)) {
        New-Item -ItemType Directory -Path $destination -Force | Out-Null
    }

    Write-Host "==> Copying build output to $destination" -ForegroundColor Cyan
    Get-ChildItem -Path $outputDir -Filter '*.dll' | Copy-Item -Destination $destination -Force

    $dictionarySource = Join-Path $outputDir 'dictionary'
    if (Test-Path $dictionarySource) {
        Copy-Item -Path $dictionarySource -Destination $destination -Recurse -Force
    }
}

Write-Host "==> Done. RevitParameterInspector.bundle is ready under $(Split-Path $bundleContents -Parent)." -ForegroundColor Green
Write-Host "    Copy that .bundle folder to %ProgramData%\Autodesk\ApplicationPlugins\ to install it."

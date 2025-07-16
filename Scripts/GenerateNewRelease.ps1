#
# This script, generates a new tag for the git repo, and commits it, which also causes a new release to be generated.
# If a tagged version already exists, it will detect it first, and increment the build version (Major.Minor.Build).
# There is a yml workflow in the github repo, that if it detects a "v*" tag, it will automatically publish a new release, with that tag.
# If you dont want it to just increment the build version, and want to specify a new major or minor version, just pass it argument -UseVersion.
#
# Arguments:
# StartVersion -> this is only used if there is no existing tagged version in gihub. Then it will start with this version
# UseVersion -> this is where you can tell it to use a specific version, rather than it using the automated version which it detects first.
# So most of the time, dont need UseVersion. Just let it automate it. Unless you want a new major or minor release, then specify release version yourself.
#
# Example to auto increment to next build version, and generate a release.
# .\Scripts\GenerateNewRelease.ps1
#
# Example to manually specify what version to tag, and generate a release.
# .\Scripts\GenerateNewRelease.ps1 -UseVersion v2.1.0
#

param(
    [string]$StartVersion = "v1.1.0",
    [string]$UseVersion
)

# Get the full path of the directory where this script is located
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Find the git root directory by running 'git rev-parse --show-toplevel'
$gitRoot = git -C $scriptDir rev-parse --show-toplevel 2>$null

if (-not $gitRoot) {
    Write-Error "Unable to find git root. Make sure this script is inside a git repo."
    exit 1
}

Write-Host "Git root directory detected as: $gitRoot"

# Helper function to run git commands from git root
function GitRootCmd {
    param([string[]]$args)
    & git -C $gitRoot @args
}

function Parse-Version($versionString) {
    if ($versionString.StartsWith("v")) {
        $versionString = $versionString.Substring(1)
    }
    return [System.Version]::Parse($versionString)
}

if ($UseVersion) {
    # Validate explicit version format
    if ($UseVersion -notmatch '^v\d+\.\d+\.\d+$') {
        Write-Error "UseVersion parameter must be in the format vMajor.Minor.Build, e.g. v2.0.0"
        exit 1
    }
    Write-Host "Using explicit version: $UseVersion"
    $newTag = $UseVersion
} else {
    # Fetch tags from remote
    GitRootCmd fetch --tags

    # Get all tags starting with 'v' and parse to versions
    $tags = GitRootCmd tag | Where-Object { $_ -match '^v\d+\.\d+\.\d+$' }

    if ($tags.Count -eq 0) {
        Write-Host "No existing version tags found. Starting at $StartVersion"
        $newVersion = Parse-Version $StartVersion
    } else {
        # Convert tags to System.Version objects, sort descending
        $versions = $tags | ForEach-Object { Parse-Version $_ } | Sort-Object -Descending
        $latestVersion = $versions[0]
        Write-Host "Latest existing version: v$latestVersion"

        # Increment patch version
        $newVersion = [System.Version]::new(
            $latestVersion.Major,
            $latestVersion.Minor,
            $latestVersion.Build + 1
        )
    }
    $newTag = "v$newVersion"
}

Write-Host "New version to tag: $newTag"

# Create new git tag
GitRootCmd @("tag", $newTag)

# Push tag to origin
GitRootCmd @("push", "origin", $newTag)

Write-Host "Tag $newTag created and pushed successfully to GitHub."



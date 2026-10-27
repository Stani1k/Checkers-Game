<#
.SYNOPSIS
    Regenerates THIRD-PARTY-NOTICES.md from the NuGet packages the shipped projects depend on.
.DESCRIPTION
    Covers direct and transitive packages of the Console, Server, Tools and WPF projects (test projects are excluded).
    Run from anywhere: .\scripts\Update-ThirdPartyNotices.ps1
#>
[CmdletBinding()]
param(
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $OutputPath) { $OutputPath = Join-Path $scriptDir '..\THIRD-PARTY-NOTICES.md' }
$root = Resolve-Path (Join-Path $scriptDir '..')
$projects = 'Checkers.Console', 'Checkers.Server', 'Checkers.Tools', 'Checkers.WPF'
$cache = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $HOME '.nuget\packages' }
$spdxBase = 'https://raw.githubusercontent.com/spdx/license-list-data/main/text'

$packages = @{}
foreach ($project in $projects) {
    $csproj = Join-Path $root "$project\$project.csproj"
    dotnet restore $csproj --nologo -v q | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Restore failed for $project" }
    $json = dotnet list $csproj package --include-transitive --format json | ConvertFrom-Json
    foreach ($framework in $json.projects.frameworks) {
        foreach ($p in @($framework.topLevelPackages) + @($framework.transitivePackages)) {
            if (-not $p) { continue }
            $key = "$($p.id)|$($p.resolvedVersion)"
            if (-not $packages.ContainsKey($key)) {
                $packages[$key] = [pscustomobject]@{ Id = $p.id; Version = $p.resolvedVersion; UsedBy = [System.Collections.Generic.SortedSet[string]]::new() }
            }
            [void]$packages[$key].UsedBy.Add($project)
        }
    }
}

$licenseTexts = @{}
$entries = foreach ($pkg in $packages.Values | Sort-Object Id, Version) {
    $dir = Join-Path $cache "$($pkg.Id.ToLowerInvariant())\$($pkg.Version)"
    $nuspecFile = Join-Path $dir "$($pkg.Id.ToLowerInvariant()).nuspec"
    if (-not (Test-Path $nuspecFile)) { throw "Package metadata not found: $nuspecFile" }
    $meta = ([xml](Get-Content $nuspecFile -Raw)).package.metadata

    $licenseId = $null
    $text = $null
    if ($meta.license -and $meta.license.type -eq 'expression') {
        $licenseId = $meta.license.'#text'
    }
    elseif ($meta.license -and $meta.license.type -eq 'file') {
        $file = Join-Path $dir $meta.license.'#text'
        $text = (Get-Content $file -Raw).Trim()
        $licenseId = 'See license text'
    }
    else {
        $licenseId = 'Unknown'
    }

    if (-not $text -and $licenseId -ne 'Unknown') {
        foreach ($id in ($licenseId -split '[\s()]+' | Where-Object { $_ -and $_ -notin 'OR', 'AND', 'WITH' })) {
            if (-not $licenseTexts.ContainsKey($id)) {
                try { $licenseTexts[$id] = ((Invoke-RestMethod "$spdxBase/$id.txt").ToString() -replace '(?m)^.*<(year|copyright holders)>.*\r?\n(\r?\n)?', '').Trim() }
                catch { Write-Warning "Could not download the text of license '$id'"; $licenseTexts[$id] = $null }
            }
        }
    }

    [pscustomobject]@{
        Id = $pkg.Id; Version = $pkg.Version; License = $licenseId; LicenseText = $text
        Url = if ($meta.projectUrl) { $meta.projectUrl } else { "https://www.nuget.org/packages/$($pkg.Id)/$($pkg.Version)" }
        Copyright = ($meta.copyright -as [string])
        Authors = ($meta.authors -as [string])
        UsedBy = ($pkg.UsedBy -join ', ')
    }
}

$sb = [System.Text.StringBuilder]::new()
$nl = "`n"
[void]$sb.Append("# Third-Party Notices$nl$nl")
[void]$sb.Append("Checkers Game uses the open-source software listed below. This file is generated from the NuGet package metadata by ``scripts/Update-ThirdPartyNotices.ps1`` - do not edit it by hand. It covers the packages that the application projects depend on (direct and transitive) and excludes test-only packages.$nl$nl")
[void]$sb.Append("## Packages$nl$nl")
[void]$sb.Append("| Package | Version | License | Used by |$nl|---|---|---|---|$nl")
foreach ($e in $entries) {
    [void]$sb.Append("| [$($e.Id)]($($e.Url)) | $($e.Version) | $($e.License) | $($e.UsedBy) |$nl")
}

[void]$sb.Append("$nl## License texts$nl")
$groups = $entries | Group-Object { if ($_.LicenseText) { 'file:' + $_.LicenseText } else { $_.License } } | Sort-Object { $_.Group[0].License }
foreach ($g in $groups) {
    $first = $g.Group[0]
    $text = if ($first.LicenseText) { $first.LicenseText }
            elseif ($first.License -eq 'Unknown') { $null }
            else { ($first.License -split '[\s()]+' | Where-Object { $licenseTexts[$_] } | ForEach-Object { $licenseTexts[$_] }) -join "$nl$nl---$nl$nl" }
    [void]$sb.Append("$nl### $($first.License)$nl$nl")
    [void]$sb.Append("Applies to: " + (($g.Group | ForEach-Object { "$($_.Id) $($_.Version)" }) -join ', ') + "$nl$nl")
    $notices = $g.Group | Where-Object { $_.Copyright } | ForEach-Object { $_.Copyright.Trim() } | Sort-Object -Unique
    foreach ($n in $notices) { [void]$sb.Append("- $n$nl") }
    if ($notices) { [void]$sb.Append($nl) }
    if ($text) { [void]$sb.Append("``````text$nl$text$nl``````$nl") }
    else { [void]$sb.Append("_The license text could not be determined automatically; see the package project pages above._$nl") }
}

[System.IO.File]::WriteAllText([System.IO.Path]::GetFullPath($OutputPath), $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $([System.IO.Path]::GetFullPath($OutputPath)) with $(@($entries).Count) packages."

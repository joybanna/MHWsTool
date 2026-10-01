param(
    [string]$CatalogPath = (Join-Path $PSScriptRoot '..\MHWsTool\Data\catalog.json')
)

$ErrorActionPreference = 'Stop'
$source = 'https://wilds.mhdb.io/en/skills'
$resolved = [System.IO.Path]::GetFullPath($CatalogPath)
$catalog = Get-Content -LiteralPath $resolved -Raw | ConvertFrom-Json
$rawSkills = Invoke-RestMethod -Uri $source -TimeoutSec 60

# Validate the complete match before updating this search-data snapshot.
foreach ($skill in $catalog.skills) {
    $raw = @($rawSkills | Where-Object { $_.id -eq $skill.id })
    if ($raw.Count -ne 1 -or $raw[0].name -ne $skill.name -or $raw[0].kind -ne $skill.kind) {
        throw "Skill catalog has changed: $($skill.name). Run Import-Data.ps1 to refresh the complete catalog."
    }
    if ($raw[0].ranks.Count -ne $skill.ranks.Count) { throw "Rank count changed: $($skill.name)" }
    foreach ($rank in $skill.ranks) {
        $rawRank = @($raw[0].ranks | Where-Object { $_.level -eq $rank.level })
        if ($rawRank.Count -ne 1 -or [int]$rawRank[0].setPiecesRequired -ne $rank.setPiecesRequired -or
            [string]::IsNullOrWhiteSpace($rawRank[0].description)) {
            throw "Missing or incompatible rank details: $($skill.name) Lv $($rank.level)"
        }
    }
}
foreach ($skill in $catalog.skills) {
    $raw = $rawSkills | Where-Object { $_.id -eq $skill.id }
    $skill | Add-Member -NotePropertyName description -NotePropertyValue $raw.description -Force
    foreach ($rank in $skill.ranks) {
        $rawRank = $raw.ranks | Where-Object { $_.level -eq $rank.level }
        $rank | Add-Member -NotePropertyName name -NotePropertyValue $rawRank.name -Force
        $rank | Add-Member -NotePropertyName description -NotePropertyValue $rawRank.description -Force
    }
}
$catalog | Add-Member -NotePropertyName skillDetailsSource -NotePropertyValue $source -Force
$catalog | Add-Member -NotePropertyName skillDetailsSnapshotUtc -NotePropertyValue (Get-Date).ToUniversalTime().ToString('o') -Force
$json = ConvertTo-Json -InputObject $catalog -Depth 20 -Compress
[System.IO.File]::WriteAllText($resolved, $json, [System.Text.UTF8Encoding]::new($false))
Write-Output "Saved offline descriptions and rank effects for $($catalog.skills.Count) skills."

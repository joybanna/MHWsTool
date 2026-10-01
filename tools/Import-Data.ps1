param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\MHWsTool\Data\catalog.json')
)

$ErrorActionPreference = 'Stop'
$base = 'https://wilds.mhdb.io/en'

function Get-Data([string]$Path) {
    Write-Host "Fetching $Path..."
    return @(Invoke-RestMethod -Uri "$base/$Path" -TimeoutSec 60)
}

function Convert-Skills($Entries) {
    return @($Entries | ForEach-Object {
        [ordered]@{
            name = $_.skill.name
            kind = $_.skill.kind
            level = [int]$_.level
            setPiecesRequired = if ($null -eq $_.setPiecesRequired) { 0 } else { [int]$_.setPiecesRequired }
        }
    } | Where-Object { $_.name -and $_.level -gt 0 })
}

$armorRaw = Get-Data 'armor'
$decorationRaw = Get-Data 'decorations'
$charmRaw = Get-Data 'charms'
$skillRaw = Get-Data 'skills'
$weaponRaw = Get-Data 'weapons'

$armor = @($armorRaw | Where-Object { $_.name -and $_.kind -in @('head', 'chest', 'arms', 'waist', 'legs') } | ForEach-Object {
    [ordered]@{
        id = [int]$_.id
        name = $_.name
        kind = $_.kind
        rank = $_.rank
        rarity = [int]$_.rarity
        defense = [int]$_.defense.base
        resistances = [ordered]@{
            fire = [int]$_.resistances.fire
            water = [int]$_.resistances.water
            ice = [int]$_.resistances.ice
            thunder = [int]$_.resistances.thunder
            dragon = [int]$_.resistances.dragon
        }
        armorSet = $_.armorSet.name
        slots = @($_.slots | ForEach-Object { [int]$_ })
        skills = @(Convert-Skills $_.skills)
    }
})

$decorations = @($decorationRaw | Where-Object { $_.name -and $_.slot -gt 0 } | ForEach-Object {
    [ordered]@{
        id = [int]$_.id
        name = $_.name
        kind = $_.kind
        slot = [int]$_.slot
        skills = @(Convert-Skills $_.skills)
    }
})

$charms = @($charmRaw | ForEach-Object {
    $parentId = [int]$_.id
    $_.ranks | Where-Object { $_.name } | ForEach-Object {
        [ordered]@{
            id = "$parentId-$($_.id)"
            name = $_.name
            rarity = [int]$_.rarity
            skills = @(Convert-Skills $_.skills)
        }
    }
})

$skills = @($skillRaw | Where-Object { $_.name -and $_.kind } | ForEach-Object {
    [ordered]@{
        id = [int]$_.id
        name = $_.name
        kind = $_.kind
        maxLevel = [int](@($_.ranks | ForEach-Object { $_.level } | Measure-Object -Maximum).Maximum)
        description = $_.description
        ranks = @($_.ranks | ForEach-Object {
            [ordered]@{
                level = [int]$_.level
                description = $_.description
                setPiecesRequired = if ($null -eq $_.setPiecesRequired) { 0 } else { [int]$_.setPiecesRequired }
                name = $_.name
            }
        })
    }
})

$weapons = @($weaponRaw | Where-Object { $_.name -and $_.kind } | ForEach-Object {
    [ordered]@{
        id = [int]$_.id
        name = $_.name
        kind = $_.kind
        rarity = [int]$_.rarity
        slots = @($_.slots | ForEach-Object { [int]$_ })
        skills = @(Convert-Skills $_.skills)
    }
})

$catalog = [ordered]@{
    source = 'Monster Hunter Wilds DB (https://wilds.mhdb.io/)'
    snapshotUtc = (Get-Date).ToUniversalTime().ToString('o')
    skillDetailsSource = "$base/skills"
    skillDetailsSnapshotUtc = (Get-Date).ToUniversalTime().ToString('o')
    armor = $armor
    decorations = $decorations
    charms = $charms
    skills = $skills
    weapons = $weapons
}

$resolved = [System.IO.Path]::GetFullPath($OutputPath)
[System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($resolved)) | Out-Null
$json = ConvertTo-Json -InputObject $catalog -Depth 20 -Compress
[System.IO.File]::WriteAllText($resolved, $json, [System.Text.UTF8Encoding]::new($false))
Write-Host "Saved $resolved"
Write-Host "Armor: $($armor.Count); decorations: $($decorations.Count); charms: $($charms.Count); skills: $($skills.Count); weapons: $($weapons.Count)"

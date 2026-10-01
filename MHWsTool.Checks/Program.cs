using System.Diagnostics;
using MHWsTool;

var catalog = Catalog.Load();
Check(catalog.Armor.Count == 714, "all armor loaded");
Check(catalog.Decorations.Count == 361, "all decorations loaded");
Check(catalog.Charms.Count == 187, "all talismans loaded");
Check(catalog.Skills.Count == 179, "all skills loaded");
Check(catalog.Weapons.Count == 1188, "all weapons loaded");
Check(catalog.Skills.All(s => s.Ranks.Count > 0 && s.Ranks.All(r => !string.IsNullOrWhiteSpace(r.Description))),
    "every skill, set and group has offline effects for all ranks");
Check(catalog.SkillDetailsSnapshotUtc != default && catalog.SkillDetailsSource == "https://wilds.mhdb.io/en/skills",
    "skill descriptions retain their source and snapshot date");
var fortifyInfo = new SkillInfoEntry(catalog.Skills.Single(s => s.Name == "Fortifying Pelt"), catalog);
Check(fortifyInfo.Matches("Fortify") && fortifyInfo.Matches("fainting") && fortifyInfo.Ranks.Single().Requirement == "3 armor pieces",
    "group info finds bonus names and effects and shows the three-piece threshold");
var arkveldInfo = new SkillInfoEntry(catalog.Skills.Single(s => s.Name == "Arkveld's Hunger"), catalog);
Check(arkveldInfo.Ranks.Select(r => r.Requirement).SequenceEqual(new[] { "2 armor pieces", "4 armor pieces" })
    && arkveldInfo.Ranks[1].Title.Contains("Hasten Recovery II") && arkveldInfo.ArmorSets.Contains("Arkveld"),
    "set info preserves tier names, thresholds and armor-set sources");

var stopwatch = Stopwatch.StartNew();
var freeMeal = SearchEngine.Find(catalog, new SearchRequest
{
    Rank = "high",
    NoCharm = true,
    Targets = new Dictionary<string, int> { ["Free Meal"] = 1 }
});
Check(freeMeal.Count > 0 && freeMeal[0].Complete, "high-rank skill search finds a complete build");
Check(freeMeal.All(r => r.Armor.Count == 5 && r.Armor.All(a => a.Rank == "high")), "high-rank results have five high-rank pieces");
Check(freeMeal.All(r => r.Charm is null), "no-talisman choice is respected");
Check(freeMeal.All(r => r.SkillLabels.Count == r.Skills.Count && r.Skills.Count >= 1),
    "results retain every active skill with a display label");
Check(freeMeal.All(r => r.OpenSlotDetails.Count == r.OpenSlots), "results retain every unused slot and its size");
Check(freeMeal.All(r => r.Resistances.Fire == r.Armor.Sum(a => a.Resistances.Fire) &&
    r.Resistances.Water == r.Armor.Sum(a => a.Resistances.Water) &&
    r.Resistances.Ice == r.Armor.Sum(a => a.Resistances.Ice) &&
    r.Resistances.Thunder == r.Armor.Sum(a => a.Resistances.Thunder) &&
    r.Resistances.Dragon == r.Armor.Sum(a => a.Resistances.Dragon)),
    "result element values equal the five armor pieces' resistances");

var groupSkill = SearchEngine.Find(catalog, new SearchRequest
{
    Rank = "high",
    NoCharm = true,
    Targets = new Dictionary<string, int> { ["Fortifying Pelt"] = 1 }
});
Check(groupSkill.Count > 0 && groupSkill[0].Complete, "group skill activates from required armor pieces");

var setTierTwo = SearchEngine.Find(catalog, new SearchRequest
{
    Rank = "high",
    NoCharm = true,
    Targets = new Dictionary<string, int> { ["Fulgur Anjanath's Will"] = 2 }
});
Check(setTierTwo.Count > 0 && setTierTwo[0].Complete, "four-piece set bonus reaches level two");
Check(setTierTwo.All(result => result.SkillLabels["Fulgur Anjanath's Will"].Contains(':')),
    "active set bonuses use their tier names in result badges");

var blocked = SearchEngine.Find(catalog, new SearchRequest
{
    Rank = "high",
    Targets = new Dictionary<string, int> { ["Free Meal"] = 1 },
    ExcludedArmor = catalog.Armor.Where(a => a.Kind == "head" && a.Rank == "high").Select(a => a.Id).ToHashSet()
});
Check(blocked.Count == 0, "armor exclusions prevent impossible search");

var lowRank = SearchEngine.Find(catalog, new SearchRequest
{
    Rank = "low",
    NoCharm = true,
    Targets = new Dictionary<string, int> { ["Botanist"] = 1 }
});
Check(lowRank.Count > 0 && lowRank.All(r => r.Armor.All(a => a.Rank == "low")), "low-rank filter is respected");

var customCharm = new Charm { Id = "check", Name = "Check charm", IsCustom = true, WeaponSlots = [1] };
var weaponSlot = SearchEngine.Find(catalog, new SearchRequest
{
    Rank = "high",
    SelectedCharm = customCharm,
    Targets = new Dictionary<string, int> { ["Poison Attack"] = 1 }
});
Check(weaponSlot.Count > 0 && weaponSlot[0].Complete && weaponSlot[0].Decorations.Any(d => d.Kind == "weapon"),
    "custom talisman weapon slot accepts weapon decorations");

var matchCatalog = new Catalog
{
    Skills =
    [
        new GameSkill { Name = "Free Meal", Kind = "armor", MaxLevel = 3 },
        new GameSkill { Name = "Speed Eating", Kind = "armor", MaxLevel = 3 }
    ],
    Armor =
    [
        new Armor { Id = 1, Kind = "head", Rank = "high", Defense = 10,
            Skills = [new SkillValue { Name = "Free Meal", Level = 2 }, new SkillValue { Name = "Speed Eating", Level = 1 }] },
        new Armor { Id = 2, Kind = "head", Rank = "high", Defense = 100,
            Skills = [new SkillValue { Name = "Free Meal", Level = 3 }] },
        new Armor { Id = 3, Kind = "head", Rank = "high", Defense = 200 },
        new Armor { Id = 4, Kind = "chest", Rank = "high" },
        new Armor { Id = 5, Kind = "arms", Rank = "high" },
        new Armor { Id = 6, Kind = "waist", Rank = "high" },
        new Armor { Id = 7, Kind = "legs", Rank = "high" }
    ]
};
var matchesOnly = SearchEngine.Find(matchCatalog, new SearchRequest
{
    NoCharm = true,
    Targets = new Dictionary<string, int> { ["Free Meal"] = 2, ["Speed Eating"] = 1 }
});
Check(matchesOnly.Count == 1 && matchesOnly[0].Armor[0].Id == 1,
    "search omits partial builds even when they have higher defense or excess levels in another skill");

var noMatch = SearchEngine.Find(matchCatalog, new SearchRequest
{
    NoCharm = true,
    Targets = new Dictionary<string, int> { ["Free Meal"] = 2, ["Speed Eating"] = 2 }
});
Check(noMatch.Count == 0, "search returns no suggestions when armor candidates cannot meet every required level");

var providedWeapon = new Weapon
{
    Name = "Fixed weapon", Skills = [new SkillValue { Name = "Speed Eating", Level = 2 }]
};
var manualWeaponRequest = new SearchRequest
{
    NoCharm = true,
    Weapon = providedWeapon,
    WeaponSkillsOverride = [new SkillValue { Name = "Speed Eating", Level = 1 }],
    Targets = new Dictionary<string, int> { ["Speed Eating"] = 2 }
};
var manualWeapon = SearchEngine.Find(matchCatalog, manualWeaponRequest);
Check(manualWeapon.Count == 1 && manualWeapon[0].Armor[0].Id == 1 && manualWeapon[0].Skills["Speed Eating"] == 2,
    "manual weapon skills replace catalog skills without counting them twice");
var weaponProgress = SearchEngine.GetRequirementProgress(matchCatalog, manualWeaponRequest)["Speed Eating"];
Check(weaponProgress.Provided == 1 && weaponProgress.Remaining == 1,
    "ordinary skill requirements subtract provided weapon levels");

var providedCharm = new Charm
{
    Id = "fixed", Name = "Fixed charm", Skills = [new SkillValue { Name = "Speed Eating", Level = 2 }]
};
var manualCharm = SearchEngine.Find(matchCatalog, new SearchRequest
{
    SelectedCharm = providedCharm,
    TalismanSkillsOverride = [new SkillValue { Name = "Speed Eating", Level = 1 }],
    Targets = new Dictionary<string, int> { ["Speed Eating"] = 2 }
});
Check(manualCharm.Count == 1 && manualCharm[0].Armor[0].Id == 1,
    "manual talisman skills replace catalog skills without counting them twice");

var suppliedTargets = SearchEngine.Find(matchCatalog, new SearchRequest
{
    WeaponSkillsOverride = [new SkillValue { Name = "Free Meal", Level = 1 }, new SkillValue { Name = "Speed Eating", Level = 1 }],
    TalismanSkillsOverride = [new SkillValue { Name = "Free Meal", Level = 1 }, new SkillValue { Name = "Speed Eating", Level = 1 }],
    Targets = new Dictionary<string, int> { ["Free Meal"] = 2, ["Speed Eating"] = 2 }
});
Check(suppliedTargets.Count > 0 && suppliedTargets[0].Armor[0].Id == 3 && suppliedTargets[0].Complete,
    "weapon and talisman levels combine and fully supplied targets allow armor with no target skills");

var explicitEmptyWeapon = SearchEngine.Find(matchCatalog, new SearchRequest
{
    NoCharm = true, Weapon = providedWeapon, WeaponSkillsOverride = [],
    Targets = new Dictionary<string, int> { ["Speed Eating"] = 2 }
});
Check(explicitEmptyWeapon.Count == 0, "empty manual equipment skills do not silently restore catalog skills");
var noManualCharm = SearchEngine.Find(matchCatalog, new SearchRequest
{
    NoCharm = true, TalismanSkillsOverride = [new SkillValue { Name = "Speed Eating", Level = 2 }],
    Targets = new Dictionary<string, int> { ["Speed Eating"] = 2 }
});
Check(noManualCharm.Count == 0, "no-talisman choice also excludes manually provided talisman skills");

var bonusCatalog = new Catalog
{
    Skills =
    [
        new GameSkill { Name = "Test Set", Kind = "set", MaxLevel = 2,
            Ranks = [new SkillRank { Level = 1, SetPiecesRequired = 2 }, new SkillRank { Level = 2, SetPiecesRequired = 4 }] },
        new GameSkill { Name = "Test Group", Kind = "group", MaxLevel = 1,
            Ranks = [new SkillRank { Level = 1, SetPiecesRequired = 3 }] }
    ],
    Armor =
    [
        new Armor { Id = 1, Kind = "head", Rank = "high", Skills =
            [new SkillValue { Name = "Test Set", Level = 1, SetPiecesRequired = 2 }, new SkillValue { Name = "Test Group", Level = 1, SetPiecesRequired = 3 }] },
        new Armor { Id = 2, Kind = "chest", Rank = "high", Skills =
            [new SkillValue { Name = "Test Set", Level = 1, SetPiecesRequired = 2 }, new SkillValue { Name = "Test Group", Level = 1, SetPiecesRequired = 3 }] },
        new Armor { Id = 3, Kind = "arms", Rank = "high" },
        new Armor { Id = 4, Kind = "waist", Rank = "high" },
        new Armor { Id = 5, Kind = "legs", Rank = "high" }
    ]
};
var suppliedBonusesRequest = new SearchRequest
{
    WeaponSkillsOverride =
        [new SkillValue { Name = "Test Set", Level = 1, SetPiecesRequired = 2 }, new SkillValue { Name = "Test Group", Level = 1, SetPiecesRequired = 3 }],
    TalismanSkillsOverride = [new SkillValue { Name = "Test Set", Level = 1, SetPiecesRequired = 2 }],
    Targets = new Dictionary<string, int> { ["Test Set"] = 2, ["Test Group"] = 1 }
};
var bonusProgress = SearchEngine.GetRequirementProgress(bonusCatalog, suppliedBonusesRequest);
Check(bonusProgress["Test Set"].Provided == 2 && bonusProgress["Test Set"].Remaining == 2 &&
    bonusProgress["Test Group"].Provided == 1 && bonusProgress["Test Group"].Remaining == 2,
    "set and group requirements subtract equipment piece contributions instead of bonus levels");
var suppliedBonuses = SearchEngine.Find(bonusCatalog, suppliedBonusesRequest);
Check(suppliedBonuses.Count == 1 && suppliedBonuses[0].Skills["Test Set"] == 2 && suppliedBonuses[0].Skills["Test Group"] == 1,
    "weapon and talisman bonus contributions activate set and group tiers with fewer armor pieces");
var insufficientBonus = SearchEngine.Find(bonusCatalog, new SearchRequest
{
    NoCharm = true,
    WeaponSkillsOverride = [new SkillValue { Name = "Test Set", Level = 1, SetPiecesRequired = 2 }],
    Targets = new Dictionary<string, int> { ["Test Set"] = 2 }
});
Check(insufficientBonus.Count == 0, "three total set pieces cannot unlock a four-piece tier");

var decoCatalog = new Catalog
{
    Skills = [new GameSkill { Name = "Free Meal", Kind = "armor", MaxLevel = 3 }],
    Armor = [.. matchCatalog.Armor.Where(armor => armor.Id >= 3).Select(armor => new Armor
        { Id = armor.Id, Kind = armor.Kind, Rank = armor.Rank, Slots = armor.Kind == "head" ? [1] : [] })],
    Decorations = [new Decoration { Id = 1, Name = "Free Meal Jewel", Kind = "armor", Slot = 1,
        Skills = [new SkillValue { Name = "Free Meal", Level = 1 }] }]
};
var reducedDecos = SearchEngine.Find(decoCatalog, new SearchRequest
{
    WeaponSkillsOverride = [new SkillValue { Name = "Free Meal", Level = 1 }], NoCharm = true,
    Targets = new Dictionary<string, int> { ["Free Meal"] = 2 }
});
Check(reducedDecos.Count == 1 && reducedDecos[0].Decorations.Count == 1 && reducedDecos[0].Skills["Free Meal"] == 2,
    "decoration placement fills only levels still missing after equipment skills");
var decorationDisplay = new BuildDisplay(1, reducedDecos[0], new Dictionary<string, int> { ["Free Meal"] = 2 });
Check(decorationDisplay.DecorationGroups.Count == 1 && decorationDisplay.DecorationGroups[0].Text == "Free Meal Jewel [1]" &&
    decorationDisplay.DecorationGroupsVisibility == System.Windows.Visibility.Visible,
    "result card groups equipped decorations into named level badges");
Check(decorationDisplay.SkillPills.Single(skill => skill.Label == "Free Meal").IsRequested,
    "result card marks requested skills for highlighted badges");
Check(decorationDisplay.Elements.Select(element => element.Name).SequenceEqual(new[] { "Fire", "Water", "Thunder", "Ice", "Dragon" }),
    "result card follows the reference element order");
var noExtraDecos = SearchEngine.Find(decoCatalog, new SearchRequest
{
    WeaponSkillsOverride = [new SkillValue { Name = "Free Meal", Level = 2 }], NoCharm = true,
    Targets = new Dictionary<string, int> { ["Free Meal"] = 2 }
});
Check(noExtraDecos.Count == 1 && noExtraDecos[0].Decorations.Count == 0 && noExtraDecos[0].OpenSlots == 1,
    "fully supplied requirements keep decoration slots unused");

Console.WriteLine($"Checks passed in {stopwatch.Elapsed.TotalSeconds:F1}s");

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception($"FAILED: {message}");
    Console.WriteLine($"PASS: {message}");
}

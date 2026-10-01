namespace MHWsTool;

public sealed class SearchRequest
{
    public string Rank { get; init; } = "high";
    public Dictionary<string, int> Targets { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<int> ExcludedArmor { get; init; } = [];
    public HashSet<int> ExcludedDecorations { get; init; } = [];
    public HashSet<string> ExcludedCharms { get; init; } = [];
    public Weapon? Weapon { get; init; }
    public Charm? SelectedCharm { get; init; }
    // Null uses the item's catalog skills. An empty list explicitly supplies no skills.
    public List<SkillValue>? WeaponSkillsOverride { get; init; }
    public List<SkillValue>? TalismanSkillsOverride { get; init; }
    public bool NoCharm { get; init; }
}

public sealed record RequirementProgress(string Name, bool IsBonus, int Required, int Provided)
{
    public int Remaining => Math.Max(0, Required - Provided);
    public string Summary => IsBonus
        ? $"From gear {Provided} piece(s) · Need {Remaining} more of {Required} pieces"
        : $"From gear Lv {Provided} · Need Lv {Remaining} more";
}

public sealed class BuildResult
{
    public required List<Armor> Armor { get; init; }
    public Charm? Charm { get; init; }
    public required List<Decoration> Decorations { get; init; }
    public required Dictionary<string, int> Skills { get; init; }
    public required Dictionary<string, string> SkillLabels { get; init; }
    public required List<BuildSlot> OpenSlotDetails { get; init; }
    public required ElementResistances Resistances { get; init; }
    public List<SkillValue> EquipmentSkills { get; init; } = [];
    public int MissingLevels { get; init; }
    public int Defense { get; init; }
    public int OpenSlots { get; init; }
    public bool Complete => MissingLevels == 0;
    public string ArmorText => string.Join("  ·  ", Armor.Select(a => a.Name));
    public string DecorationText => Decorations.Count == 0 ? "None" : string.Join(", ", Decorations.GroupBy(x => x.Name).Select(g => $"{g.Key} ×{g.Count()}"));
    public string SkillText => string.Join("  ·  ", Skills.OrderBy(x => x.Key).Select(x => $"{x.Key} {x.Value}"));
}

public sealed record BuildSlot(int Size, string Kind);

public static class SearchEngine
{
    private static readonly string[] ArmorKinds = ["head", "chest", "arms", "waist", "legs"];
    private const int PerSlotLimit = 38;
    private const int BeamWidth = 2000;
    private const int CharmLimit = 12;

    public static Dictionary<string, RequirementProgress> GetRequirementProgress(Catalog catalog, SearchRequest request)
    {
        var fixedSkills = WeaponSkills(request).Concat(TalismanSkills(request, request.SelectedCharm)).ToArray();
        var definitions = catalog.Skills.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
        return request.Targets.ToDictionary(target => target.Key, target =>
        {
            var definition = definitions.GetValueOrDefault(target.Key);
            var isBonus = definition?.Kind is "set" or "group";
            var required = isBonus
                ? definition!.Ranks.FirstOrDefault(rank => rank.Level == target.Value)?.SetPiecesRequired ?? target.Value
                : target.Value;
            var provided = isBonus
                ? fixedSkills.Count(skill => skill.Name.Equals(target.Key, StringComparison.OrdinalIgnoreCase) && skill.SetPiecesRequired > 0)
                : fixedSkills.Where(skill => skill.Name.Equals(target.Key, StringComparison.OrdinalIgnoreCase) && skill.SetPiecesRequired == 0).Sum(skill => skill.Level);
            return new RequirementProgress(target.Key, isBonus, required, provided);
        }, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<SkillValue> WeaponSkills(SearchRequest request) =>
        request.WeaponSkillsOverride ?? request.Weapon?.Skills ?? [];

    private static IEnumerable<SkillValue> TalismanSkills(SearchRequest request, Charm? charm) =>
        request.NoCharm ? [] : request.TalismanSkillsOverride ?? charm?.Skills ?? [];

    public static IReadOnlyList<BuildResult> Find(Catalog catalog, SearchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Targets.Count == 0) return [];

        var skillKinds = catalog.Skills.ToDictionary(s => s.Name, s => s.Kind, StringComparer.OrdinalIgnoreCase);
        var bonusRanks = catalog.Skills.Where(s => s.Kind is "set" or "group")
            .ToDictionary(s => s.Name, s => s.Ranks, StringComparer.OrdinalIgnoreCase);
        var remainingTargets = GetRequirementProgress(catalog, request).Values
            .Where(progress => progress.Remaining > 0)
            .ToDictionary(progress => progress.Name, progress => progress.Remaining, StringComparer.OrdinalIgnoreCase);
        var availableDecorations = catalog.Decorations
            .Where(d => !request.ExcludedDecorations.Contains(d.Id))
            .Where(d => d.Skills.Any(s => request.Targets.ContainsKey(s.Name)))
            .ToArray();

        var candidates = new List<Armor>[ArmorKinds.Length];
        for (var slot = 0; slot < ArmorKinds.Length; slot++)
        {
            candidates[slot] = catalog.Armor
                .Where(a => a.Kind == ArmorKinds[slot] && a.Rank == request.Rank && !request.ExcludedArmor.Contains(a.Id))
                .OrderByDescending(a => ArmorScore(a, remainingTargets))
                .Take(PerSlotLimit)
                .ToList();
            if (candidates[slot].Count == 0) return [];
        }

        var beam = new List<ArmorState> { new([], 0) };
        foreach (var slotItems in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            beam = beam.SelectMany(state => slotItems.Select(item =>
                    new ArmorState([.. state.Items, item], 0)))
                .Select(state => state with { Score = StateScore(state.Items, remainingTargets) })
                .OrderByDescending(state => state.Score)
                .Take(BeamWidth)
                .ToList();
        }

        var charms = ChooseCharms(catalog, request, remainingTargets);
        var results = new List<BuildResult>();
        foreach (var state in beam)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var charm in charms)
            {
                results.Add(Evaluate(state.Items, request, charm, skillKinds, bonusRanks, availableDecorations));
            }
        }

        return results
            .Where(r => r.Complete)
            .OrderByDescending(r => r.Defense)
            .ThenByDescending(r => r.OpenSlots)
            .GroupBy(r => string.Join('-', r.Armor.Select(a => a.Id)))
            .Select(g => g.First())
            .Take(30)
            .ToList();
    }

    private static double ArmorScore(Armor armor, Dictionary<string, int> targets)
    {
        double score = armor.Defense * 0.015 + armor.Slots.Sum(x => x * 1.7);
        foreach (var skill in armor.Skills)
        {
            if (!targets.TryGetValue(skill.Name, out var target)) continue;
            score += skill.SetPiecesRequired > 0 ? 12 : Math.Min(skill.Level, target) * 12;
        }
        return score;
    }

    private static double StateScore(List<Armor> items, Dictionary<string, int> targets)
    {
        var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var bonusCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        double score = items.Sum(a => a.Defense) * 0.015 + items.Sum(a => a.Slots.Sum(x => x * 1.7));
        foreach (var skill in items.SelectMany(a => a.Skills))
        {
            if (!targets.ContainsKey(skill.Name)) continue;
            if (skill.SetPiecesRequired > 0) bonusCounts[skill.Name] = bonusCounts.GetValueOrDefault(skill.Name) + 1;
            else levels[skill.Name] = levels.GetValueOrDefault(skill.Name) + skill.Level;
        }
        foreach (var (name, target) in targets)
        {
            score += Math.Min(levels.GetValueOrDefault(name), target) * 12;
            score += Math.Min(bonusCounts.GetValueOrDefault(name), target) * 6;
        }
        return score;
    }

    private static List<Charm?> ChooseCharms(Catalog catalog, SearchRequest request, Dictionary<string, int> remainingTargets)
    {
        if (request.NoCharm) return [null];
        if (request.TalismanSkillsOverride is not null) return [request.SelectedCharm];
        if (request.SelectedCharm is not null) return [request.SelectedCharm];
        var ranked = catalog.Charms
            .Where(c => !request.ExcludedCharms.Contains(c.Id))
            .Select(c => (Charm: c, Score: c.Skills.Sum(s => remainingTargets.TryGetValue(s.Name, out var target) ? Math.Min(s.Level, target) * 10 : 0) + c.Slots.Sum(x => x * 2)))
            .OrderByDescending(x => x.Score)
            .Take(CharmLimit)
            .Select(x => (Charm?)x.Charm)
            .ToList();
        ranked.Add(null);
        return ranked;
    }

    private static BuildResult Evaluate(List<Armor> armor, SearchRequest request, Charm? charm,
        Dictionary<string, string> skillKinds,
        Dictionary<string, List<SkillRank>> bonusRanks, Decoration[] decorations)
    {
        var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var targets = request.Targets;
        var weapon = request.Weapon;
        var equipmentSkills = WeaponSkills(request).Concat(TalismanSkills(request, charm)).ToList();
        var allSkills = armor.SelectMany(a => a.Skills)
            .Concat(equipmentSkills)
            .ToArray();

        foreach (var skill in allSkills.Where(s => s.SetPiecesRequired == 0))
            levels[skill.Name] = levels.GetValueOrDefault(skill.Name) + skill.Level;

        foreach (var bonus in allSkills.Where(s => s.SetPiecesRequired > 0).GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
        {
            var active = bonusRanks.GetValueOrDefault(bonus.Key)?
                .Where(rank => bonus.Count() >= rank.SetPiecesRequired)
                .Select(rank => rank.Level)
                .DefaultIfEmpty(0)
                .Max() ?? 0;
            if (active > 0) levels[bonus.Key] = Math.Max(levels.GetValueOrDefault(bonus.Key), active);
        }

        var slots = armor.SelectMany(a => a.Slots.Select(size => (Size: size, Kind: "armor")))
            .Concat((weapon?.Slots ?? []).Select(size => (Size: size, Kind: "weapon")))
            .Concat((charm?.Slots ?? []).Select(size => (Size: size, Kind: "armor")))
            .Concat((charm?.WeaponSlots ?? []).Select(size => (Size: size, Kind: "weapon")))
            .OrderBy(x => x.Size)
            .ToArray();
        var placed = new List<Decoration>();
        var openSlotDetails = new List<BuildSlot>();
        var openSlots = 0;
        foreach (var slot in slots)
        {
            var best = decorations
                .Where(d => d.Kind == slot.Kind && d.Slot <= slot.Size)
                .Select(d => (Decoration: d, Value: d.Skills.Sum(s =>
                    targets.TryGetValue(s.Name, out var target) &&
                    skillKinds.GetValueOrDefault(s.Name) == slot.Kind
                        ? Math.Min(s.Level, Math.Max(0, target - levels.GetValueOrDefault(s.Name)))
                        : 0)))
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value)
                .ThenByDescending(x => x.Decoration.Slot)
                .FirstOrDefault();
            if (best.Decoration is null)
            {
                openSlots++;
                openSlotDetails.Add(new BuildSlot(slot.Size, slot.Kind));
                continue;
            }
            placed.Add(best.Decoration);
            foreach (var skill in best.Decoration.Skills)
                levels[skill.Name] = levels.GetValueOrDefault(skill.Name) + skill.Level;
        }

        var missing = targets.Sum(t => Math.Max(0, t.Value - levels.GetValueOrDefault(t.Key)));
        var skillLabels = levels.ToDictionary(x => x.Key, x => $"{x.Key} Lv{x.Value}", StringComparer.OrdinalIgnoreCase);
        foreach (var (name, ranks) in bonusRanks)
        {
            var level = levels.GetValueOrDefault(name);
            if (level <= 0) continue;
            var rankName = ranks.FirstOrDefault(rank => rank.Level == level)?.Name;
            skillLabels[name] = string.IsNullOrWhiteSpace(rankName) ? $"{name} Lv{level}" : $"{name}: {rankName}";
        }
        return new BuildResult
        {
            Armor = armor,
            Charm = charm,
            Decorations = placed,
            Skills = levels,
            SkillLabels = skillLabels,
            OpenSlotDetails = openSlotDetails,
            EquipmentSkills = equipmentSkills,
            MissingLevels = missing,
            Defense = armor.Sum(a => a.Defense),
            OpenSlots = openSlots,
            Resistances = new ElementResistances
            {
                Fire = armor.Sum(a => a.Resistances.Fire),
                Water = armor.Sum(a => a.Resistances.Water),
                Ice = armor.Sum(a => a.Resistances.Ice),
                Thunder = armor.Sum(a => a.Resistances.Thunder),
                Dragon = armor.Sum(a => a.Resistances.Dragon)
            }
        };
    }

    private sealed record ArmorState(List<Armor> Items, double Score);
}

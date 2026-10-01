namespace MHWsTool;

public sealed class SkillInfoEntry
{
    public GameSkill Skill { get; }
    public string Name => Skill.Name;
    public string Category => Skill.Kind switch
    {
        "weapon" => "Weapon Skill",
        "armor" => "Armor Skill",
        "set" => "Set / Series Bonus",
        "group" => "Group Bonus",
        _ => Skill.Kind
    };
    public string Summary { get; }
    public string Activation => Skill.Kind switch
    {
        "set" => "Equip armor carrying this same set bonus. The required piece count for each tier is shown below; tiers do not add together.",
        "group" => "Equip the required number of armor pieces carrying this group bonus. You can combine pieces from different armor sets.",
        "weapon" => "Gain levels from weapons and weapon decorations. Custom talismans may also provide this skill. Effects depend on the weapon and conditions described below.",
        _ => "Gain levels from armor, talismans and armor decorations. Skill levels add together, up to the maximum shown below."
    };
    public string LevelSummary => $"{Category} · Max Lv {Skill.MaxLevel}";
    public IReadOnlyList<SkillInfoRank> Ranks { get; }
    public string ArmorSets { get; }
    public bool Matches(string query) => Name.Contains(query, StringComparison.OrdinalIgnoreCase)
        || Summary.Contains(query, StringComparison.OrdinalIgnoreCase)
        || Ranks.Any(rank => rank.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || rank.Effect.Contains(query, StringComparison.OrdinalIgnoreCase));

    public SkillInfoEntry(GameSkill skill, Catalog catalog)
    {
        Skill = skill;
        Summary = !string.IsNullOrWhiteSpace(skill.Description) ? skill.Description
            : skill.Ranks.OrderBy(r => r.Level).FirstOrDefault()?.Description ?? "No description in this data snapshot.";
        Ranks = skill.Ranks.OrderBy(r => r.Level).Select(r => new SkillInfoRank(
            string.IsNullOrWhiteSpace(r.Name) ? $"Lv {r.Level}" : $"Lv {r.Level} · {r.Name}",
            r.SetPiecesRequired > 0 ? $"{r.SetPiecesRequired} armor pieces" : $"{r.Level} skill point{(r.Level == 1 ? "" : "s")}",
            string.IsNullOrWhiteSpace(r.Description) ? "No level effect in this data snapshot." : r.Description)).ToList();
        var sets = catalog.Armor.Where(a => a.Skills.Any(s => s.Name.Equals(skill.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(a => a.ArmorSet).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().Order().ToList();
        ArmorSets = sets.Count > 0 ? string.Join(" · ", sets)
            : "No armor in this snapshot carries this skill directly. Check weapons, decorations or talismans.";
    }
}

public sealed record SkillInfoRank(string Title, string Requirement, string Effect);

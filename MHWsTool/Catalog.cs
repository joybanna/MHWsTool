using System.Reflection;
using System.Text.Json;

namespace MHWsTool;

public sealed class Catalog
{
    public string Source { get; set; } = "";
    public DateTime SnapshotUtc { get; set; }
    public string SkillDetailsSource { get; set; } = "";
    public DateTime SkillDetailsSnapshotUtc { get; set; }
    public List<Armor> Armor { get; set; } = [];
    public List<Decoration> Decorations { get; set; } = [];
    public List<Charm> Charms { get; set; } = [];
    public List<GameSkill> Skills { get; set; } = [];
    public List<Weapon> Weapons { get; set; } = [];

    public static Catalog Load()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("MHWsTool.Data.catalog.json")
            ?? throw new InvalidOperationException("Embedded catalog was not found.");
        return JsonSerializer.Deserialize<Catalog>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Embedded catalog is empty.");
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}

public sealed class SkillValue
{
    public string Name { get; set; } = "";
    public string? Kind { get; set; }
    public int Level { get; set; }
    public int SetPiecesRequired { get; set; }
}

public sealed class Armor
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Rank { get; set; } = "";
    public int Rarity { get; set; }
    public int Defense { get; set; }
    public ElementResistances Resistances { get; set; } = new();
    public string? ArmorSet { get; set; }
    public List<int> Slots { get; set; } = [];
    public List<SkillValue> Skills { get; set; } = [];
}

public sealed class Decoration
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public int Slot { get; set; }
    public List<SkillValue> Skills { get; set; } = [];
}

public sealed class Charm
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Rarity { get; set; }
    public List<int> Slots { get; set; } = [];
    public List<int> WeaponSlots { get; set; } = [];
    public List<SkillValue> Skills { get; set; } = [];
    public bool IsCustom { get; set; }
}

public sealed class GameSkill
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public string? Description { get; set; }
    public int MaxLevel { get; set; }
    public List<SkillRank> Ranks { get; set; } = [];
    public override string ToString() => $"{Name}  ·  {Kind}";
}

public sealed class SkillRank
{
    public int Level { get; set; }
    public string? Description { get; set; }
    public int SetPiecesRequired { get; set; }
    public string? Name { get; set; }
}

public sealed class ElementResistances
{
    public int Fire { get; set; }
    public int Water { get; set; }
    public int Ice { get; set; }
    public int Thunder { get; set; }
    public int Dragon { get; set; }
}

public sealed class Weapon
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public int Rarity { get; set; }
    public List<int> Slots { get; set; } = [];
    public List<SkillValue> Skills { get; set; } = [];
    public override string ToString() => $"{Name}  ·  {Kind}  ·  R{Rarity}";
}

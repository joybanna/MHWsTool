using System.ComponentModel;
using System.Windows.Media;

namespace MHWsTool;

public enum SkillSelectionSource { Required, Weapon, Talisman }

public sealed class SkillSelectionRow : INotifyPropertyChanged
{
    private SkillLevelOption _selectedOption;
    private string _progressText = "";
    public GameSkill Skill { get; }
    public SkillSelectionSource Source { get; }
    public string Name => Skill.Name;
    public string KindLabel => Skill.Kind switch
    {
        "weapon" => "Weapon Skill",
        "set" => "Skill Set",
        "group" => "Skill Group",
        _ => "Armor Skill"
    };
    public string LevelControlName => Source == SkillSelectionSource.Required
        ? $"Required level for {Name}" : $"{Source} contribution for {Name}";
    public string RemoveControlName => Source == SkillSelectionSource.Required
        ? $"Remove {Name}" : $"Remove {Source} {Name}";
    public string ProgressText
    {
        get => _progressText;
        set
        {
            if (_progressText == value) return;
            _progressText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressText)));
        }
    }
    public SkillLevelOption[] Options { get; }
    public SkillLevelOption[] LevelOptions { get; }
    public bool IsSelected
    {
        get => _selectedOption.Level > 0;
        set => SelectedOption = value ? (IsSelected ? SelectedOption : LevelOptions[0]) : Options[0];
    }
    public SkillLevelOption SelectedOption
    {
        get => _selectedOption;
        set
        {
            if (value is null || _selectedOption == value) return;
            _selectedOption = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedOption)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public SkillSelectionRow(GameSkill skill, SkillSelectionSource source = SkillSelectionSource.Required)
    {
        Skill = skill;
        Source = source;
        var equipmentBonus = source != SkillSelectionSource.Required && (skill.Kind is "set" or "group");
        var levels = skill.Kind is "set" or "group"
            ? skill.Ranks.Select(r => r.Level).Where(l => l > 0).Distinct().Order().ToArray()
            : [];
        if (levels.Length == 0) levels = Enumerable.Range(1, Math.Max(1, skill.MaxLevel)).ToArray();
        if (equipmentBonus) levels = [1];
        Options = [new(0, skill.Name, "None"), .. levels.Select(level =>
        {
            var pieces = skill.Ranks.FirstOrDefault(r => r.Level == level)?.SetPiecesRequired ?? 0;
            if (equipmentBonus) return new SkillLevelOption(1, "1 piece", "1 piece toward this bonus");
            var display = $"Lv {level}";
            return new SkillLevelOption(level, display, pieces > 0 ? $"{display} · {pieces} pieces" : display);
        })];
        LevelOptions = Options.Skip(1).ToArray();
        _selectedOption = Options[0];
    }

    public SkillValue ToEquipmentSkill() => new()
    {
        Name = Name,
        Kind = Skill.Kind,
        Level = SelectedOption.Level,
        SetPiecesRequired = Skill.Kind is "set" or "group"
            ? Skill.Ranks.Where(rank => rank.SetPiecesRequired > 0).Select(rank => rank.SetPiecesRequired).DefaultIfEmpty(1).Min()
            : 0
    };

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed record SkillLevelOption(int Level, string Display, string MenuLabel)
{
    private static readonly Brush Placeholder = new SolidColorBrush(Color.FromRgb(0x9F, 0xB9, 0xCC));
    private static readonly Brush Selected = new SolidColorBrush(Color.FromRgb(0xE8, 0xEE, 0xF7));
    public Brush TextBrush => Level == 0 ? Placeholder : Selected;
    public override string ToString() => MenuLabel;
}

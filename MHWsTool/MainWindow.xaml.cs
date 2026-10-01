using System.IO;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MHWsTool;

public partial class MainWindow : Window
{
    private readonly Catalog _catalog;
    private readonly Dictionary<string, int> _targets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _weaponSkills = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _talismanSkills = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SkillSelectionRow> _skillRows = [];
    private readonly List<SkillSelectionRow> _weaponSkillRows = [];
    private readonly List<SkillSelectionRow> _talismanSkillRows = [];
    private readonly ObservableCollection<SkillSelectionRow> _selectedSkills = [];
    private readonly ObservableCollection<SkillSelectionRow> _selectedWeaponSkills = [];
    private readonly ObservableCollection<SkillSelectionRow> _selectedTalismanSkills = [];
    private SkillSelectionSource _catalogSource;
    private int _skillColumns = 4;
    private int _settingsRevision;
    private readonly string _customCharmPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WildsForge", "custom-talismans.json");

    public MainWindow()
    {
        InitializeComponent();
        _catalog = Catalog.Load();
        InfoView.Initialize(_catalog);
        LoadCustomCharms();
        _skillRows.AddRange(_catalog.Skills.OrderBy(s => s.Name).Select(s => new SkillSelectionRow(s)));
        _weaponSkillRows.AddRange(_catalog.Skills.OrderBy(s => s.Name).Select(s => new SkillSelectionRow(s, SkillSelectionSource.Weapon)));
        _talismanSkillRows.AddRange(_catalog.Skills.OrderBy(s => s.Name).Select(s => new SkillSelectionRow(s, SkillSelectionSource.Talisman)));
        SelectedSkillsList.ItemsSource = _selectedSkills;
        WeaponSkillsList.ItemsSource = _selectedWeaponSkills;
        TalismanSkillsList.ItemsSource = _selectedTalismanSkills;
        RefreshSkillSections();
        UpdateTargetSummary();
        DataStatus.Text = $"Data snapshot: {_catalog.SnapshotUtc:yyyy-MM-dd} UTC  ·  {_catalog.Armor.Count} armor  ·  {_catalog.Decorations.Count} decorations  ·  {_catalog.Charms.Count} talismans";
        Closed += (_, _) => CalculatorView.Dispose();
    }

    private void SkillFilter_Changed(object sender, TextChangedEventArgs e)
    {
        RefreshSkillSections();
        ShowCatalog();
    }

    private void ShowCatalog()
    {
        if (_catalog is null) return;
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
        {
            if (SkillSectionsList.Items.Count == 0) NoMatchingSkills.BringIntoView();
            else if (SkillSectionsList.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement section)
                section.BringIntoView(new Rect(0, 0, section.ActualWidth, Math.Min(section.ActualHeight, 110)));
        }));
    }

    private void CatalogSource_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { Tag: string tag } || !Enum.TryParse(tag, out _catalogSource)) return;
        RefreshSkillSections();
        if (EquipmentSkillsOptions is not null)
            EquipmentSkillsOptions.IsExpanded = _catalogSource != SkillSelectionSource.Required;
        if (_catalogSource == SkillSelectionSource.Required) PlannerScroll?.ScrollToTop();
        else ShowCatalog();
    }

    private void ManualEquipment_Changed(object sender, RoutedEventArgs e)
    {
        if (_catalog is null) return;
        UpdateTargetSummary();
        InvalidateResults();
    }

    private void SkillSections_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var columns = e.NewSize.Width >= 520 ? 4 : e.NewSize.Width >= 380 ? 3 : e.NewSize.Width >= 240 ? 2 : 1;
        if (_skillColumns == columns) return;
        _skillColumns = columns;
        RefreshSkillSections();
    }

    private void RefreshSkillSections()
    {
        if (_catalog is null || SkillSectionsList is null || SkillFilterBox is null) return;
        var query = SkillFilterBox.Text.Trim();
        var rows = _catalogSource switch
        {
            SkillSelectionSource.Weapon => _weaponSkillRows,
            SkillSelectionSource.Talisman => _talismanSkillRows,
            _ => _skillRows
        };
        var matches = rows.Where(row => row.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        SkillSectionsList.ItemsSource = matches.GroupBy(row => row.Skill.Kind)
            .OrderBy(group => group.Key switch { "weapon" => 0, "armor" => 1, "set" => 2, _ => 3 })
            .Select(group => new SkillSection(group.Key switch
            {
                "weapon" => "Weapon",
                "set" => "Set / Series",
                "group" => "Group",
                _ => "Armor"
            }, group.ToList(), _skillColumns)).ToList();
        NoMatchingSkills.Visibility = matches.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CatalogSkill_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Primitives.ToggleButton { DataContext: SkillSelectionRow row } toggle) return;
        SetRequirement(row, toggle.IsChecked == true
            ? (row.IsSelected ? row.SelectedOption : row.LevelOptions[0]) : row.Options[0]);
    }

    private void RemoveSkill_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: SkillSelectionRow row }) SetRequirement(row, row.Options[0]);
    }

    private void TargetLevel_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox { DataContext: SkillSelectionRow row, SelectedItem: SkillLevelOption option }) return;
        SetRequirement(row, option);
    }

    private void SetRequirement(SkillSelectionRow row, SkillLevelOption option)
    {
        var (levels, selected) = row.Source switch
        {
            SkillSelectionSource.Weapon => (_weaponSkills, _selectedWeaponSkills),
            SkillSelectionSource.Talisman => (_talismanSkills, _selectedTalismanSkills),
            _ => (_targets, _selectedSkills)
        };
        if (levels.GetValueOrDefault(row.Name) == option.Level) return;
        if (option.Level == 0)
        {
            levels.Remove(row.Name);
            selected.Remove(row);
        }
        else
        {
            levels[row.Name] = option.Level;
        }
        row.SelectedOption = option;
        if (option.Level > 0 && !selected.Contains(row)) selected.Add(row);
        if (row.Source != SkillSelectionSource.Required)
        {
            if (row.Source == SkillSelectionSource.Weapon) ManualWeaponBox.IsChecked = true;
            else ManualTalismanBox.IsChecked = true;
            EquipmentSkillsOptions.IsExpanded = true;
        }
        UpdateTargetSummary();
        InvalidateResults();
        if (option.Level > 0)
        {
            var list = row.Source switch
            {
                SkillSelectionSource.Weapon => WeaponSkillsList,
                SkillSelectionSource.Talisman => TalismanSkillsList,
                _ => SelectedSkillsList
            };
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
                (list.ItemContainerGenerator.ContainerFromItem(row) as FrameworkElement)?.BringIntoView()));
        }
    }

    private void UpdateTargetSummary()
    {
        TargetSummary.Text = _targets.Count == 0 ? "No required skills selected" :
            $"{_targets.Count} required skill{(_targets.Count == 1 ? "" : "s")} selected";
        NoSelectedSkills.Visibility = _targets.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        NoWeaponSkills.Visibility = _selectedWeaponSkills.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        NoTalismanSkills.Visibility = _selectedTalismanSkills.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        var request = CreateSearchRequest();
        var progress = SearchEngine.GetRequirementProgress(_catalog, request);
        foreach (var row in _selectedSkills) row.ProgressText = progress[row.Name].Summary;
    }

    private void InvalidateResults(string? message = null)
    {
        _settingsRevision++;
        if (ResultsList is null) return;
        ResultsList.ItemsSource = null;
        ResultCount.Text = "0Sets";
        ResultSummary.Text = message ?? "Settings changed. Search again to update builds.";
        ResultSummary.Visibility = Visibility.Visible;
    }

    private void RankChanged(object sender, RoutedEventArgs e)
    {
        InvalidateResults("Rank changed. Search again to update builds.");
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        if (_targets.Count == 0)
        {
            ResultSummary.Text = "Choose a skill, set or group and its required level, then search.";
            ResultSummary.Visibility = Visibility.Visible;
            return;
        }
        var revision = _settingsRevision;
        var request = CreateSearchRequest();
        SearchButton.IsEnabled = false;
        ResultSummary.Text = "Searching local data…";
        ResultSummary.Visibility = Visibility.Visible;
        ResultsList.ItemsSource = null;
        try
        {
            var builds = await Task.Run(() => SearchEngine.Find(_catalog, request));
            if (revision != _settingsRevision) return;
            ResultsList.ItemsSource = builds.Select((build, index) => new BuildDisplay(index + 1, build, request.Targets,
                request.TalismanSkillsOverride is not null)).ToList();
            ResultCount.Text = $"{builds.Count}Sets";
            ResultSummary.Text = builds.Count == 0 ? "No matching set found in the search shortlist." :
                $"{builds.Count} suggested sets meet all selected skill levels. Highest base defense first.";
            ResultSummary.Visibility = builds.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            if (revision == _settingsRevision)
            {
                ResultSummary.Text = $"Search failed: {ex.Message}";
                ResultSummary.Visibility = Visibility.Visible;
            }
        }
        finally
        {
            SearchButton.IsEnabled = true;
        }
    }

    private SearchRequest CreateSearchRequest() => new()
        {
            Rank = LowRankRadio.IsChecked == true ? "low" : "high",
            Targets = new Dictionary<string, int>(_targets, StringComparer.OrdinalIgnoreCase),
            WeaponSkillsOverride = ManualWeaponBox.IsChecked == true ? _selectedWeaponSkills.Select(row => row.ToEquipmentSkill()).ToList() : null,
            TalismanSkillsOverride = ManualTalismanBox.IsChecked == true ? _selectedTalismanSkills.Select(row => row.ToEquipmentSkill()).ToList() : null
        };

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _targets.Clear();
        _weaponSkills.Clear();
        _talismanSkills.Clear();
        _selectedSkills.Clear();
        _selectedWeaponSkills.Clear();
        _selectedTalismanSkills.Clear();
        foreach (var row in _skillRows.Concat(_weaponSkillRows).Concat(_talismanSkillRows)) row.SelectedOption = row.Options[0];
        HighRankRadio.IsChecked = true;
        ManualWeaponBox.IsChecked = false;
        ManualTalismanBox.IsChecked = false;
        EquipmentSkillsOptions.IsExpanded = false;
        RequiredSourceRadio.IsChecked = true;
        SkillFilterBox.Text = "";
        RefreshSkillSections();
        UpdateTargetSummary();
        PlannerScroll.ScrollToTop();
        InvalidateResults("Choose skills, sets or groups and their required levels, then search for armor sets.");
    }

    private void LoadCustomCharms()
    {
        if (!File.Exists(_customCharmPath)) return;
        try
        {
            var charms = JsonSerializer.Deserialize<List<Charm>>(File.ReadAllText(_customCharmPath), Catalog.JsonOptions) ?? [];
            _catalog.Charms.AddRange(charms.Where(x => x.IsCustom));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not load custom talismans: {ex.Message}", "Wilds Forge", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private sealed record SkillSection(string Title, List<SkillSelectionRow> Skills, int Columns);
}

public sealed class BuildDisplay
{
    public List<ResultArmorItem> ArmorItems { get; }
    public string Charm { get; }
    public List<ResultSkillPill> SkillPills { get; }
    public List<ResultSlotCategory> SlotCategories { get; }
    public Visibility SlotGroupsVisibility { get; }
    public Visibility NoSlotsVisibility { get; }
    public List<ResultDecorationGroup> DecorationGroups { get; }
    public Visibility DecorationGroupsVisibility { get; }
    public Visibility NoDecorationsVisibility { get; }
    public int Defense { get; }
    public List<ResultElementItem> Elements { get; }

    public BuildDisplay(int index, BuildResult result, Dictionary<string, int> targets, bool manualTalisman = false)
    {
        ArmorItems = result.Armor.Select(armor => new ResultArmorItem(armor.Name,
            $"/MHWsTool;component/Assets/results/{armor.Kind}.png", armor.Kind)).ToList();
        Charm = result.Charm?.Name ?? (manualTalisman ? "Manual talisman" : "None");
        SkillPills = result.SkillLabels
            .OrderByDescending(skill => targets.ContainsKey(skill.Key))
            .ThenBy(skill => skill.Value.Contains(':') ? 1 : 0)
            .ThenBy(skill => skill.Key)
            .Select(skill => new ResultSkillPill(skill.Value, targets.ContainsKey(skill.Key), skill.Key))
            .ToList();
        SlotCategories = result.OpenSlotDetails
            .GroupBy(slot => slot.Kind)
            .OrderBy(group => group.Key.Equals("armor", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .Select(category => new ResultSlotCategory(
                $"/MHWsTool;component/Assets/results/slot-category-{category.Key.ToLowerInvariant()}.png",
                $"{category.Key} decoration slots",
                category.GroupBy(slot => slot.Size)
                    .OrderByDescending(group => group.Key)
                    .Select(group => new ResultSlotGroup(
                        $"/MHWsTool;component/Assets/results/slot-{Math.Clamp(group.Key, 1, 4)}.png",
                        $"×{group.Count()}", $"Level {group.Key} {category.Key} slots"))
                    .ToList()))
            .ToList();
        SlotGroupsVisibility = SlotCategories.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        NoSlotsVisibility = SlotCategories.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        DecorationGroups = result.Decorations
            .GroupBy(decoration => new { decoration.Name, decoration.Slot })
            .OrderByDescending(group => group.Key.Slot)
            .ThenBy(group => group.Key.Name)
            .Select(group =>
            {
                var displayName = group.Key.Name.EndsWith($"[{group.Key.Slot}]", StringComparison.Ordinal)
                    ? group.Key.Name
                    : $"{group.Key.Name} [{group.Key.Slot}]";
                return new ResultDecorationGroup(
                    displayName,
                    group.Count() > 1 ? $"×{group.Count()}" : string.Empty,
                    $"{group.Key.Name} · Lv{group.Key.Slot} · {group.Count()} equipped");
            })
            .ToList();
        DecorationGroupsVisibility = DecorationGroups.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        NoDecorationsVisibility = DecorationGroups.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        Defense = result.Defense;
        Elements =
        [
            new("Fire", "/MHWsTool;component/Assets/results/element-fire.png", result.Resistances.Fire),
            new("Water", "/MHWsTool;component/Assets/results/element-water.png", result.Resistances.Water),
            new("Thunder", "/MHWsTool;component/Assets/results/element-thunder.png", result.Resistances.Thunder),
            new("Ice", "/MHWsTool;component/Assets/results/element-ice.png", result.Resistances.Ice),
            new("Dragon", "/MHWsTool;component/Assets/results/element-dragon.png", result.Resistances.Dragon)
        ];
    }
}

public sealed record ResultArmorItem(string Name, string Icon, string Kind);
public sealed record ResultSkillPill(string Text, bool IsRequested, string Label);
public sealed record ResultSlotCategory(string Icon, string Label, List<ResultSlotGroup> Groups);
public sealed record ResultSlotGroup(string Icon, string Count, string Label);
public sealed record ResultDecorationGroup(string Text, string Count, string Label);
public sealed record ResultElementItem(string Name, string Icon, int Value);

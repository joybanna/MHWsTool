using System.Windows;
using System.Windows.Controls;

namespace MHWsTool;

public partial class SkillInfoView : UserControl
{
    private List<SkillInfoEntry> _entries = [];
    private string _category = "all";

    public SkillInfoView() => InitializeComponent();

    public void Initialize(Catalog catalog)
    {
        _entries = catalog.Skills.OrderBy(s => s.Name).Select(s => new SkillInfoEntry(s, catalog)).ToList();
        var detailsDate = catalog.SkillDetailsSnapshotUtc == default ? catalog.SnapshotUtc : catalog.SkillDetailsSnapshotUtc;
        InfoSource.Text = $"Offline effects: Monster Hunter Wilds DB · {detailsDate:yyyy-MM-dd} UTC · English game descriptions. Guide reference: Game8 List of Skills.";
        InfoSource.ToolTip = string.IsNullOrWhiteSpace(catalog.SkillDetailsSource) ? catalog.Source : catalog.SkillDetailsSource;
        RefreshEntries();
    }

    private void Filter_Changed(object sender, TextChangedEventArgs e) => RefreshEntries();

    private void Category_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string category }) _category = category;
        RefreshEntries();
    }

    private void RefreshEntries()
    {
        if (InfoList is null || InfoSearchBox is null || InfoCount is null) return;
        var previous = InfoList.SelectedItem as SkillInfoEntry;
        var query = InfoSearchBox.Text.Trim();
        var matches = _entries.Where(entry => (_category switch
        {
            "skills" => entry.Skill.Kind is "weapon" or "armor",
            "set" => entry.Skill.Kind == "set",
            "group" => entry.Skill.Kind == "group",
            _ => true
        }) && entry.Matches(query)).ToList();
        InfoList.ItemsSource = matches;
        InfoCount.Text = $"{matches.Count} / {_entries.Count} entries";
        InfoNoMatches.Visibility = matches.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        InfoList.SelectedItem = previous is not null && matches.Contains(previous) ? previous : matches.FirstOrDefault();
        if (InfoList.SelectedItem is not null) InfoList.ScrollIntoView(InfoList.SelectedItem);
    }

    private void InfoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (InfoDetails is null || InfoDetailScroll is null || InfoEmpty is null) return;
        var selected = InfoList.SelectedItem as SkillInfoEntry;
        InfoDetails.DataContext = selected;
        InfoDetailScroll.Visibility = selected is null ? Visibility.Collapsed : Visibility.Visible;
        InfoEmpty.Visibility = selected is null ? Visibility.Visible : Visibility.Collapsed;
        InfoDetailScroll.ScrollToTop();
    }
}

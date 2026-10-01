using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MHWsTool;

internal static class DialogTheme
{
    public static readonly Brush Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#172433"));
    public static readonly Brush Foreground = Brushes.White;
    public static Button Button(string title, RoutedEventHandler onClick)
    {
        var button = new Button { Content = title, Padding = new Thickness(12, 7, 12, 7), Margin = new Thickness(4), MinWidth = 86 };
        button.Click += onClick;
        return button;
    }
    public static TextBlock Label(string text) => new() { Text = text, Foreground = Brushes.LightGray, Margin = new Thickness(0, 7, 0, 3) };
}

public sealed class PickerDialog<T> : Window where T : class
{
    private readonly List<T> _items;
    private readonly Func<T, string> _display;
    private readonly TextBox _filter = new() { Margin = new Thickness(0, 8, 0, 8), Padding = new Thickness(8) };
    private readonly ListBox _list = new() { Background = DialogTheme.Background, Foreground = Brushes.White };
    public T? Selected { get; private set; }
    public bool PickNone { get; private set; }

    public PickerDialog(Window owner, string title, IEnumerable<T> items, Func<T, string> display,
        string clearLabel, string? noneLabel = null)
    {
        Owner = owner;
        Title = title;
        Width = 670;
        Height = 600;
        MinWidth = 450;
        MinHeight = 350;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = DialogTheme.Background;
        Foreground = DialogTheme.Foreground;
        _items = items.ToList();
        _display = display;

        var root = new DockPanel { Margin = new Thickness(18) };
        var top = new StackPanel();
        top.Children.Add(new TextBlock { Text = title, FontSize = 19, FontWeight = FontWeights.Bold });
        top.Children.Add(_filter);
        DockPanel.SetDock(top, Dock.Top);
        root.Children.Add(top);

        var bottom = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 9, 0, 0) };
        bottom.Children.Add(DialogTheme.Button(clearLabel, (_, _) => { Selected = null; PickNone = false; DialogResult = true; }));
        if (noneLabel is not null)
            bottom.Children.Add(DialogTheme.Button(noneLabel, (_, _) => { Selected = null; PickNone = true; DialogResult = true; }));
        bottom.Children.Add(DialogTheme.Button("Select", (_, _) => SelectCurrent()));
        DockPanel.SetDock(bottom, Dock.Bottom);
        root.Children.Add(bottom);
        root.Children.Add(_list);
        Content = root;
        _filter.TextChanged += (_, _) => Refresh();
        _list.MouseDoubleClick += (_, _) => SelectCurrent();
        Refresh();
    }

    private void Refresh()
    {
        var query = _filter.Text.Trim();
        _list.ItemsSource = _items.Where(x => _display(x).Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(250).Select(x => new PickerItem<T>(x, _display(x))).ToList();
    }

    private void SelectCurrent()
    {
        if (_list.SelectedItem is not PickerItem<T> selected) return;
        Selected = selected.Value;
        PickNone = false;
        DialogResult = true;
    }

    private sealed record PickerItem<TValue>(TValue Value, string Label)
    {
        public override string ToString() => Label;
    }
}

public sealed class MultiPickerDialog<T> : Window where T : class
{
    private readonly List<T> _items;
    private readonly Func<T, string> _key;
    private readonly Func<T, string> _display;
    private readonly HashSet<string> _selected;
    private readonly TextBox _filter = new() { Margin = new Thickness(0, 8, 0, 8), Padding = new Thickness(8) };
    private readonly ListBox _list = new() { Background = DialogTheme.Background, BorderThickness = new Thickness(0) };
    private readonly TextBlock _count = DialogTheme.Label("");
    public IReadOnlyCollection<string> SelectedKeys => _selected;

    public MultiPickerDialog(Window owner, string title, IEnumerable<T> items, Func<T, string> key,
        Func<T, string> display, IEnumerable<string> selected)
    {
        Owner = owner;
        Title = title;
        Width = 670;
        Height = 600;
        MinWidth = 450;
        MinHeight = 350;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = DialogTheme.Background;
        Foreground = DialogTheme.Foreground;
        _items = items.ToList();
        _key = key;
        _display = display;
        _selected = new HashSet<string>(selected);

        var root = new DockPanel { Margin = new Thickness(18) };
        var top = new StackPanel();
        top.Children.Add(new TextBlock { Text = title, FontSize = 19, FontWeight = FontWeights.Bold });
        top.Children.Add(_filter);
        DockPanel.SetDock(top, Dock.Top);
        root.Children.Add(top);

        var bottom = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
        DockPanel.SetDock(_count, Dock.Left);
        bottom.Children.Add(_count);
        var done = DialogTheme.Button("Done", (_, _) => DialogResult = true);
        DockPanel.SetDock(done, Dock.Right);
        bottom.Children.Add(done);
        DockPanel.SetDock(bottom, Dock.Bottom);
        root.Children.Add(bottom);
        root.Children.Add(_list);
        Content = root;
        _filter.TextChanged += (_, _) => Refresh();
        Refresh();
    }

    private void Refresh()
    {
        var query = _filter.Text.Trim();
        _list.Items.Clear();
        foreach (var item in _items.Where(x => _display(x).Contains(query, StringComparison.OrdinalIgnoreCase)).Take(250))
        {
            var key = _key(item);
            var check = new CheckBox
            {
                Content = _display(item),
                IsChecked = _selected.Contains(key),
                Foreground = Brushes.White,
                Margin = new Thickness(5, 4, 5, 4)
            };
            check.Checked += (_, _) => { _selected.Add(key); UpdateCount(); };
            check.Unchecked += (_, _) => { _selected.Remove(key); UpdateCount(); };
            _list.Items.Add(check);
        }
        UpdateCount();
    }

    private void UpdateCount() => _count.Text = $"{_selected.Count} excluded  ·  search narrows the list";
}

public sealed class CustomCharmDialog : Window
{
    private readonly TextBox _name = new() { Text = "My Appraised Talisman", Padding = new Thickness(8) };
    private readonly ComboBox _skill1 = new() { MinWidth = 280 };
    private readonly ComboBox _skill2 = new() { MinWidth = 280 };
    private readonly ComboBox _level1 = new() { Width = 72 };
    private readonly ComboBox _level2 = new() { Width = 72 };
    private readonly ComboBox[] _slots = [new(), new(), new()];
    private readonly ComboBox[] _slotKinds = [new(), new(), new()];
    public Charm? CreatedCharm { get; private set; }

    public CustomCharmDialog(Window owner, IEnumerable<GameSkill> skills)
    {
        Owner = owner;
        Title = "Add custom talisman";
        Width = 500;
        Height = 520;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = DialogTheme.Background;
        Foreground = DialogTheme.Foreground;
        var choices = skills.Where(s => s.Kind is "armor" or "weapon").OrderBy(s => s.Name).ToList();
        _skill1.ItemsSource = choices;
        _skill2.ItemsSource = choices;
        _level1.ItemsSource = Enumerable.Range(1, 7).ToList();
        _level2.ItemsSource = Enumerable.Range(1, 7).ToList();
        _level1.SelectedIndex = 0;
        _level2.SelectedIndex = 0;
        foreach (var slot in _slots) { slot.Width = 68; slot.ItemsSource = Enumerable.Range(0, 4).ToList(); slot.SelectedIndex = 0; }
        foreach (var kind in _slotKinds) { kind.Width = 79; kind.ItemsSource = new[] { "Armor", "Weapon" }; kind.SelectedIndex = 0; }

        var root = new StackPanel { Margin = new Thickness(22) };
        root.Children.Add(new TextBlock { Text = "CUSTOM TALISMAN", FontSize = 19, FontWeight = FontWeights.Bold });
        root.Children.Add(DialogTheme.Label("Name"));
        root.Children.Add(_name);
        root.Children.Add(DialogTheme.Label("Skill 1 and level"));
        root.Children.Add(SkillRow(_skill1, _level1));
        root.Children.Add(DialogTheme.Label("Skill 2 and level (optional)"));
        root.Children.Add(SkillRow(_skill2, _level2));
        root.Children.Add(DialogTheme.Label("Decoration slots · 0 means no slot"));
        var slotRow = new StackPanel { Orientation = Orientation.Horizontal };
        for (var i = 0; i < _slots.Length; i++)
        {
            var pair = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 8, 0) };
            pair.Children.Add(_slots[i]);
            pair.Children.Add(_slotKinds[i]);
            slotRow.Children.Add(pair);
        }
        root.Children.Add(slotRow);
        root.Children.Add(new TextBlock { Text = "Saved on this computer and available offline.", Foreground = Brushes.LightGray, Margin = new Thickness(0, 20, 0, 5) });
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(DialogTheme.Button("Cancel", (_, _) => DialogResult = false));
        buttons.Children.Add(DialogTheme.Button("Save talisman", (_, _) => Save()));
        root.Children.Add(buttons);
        Content = root;
    }

    private static StackPanel SkillRow(ComboBox skill, ComboBox level)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(skill);
        level.Margin = new Thickness(8, 0, 0, 0);
        row.Children.Add(level);
        return row;
    }

    private void Save()
    {
        if (_skill1.SelectedItem is not GameSkill first || string.IsNullOrWhiteSpace(_name.Text))
        {
            MessageBox.Show(this, "Enter a name and select the first skill.", "Custom talisman", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var values = new List<SkillValue> { new() { Name = first.Name, Kind = first.Kind, Level = Math.Min((int)_level1.SelectedItem, first.MaxLevel) } };
        if (_skill2.SelectedItem is GameSkill second)
        {
            if (second.Name == first.Name)
            {
                MessageBox.Show(this, "Choose two different skills.", "Custom talisman", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            values.Add(new SkillValue { Name = second.Name, Kind = second.Kind, Level = Math.Min((int)_level2.SelectedItem, second.MaxLevel) });
        }
        CreatedCharm = new Charm
        {
            Id = "custom-" + Guid.NewGuid().ToString("N"),
            Name = _name.Text.Trim(),
            IsCustom = true,
            Skills = values,
            Slots = _slots.Where((x, i) => (string)_slotKinds[i].SelectedItem == "Armor")
                .Select(x => (int)x.SelectedItem).Where(x => x > 0).ToList(),
            WeaponSlots = _slots.Where((x, i) => (string)_slotKinds[i].SelectedItem == "Weapon")
                .Select(x => (int)x.SelectedItem).Where(x => x > 0).ToList()
        };
        DialogResult = true;
    }
}

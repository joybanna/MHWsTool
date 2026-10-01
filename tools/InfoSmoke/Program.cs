using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MHWsTool;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var window = new MainWindow();
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.ShowInTaskbar = false;
        window.Left = -10000;
        window.Top = -10000;
        window.Show();
        try
        {
            var tabs = (TabControl)window.FindName("AppTabs");
            var view = (SkillInfoView)window.FindName("InfoView");
            var list = (ListBox)view.FindName("InfoList");
            var search = (TextBox)view.FindName("InfoSearchBox");
            var details = (StackPanel)view.FindName("InfoDetails");
            Layout(window, 1370, 850);
            Check(tabs.SelectedIndex == 0 && tabs.Items.Count == 3, "planner is default and Info is the third tab");
            tabs.SelectedIndex = 2;
            Layout(window, 1370, 850);
            Check(list.Items.Count == 179 && details.DataContext is SkillInfoEntry, "all 179 entries load with a selected detail");
            search.Text = "Attack Boost";
            Check(list.Items.Count == 1 && ((SkillInfoEntry)details.DataContext).Ranks.Count == 5, "skill search displays all five level effects");
            Capture(window, "info-skills.png");
            search.Text = "Arkveld's Hunger";
            ChooseCategory(view, "set");
            Check(list.Items.Count == 1 && ((SkillInfoEntry)details.DataContext).Ranks[1].Requirement == "4 armor pieces", "set detail displays four-piece second tier");
            Capture(window, "info-set.png");
            search.Text = "Fortify";
            ChooseCategory(view, "group");
            Check(list.Items.Count == 1 && ((SkillInfoEntry)details.DataContext).Name == "Fortifying Pelt", "group lookup accepts its activated bonus name");
            Capture(window, "info-group.png");
            search.Text = "no_skill_with_this_name";
            Check(list.Items.Count == 0 && ((TextBlock)view.FindName("InfoNoMatches")).Visibility == Visibility.Visible
                && details.DataContext is null, "empty filters show feedback and clear stale details");
            search.Text = "";
            Check(list.Items.Cast<SkillInfoEntry>().All(e => e.Skill.Kind == "group"), "Group filter contains only group bonuses");
            ChooseCategory(view, "skills");
            Check(list.Items.Count > 0 && list.Items.Cast<SkillInfoEntry>().All(e => e.Skill.Kind is "weapon" or "armor"), "Skills filter combines armor and weapon skills");
            ChooseCategory(view, "all");
            Check(list.Items.Count == 179, "All restores the complete catalog");
            search.Text = "Fortify";
            var before = list.SelectedItem;
            tabs.SelectedIndex = 0;
            tabs.SelectedIndex = 2;
            Check(search.Text == "Fortify" && list.SelectedItem == before, "tab switching preserves info selection and query");
            Layout(window, 1050, 690);
            Capture(window, "info-minimum.png");
            Check(view.ActualWidth > 0 && list.ActualWidth > 200 && details.ActualWidth > 300, "catalog and detail fit the minimum window size");
            Console.WriteLine("Info UI checks passed.");
        }
        finally { window.Close(); app.Shutdown(); }
    }

    private static void ChooseCategory(SkillInfoView view, string category) =>
        Descendants(view).OfType<RadioButton>().Single(r => (string)r.Tag == category).IsChecked = true;

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    private static void Layout(Window window, double width, double height)
    {
        window.Width = width;
        window.Height = height;
        window.Measure(new Size(width, height));
        window.Arrange(new Rect(0, 0, width, height));
        window.UpdateLayout();
    }

    private static void Capture(Window window, string name)
    {
        window.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        var directory = Path.Combine(Environment.CurrentDirectory, "ui-review");
        Directory.CreateDirectory(directory);
        using var stream = File.Create(Path.Combine(directory, name));
        encoder.Save(stream);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException($"FAILED: {message}");
        Console.WriteLine($"PASS: {message}");
    }
}

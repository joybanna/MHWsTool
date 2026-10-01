using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace MHWsTool;

public partial class DamageCalculatorView : UserControl, IDisposable
{
    private const string LocalHost = "calculator.wildsforge.example";
    private WebView2? _browser;
    private bool _initializing;
    private bool _ready;
    private bool _disposed;

    public DamageCalculatorView() => InitializeComponent();

    private async void View_Loaded(object sender, RoutedEventArgs e) => await InitializeAsync();
    private async void Retry_Click(object sender, RoutedEventArgs e) => await InitializeAsync();

    private async Task InitializeAsync()
    {
        if (_initializing || _ready || _disposed) return;
        _initializing = true;
        StatusTitle.Text = "Loading offline calculator…";
        StatusDetail.Text = "Preparing the bundled calculator and local game data.";
        StatusPanel.Visibility = Visibility.Visible;
        RetryButton.Visibility = Visibility.Collapsed;
        try
        {
            var folder = await Task.Run(CalculatorAssets.Prepare);
            if (_disposed) return;
            var profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WildsForge", "CalculatorProfile");
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profile);
            if (_disposed) return;
            _browser = new WebView2 { DefaultBackgroundColor = System.Drawing.Color.FromArgb(0x10, 0x17, 0x22) };
            BrowserHost.Content = _browser;
            // The WebView must be visible to initialize; the status remains visible until navigation completes.
            BrowserHost.Visibility = Visibility.Visible;
            await _browser.EnsureCoreWebView2Async(environment);
            if (_disposed) return;
            var core = _browser.CoreWebView2;
            core.SetVirtualHostNameToFolderMapping(LocalHost, folder, CoreWebView2HostResourceAccessKind.DenyCors);
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.IsZoomControlEnabled = true;
            // All calculator assets use the local virtual host. Prevent any accidental network dependency.
            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            core.WebResourceRequested += (_, args) =>
            {
                if (IsLocalUri(args.Request.Uri)) return;
                args.Response = environment.CreateWebResourceResponse(null, 403, "Offline calculator", "");
            };
            core.NavigationStarting += (_, args) => args.Cancel = !IsLocalUri(args.Uri);
            core.NewWindowRequested += (_, args) => args.Handled = true;
            core.PermissionRequested += (_, args) => args.State = CoreWebView2PermissionState.Deny;
            core.NavigationCompleted += (_, args) =>
            {
                if (_disposed) return;
                if (args.IsSuccess)
                {
                    _ready = true;
                    StatusPanel.Visibility = Visibility.Collapsed;
                }
                else ShowError($"The local calculator could not load ({args.WebErrorStatus}). Try again.");
            };
            core.Navigate($"https://{LocalHost}/calc/index.html");
        }
        catch (WebView2RuntimeNotFoundException)
        {
            ShowError("Microsoft Edge WebView2 Runtime is required. Install it using Microsoft's WebView2 Runtime installer, then click Retry. The calculator works offline after installation.");
        }
        catch (Exception ex)
        {
            if (!_disposed) ShowError($"The offline calculator could not start: {ex.Message}");
        }
        finally { _initializing = false; }
    }

    private static bool IsLocalUri(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps && uri.Host == LocalHost && uri.IsDefaultPort;

    private void ShowError(string detail)
    {
        _ready = false;
        BrowserHost.Visibility = Visibility.Collapsed;
        _browser?.Dispose();
        _browser = null;
        BrowserHost.Content = null;
        StatusTitle.Text = "Calculator unavailable";
        StatusDetail.Text = detail;
        StatusPanel.Visibility = Visibility.Visible;
        RetryButton.Visibility = Visibility.Visible;
    }

    public void Dispose()
    {
        _disposed = true;
        _browser?.Dispose();
        _browser = null;
        BrowserHost.Content = null;
    }
}

using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using MHWsTool;
using Application = System.Windows.Application;
using TabControl = System.Windows.Controls.TabControl;

internal static class Program
{
    [STAThread]
    public static int Main()
    {
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var window = new MainWindow();
        var exitCode = 1;
        window.Loaded += async (_, _) =>
        {
            try
            {
                await Run(window);
                exitCode = 0;
                Console.WriteLine("PASS: offline calculator UI smoke checks");
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); }
            finally { window.Close(); app.Shutdown(); }
        };
        app.Run(window);
        return exitCode;
    }

    private static async Task Run(MainWindow window)
    {
        var tabs = (TabControl)window.FindName("AppTabs");
        var view = (DamageCalculatorView)window.FindName("CalculatorView");
        Check(tabs.SelectedIndex == 0, "planner remains the default tab");
        var host = (ContentControl)view.FindName("BrowserHost");
        Check(host.Content is null, "calculator is loaded only on demand");
        tabs.SelectedIndex = 1;
        WebView2? browser = null;
        for (var i = 0; i < 200; i++)
        {
            await Task.Delay(100);
            browser = host.Content as WebView2;
            if (browser?.CoreWebView2 is not null && (await browser.ExecuteScriptAsync("Boolean(document.querySelector('input[aria-label=Attack]'))")) == "true") break;
        }
        if (browser?.CoreWebView2 is null) throw new Exception(((TextBlock)view.FindName("StatusDetail")).Text);
        var core = browser.CoreWebView2;
        Check(core.Source.StartsWith("https://calculator.wildsforge.example/"), "calculator is served from bundled files");
        await Task.Delay(400);
        var exceptions = new List<string>();
        core.GetDevToolsProtocolEventReceiver("Runtime.exceptionThrown").DevToolsProtocolEventReceived += (_, e) => exceptions.Add(e.ParameterObjectAsJson);
        await core.CallDevToolsProtocolMethodAsync("Runtime.enable", "{}");
        await browser.ExecuteScriptAsync("window.__remoteAssets = []; new PerformanceObserver(list => list.getEntries().forEach(e => { if (e.name.startsWith('http') && !e.name.startsWith(location.origin)) window.__remoteAssets.push(e.name); })).observe({type:'resource',buffered:true});");
        Check(await Evaluate<bool>(browser, "document.querySelectorAll('select[aria-label=Type] option').length === 14"), "all fourteen weapon types are available");
        Check(await Evaluate<bool>(browser, "document.querySelectorAll('tbody tr').length > 10"), "attack damage table renders");
        await browser.ExecuteScriptAsync("window.__stat = label => [...document.querySelectorAll('label')].find(l => l.textContent === label && l.nextElementSibling?.className === 'text-regular')?.nextElementSibling?.textContent; window.__changeNumber = (label,value) => { let e = document.querySelector('input[aria-label=\"'+label+'\"]'); Object.getOwnPropertyDescriptor(HTMLInputElement.prototype,'value').set.call(e,String(value)); e.dispatchEvent(new Event('input',{bubbles:true})); e.dispatchEvent(new Event('change',{bubbles:true})); }; window.__select = (label,text) => { let e = document.querySelector('select[aria-label=\"'+label+'\"]'); e.value = [...e.options].find(o => o.text === text).value; e.dispatchEvent(new Event('change',{bubbles:true})); }; window.__button = text => [...document.querySelectorAll('button')].find(b => b.textContent.trim() === text); ");
        Check(await Evaluate<string>(browser, "window.__stat('Attack')") == "206", "hydration applies custom weapon and Powercharm");
        await browser.ExecuteScriptAsync("window.__changeNumber('Attack',300)");
        await Task.Delay(150);
        Check(await Evaluate<string>(browser, "window.__stat('Attack')") == "306", "attack edits recalculate stats");
        await browser.ExecuteScriptAsync("window.__select('Attack Boost','Attack Boost 5')");
        await Task.Delay(150);
        Check(await Evaluate<string>(browser, "window.__stat('Attack')") == "327", "skill level edits recalculate attack");
        await browser.ExecuteScriptAsync("window.__changeNumber('Affinity',50)");
        await Task.Delay(150);
        Check(await Evaluate<string>(browser, "window.__stat('Effective Affinity')") == "50%", "affinity updates immediately");
        await browser.ExecuteScriptAsync("window.__select('Element','Fire');");
        await Task.Delay(100);
        await browser.ExecuteScriptAsync("window.__changeNumber('Element',300)");
        await Task.Delay(150);
        Check(await Evaluate<string>(browser, "window.__stat('Element')") == "300 Fire", "element is calculated offline");
        await browser.ExecuteScriptAsync("window.__changeNumber('Slash',200)");
        await Task.Delay(100);
        Check(await Evaluate<string>(browser, "document.querySelector('input[aria-label=Slash]').value") == "100", "manual hitzone input respects bounds");

        tabs.SelectedIndex = 0;
        await Task.Delay(150);
        tabs.SelectedIndex = 1;
        await Task.Delay(200);
        Check(ReferenceEquals(browser, host.Content), "tab switches reuse the calculator");
        Check(await Evaluate<string>(browser, "document.querySelector('input[aria-label=Attack]').value") == "300", "calculator inputs survive tab switches");
        await browser.ExecuteScriptAsync("window.__button('Monsters').click()");
        await Task.Delay(150);
        Check(await Evaluate<bool>(browser, "document.querySelector('[role=dialog] tbody tr') !== null"), "monster hitzones are available offline");
        await browser.ExecuteScriptAsync("document.querySelector('[role=dialog] tbody tr').click()");
        await Task.Delay(150);
        Check(await Evaluate<bool>(browser, "!document.querySelector('[role=dialog]')"), "selecting a monster part applies hitzones and closes the dialog");
        await browser.ExecuteScriptAsync("window.__button('Combo Builder').click()");
        await Task.Delay(150);
        await browser.ExecuteScriptAsync("document.querySelector('[role=dialog] tbody tr').click()");
        await Task.Delay(150);
        Check(await Evaluate<string>(browser, "window.__stat('Total Hits')") == "1", "combo builder adds an attack");
        // Close the dialog through its accessible close button, independent of icon implementation.
        await browser.ExecuteScriptAsync("document.querySelector('[role=dialog] button').click()");
        await Task.Delay(150);
        foreach (var type in new[] { "Sword and Shield", "Dual Blades", "Great Sword", "Long Sword", "Hammer", "Hunting Horn", "Lance", "Gunlance", "Switch Axe", "Charge Blade", "Insect Glaive", "Light Bowgun", "Heavy Bowgun", "Bow" })
        {
            await browser.ExecuteScriptAsync($"window.__select('Type',{JsonSerializer.Serialize(type)})");
            await Task.Delay(100);
            Check(await Evaluate<bool>(browser, "document.querySelectorAll('tbody tr').length > 0 && !document.body.innerText.includes('NaN')"), $"{type} renders calculated attacks");
        }
        await browser.ExecuteScriptAsync("window.__select('Type','Sword and Shield')");
        await Task.Delay(150);
        Check(await Evaluate<int>(browser, "window.__remoteAssets.length") == 0, "calculator requests no remote assets");
        Check(exceptions.Count == 0, "no JavaScript runtime exceptions");
        await browser.ExecuteScriptAsync("fetch('https://example.com/unexpected').then(r=>window.__blockedStatus=r.status).catch(()=>window.__blockedStatus=403)");
        await Task.Delay(150);
        Check(await Evaluate<int>(browser, "window.__blockedStatus") == 403, "remote requests are blocked");
        var evidence = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../ui-review"));
        Directory.CreateDirectory(evidence);
        await using (var screenshot = File.Create(Path.Combine(evidence, "offline-calculator.png")))
            await core.CapturePreviewAsync(Microsoft.Web.WebView2.Core.CoreWebView2CapturePreviewImageFormat.Png, screenshot);
        // Render only this window. Overlay WebView2's captured child surface because
        // WPF's RenderTargetBitmap does not include HWND-hosted controls.
        var rendered = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth,
            (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        rendered.Render(window);
        var visual = new System.Windows.Media.DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawImage(rendered, new Rect(0, 0, window.ActualWidth, window.ActualHeight));
            var child = new System.Windows.Media.Imaging.BitmapImage(new Uri(Path.Combine(evidence, "offline-calculator.png")));
            var origin = browser.TransformToAncestor(window).Transform(new System.Windows.Point(0, 0));
            dc.DrawImage(child, new Rect(origin.X, origin.Y, browser.ActualWidth, browser.ActualHeight));
        }
        var combined = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth,
            (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        combined.Render(visual);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(combined));
        using (var output = File.Create(Path.Combine(evidence, "offline-calculator-app.png"))) encoder.Save(output);
        window.Width = window.MinWidth;
        window.Height = window.MinHeight;
        await Task.Delay(200);
        Check(await Evaluate<bool>(browser, "document.documentElement.scrollWidth <= document.documentElement.clientWidth"), "calculator fits the minimum window width");
        Check(await Evaluate<bool>(browser, "document.querySelector('main').scrollHeight > document.querySelector('main').clientHeight"), "smaller windows can scroll to every calculator section");
        await using (var screenshot = File.Create(Path.Combine(evidence, "offline-calculator-minimum.png")))
            await core.CapturePreviewAsync(Microsoft.Web.WebView2.Core.CoreWebView2CapturePreviewImageFormat.Png, screenshot);
        tabs.SelectedIndex = 0;
        Check(((Button)window.FindName("SearchButton")).IsVisible && ((ItemsControl)window.FindName("SkillSectionsList")).Items.Count == 4, "single-page planner remains usable after calculator checks");
    }

    private static async Task<T> Evaluate<T>(WebView2 browser, string js) =>
        JsonSerializer.Deserialize<T>(await browser.ExecuteScriptAsync(js))!;

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception($"FAILED: {message}");
        Console.WriteLine($"PASS: {message}");
    }
}

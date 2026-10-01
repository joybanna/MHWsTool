using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;

namespace MHWsTool;

/// <summary>Unpacks the versioned, embedded static calculator for WebView2.</summary>
public static class CalculatorAssets
{
    public static string Prepare()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("MHWsTool.Data.calculator.zip")
            ?? throw new InvalidOperationException("The bundled calculator is missing. Run tools/Build-Calculator.ps1 and rebuild the app.");
        var hash = Convert.ToHexString(SHA256.HashData(stream));
        stream.Position = 0;
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WildsForge", "Calculator", hash);
        var marker = Path.Combine(root, ".complete");
        if (File.Exists(marker) && File.Exists(Path.Combine(root, "calc", "index.html"))) return root;

        Directory.CreateDirectory(root);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        archive.ExtractToDirectory(root, overwriteFiles: true);
        if (!File.Exists(Path.Combine(root, "calc", "index.html")))
            throw new InvalidOperationException("The bundled calculator does not contain calc/index.html.");
        File.WriteAllText(marker, hash);
        return root;
    }
}

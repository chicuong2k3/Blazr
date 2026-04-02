using System.Text.Json;

namespace Blazr.Services;

internal sealed class ManifestService
{
    private const string ManifestFileName = "blazr.json";

    public BlazrManifest? Load(string projectDirectory)
    {
        var manifestPath = Path.Combine(projectDirectory, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        var json = File.ReadAllText(manifestPath);
        return JsonSerializer.Deserialize<BlazrManifest>(json, SerializerOptions);
    }

    public void MarkInstalled(string projectDirectory, string featureKey)
    {
        var manifest = Load(projectDirectory) ?? new BlazrManifest();

        if (!manifest.InstalledFeatures.Contains(featureKey, StringComparer.OrdinalIgnoreCase))
        {
            manifest.InstalledFeatures.Add(featureKey);
        }

        manifest.InstalledFeatures = manifest.InstalledFeatures
            .OrderBy(static feature => feature, StringComparer.OrdinalIgnoreCase)
            .ToList();
        manifest.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var manifestPath = Path.Combine(projectDirectory, ManifestFileName);
        var json = JsonSerializer.Serialize(manifest, SerializerOptions);
        File.WriteAllText(manifestPath, json + Environment.NewLine);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}

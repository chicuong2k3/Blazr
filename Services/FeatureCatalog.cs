namespace Blazr.Services;

/// <summary>
/// Feature catalog for Blazr CLI - manages and installs various Blazor features.
/// This class is split into multiple partial files for better organization:
/// - FeatureCatalog.cs: Core registration and public API
/// - FeatureCatalog.Styling.cs: Tailwind and Blueprint features
/// - FeatureCatalog.Auth.cs: Authentication feature
/// - FeatureCatalog.Data.cs: PWA and Besql features
/// - FeatureCatalog.Http.cs: HTTP client features (HttpClient, Refit, Flurl)
/// - FeatureCatalog.Testing.cs: Test project feature
/// - FeatureCatalog.Helpers.cs: Shared helper methods
/// </summary>
internal sealed partial class FeatureCatalog
{
    private readonly CommandExecutionService commandExecutionService;
    private readonly TemplateProvider templateProvider;

    public IReadOnlyCollection<FeatureDefinition> GetAll() => _features.Values;

    public IEnumerable<string> GetFeatureKeys() => _features.Keys.OrderBy(static key => key);

    public bool TryGet(string key, out FeatureDefinition feature) =>
        _features.TryGetValue(key, out feature!);

    private readonly Dictionary<string, FeatureDefinition> _features = new(StringComparer.OrdinalIgnoreCase);

    public FeatureCatalog(CommandExecutionService commandExecutionService, TemplateProvider templateProvider)
    {
        this.commandExecutionService = commandExecutionService;
        this.templateProvider = templateProvider;

        _features["tailwind"] = new(
            "tailwind",
            "Adds Tailwind v4 standalone CLI setup for Blazor Blueprint-style utility usage.",
            (context, dryRun, step, totalSteps) => InstallTailwindAsync(context, dryRun, step, totalSteps));
        _features["blueprint"] = new(
            "blueprint",
            "Adds Blazor Blueprint components, icons, services, imports, styles, and portal host wiring.",
            (context, dryRun, step, totalSteps) => InstallBlueprintAsync(context, dryRun, step, totalSteps));
        _features["auth"] = new(
            "auth",
            "Adds a starter authentication state provider and setup notes.",
            (context, dryRun, step, totalSteps) => InstallAuthAsync(context, dryRun, step, totalSteps));
        _features["pwa"] = new(
            "pwa",
            "Adds Bit.Bswup service-worker update progress setup for Blazor PWAs.",
            (context, dryRun, step, totalSteps) => InstallPwaAsync(context, dryRun, step, totalSteps));
        _features["besql"] = new(
            "besql",
            "Adds Bit.Besql starter setup for EF Core + SQLite in the browser.",
            (context, dryRun, step, totalSteps) => InstallBesqlAsync(context, dryRun, step, totalSteps));
        _features["httpclient"] = new(
            "httpclient",
            "Adds a typed HttpClient starter service.",
            (context, dryRun, step, totalSteps) => InstallHttpClientAsync(context, dryRun, step, totalSteps));
        _features["refit"] = new(
            "refit",
            "Adds Refit with a starter API contract and registration notes.",
            (context, dryRun, step, totalSteps) => InstallRefitAsync(context, dryRun, step, totalSteps));
        _features["flurl"] = new(
            "flurl",
            "Adds Flurl.Http with a starter API client service.",
            (context, dryRun, step, totalSteps) => InstallFlurlAsync(context, dryRun, step, totalSteps));
        _features["test"] = new(
            "test",
            "Creates a sibling xUnit test project wired to the target Blazor app.",
            (context, dryRun, step, totalSteps) => InstallTestProjectAsync(context, dryRun, step, totalSteps));
    }
}

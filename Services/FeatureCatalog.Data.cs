namespace Blazr.Services;

internal sealed partial class FeatureCatalog
{
    private async Task<InstallResult> InstallPwaAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Bit.Bswup");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        EnsureImportsWithLines(
            context,
            dryRun,
            operations,
            "@using Bit.Bswup");

        var blazorScript = context.ProjectType == BlazorProjectType.Wasm
            ? "_framework/blazor.webassembly.js"
            : "_framework/blazor.web.js";

        EnsureBlazorAutostartDisabled(context, dryRun, operations, blazorScript);
        EnsureScriptReference(
            context,
            $$"""<script src="_content/Bit.Bswup/bit-bswup.progress.js"></script>""",
            dryRun,
            operations);
        EnsureScriptReference(
            context,
            $$"""<script src="_content/Bit.Bswup/bit-bswup.js" scope="/" log="verbose" sw="service-worker.js" blazorScript="{{blazorScript}}"></script>""",
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "service-worker.js"),
            templateProvider.Load("pwa", "service-worker.js.txt"),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "service-worker.published.js"),
            templateProvider.Load("pwa", "service-worker.published.js.txt"),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            ResolveComponentPath(context.ProjectDirectory, "BswupProgressHost.razor"),
            templateProvider.Load("pwa", "BswupProgressHost.razor.txt"),
            dryRun,
            operations);

        EnsureLayoutHost(context, dryRun, operations, "<BswupProgressHost />");

        nextSteps.Add("Build and publish once to verify `service-worker.published.js` is emitted with the app.");
        nextSteps.Add("If your app root container is not `#app`, update `BswupProgressHost.razor` to match the real selector.");
        nextSteps.Add("Adjust `serverHandledUrls`, `serverRenderedUrls`, and other Bswup settings in `wwwroot/service-worker.js` as your app grows.");

        return InstallResult.Completed(
            "Installed Bit.Bswup PWA starter.",
            operations,
            nextSteps);
    }

    private async Task<InstallResult> InstallBesqlAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Bit.Besql");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        EnsureScriptReference(
            context,
            """<script src="_content/Bit.Besql/bit-besql.js"></script>""",
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Data", "AppDbContext.cs"),
            templateProvider.Load("besql", "AppDbContext.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Data", "TodoItem.cs"),
            templateProvider.Load("besql", "TodoItem.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Data", "ServiceCollectionExtensions.cs"),
            templateProvider.Load("besql", "ServiceCollectionExtensions.cs.txt", context.ProjectName),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Data",
            "builder.Services.Add" + context.ProjectName + "Data();");

        nextSteps.Add("Inject `IDbContextFactory<AppDbContext>` where needed and use it like the standard EF Core SQLite provider.");

        if (context.ProjectType != BlazorProjectType.Wasm)
        {
            nextSteps.Insert(0, "Bit.Besql is mainly intended for Blazor WebAssembly/browser storage. Verify this setup carefully for non-WASM projects.");
        }

        return InstallResult.Completed(
            "Installed Bit.Besql starter.",
            operations,
            nextSteps);
    }
}

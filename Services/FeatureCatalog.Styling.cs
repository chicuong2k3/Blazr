using System.Text;

namespace Blazr.Services;

internal sealed partial class FeatureCatalog
{
    private async Task<InstallResult> InstallTailwindAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "css", "theme.css"),
            templateProvider.Load("tailwind", "theme.css"),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "css", "app-input.css"),
            templateProvider.Load("tailwind", "app-input.css"),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("wwwroot", "css", "app.css"),
            templateProvider.Load("tailwind", "app.css"),
            dryRun,
            operations);

        EnsureBuildTarget(
            context.ProjectFilePath,
            templateProvider.Load("tailwind", "tailwind-target.xml"),
            dryRun,
            operations);

        EnsureStylesheetReference(context, """<link href="css/app.css" rel="stylesheet" />""", dryRun, operations);

        nextSteps.Add("Download `tailwindcss.exe` to the project root as described in the Blazor Blueprint docs.");
        nextSteps.Add("Run `tailwindcss.exe -i wwwroot/css/app-input.css -o wwwroot/css/app.css` once to generate the first stylesheet.");
        nextSteps.Add("Keep `css/app.css` referenced instead of the pre-built Blueprint CSS when you want Tailwind utilities in your own Razor files.");

        return await Task.FromResult(InstallResult.Completed(
            "Installed Tailwind v4 standalone setup.",
            operations,
            nextSteps));
    }

    private async Task<InstallResult> InstallBlueprintAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsureBlueprintPackagesAsync(context, dryRun, operations, step, totalSteps);
        if (!packageResult.Success)
        {
            return packageResult;
        }

        EnsureProgramSetup(context, dryRun, operations);
        EnsureImports(context, dryRun, operations);
        EnsurePortalHost(context, dryRun, operations);
        EnsureStylesheetReference(context, """<link href="css/theme.css" rel="stylesheet" />""", dryRun, operations);

        var prefersCustomTailwind = File.Exists(Path.Combine(context.ProjectDirectory, "wwwroot", "css", "app.css"));
        if (!prefersCustomTailwind)
        {
            EnsureStylesheetReference(
                context,
                """<link href="_content/BlazorBlueprint.Components/blazorblueprint.css" rel="stylesheet" />""",
                dryRun,
                operations);
        }

        nextSteps.Add("If `tailwindcss.exe` is not configured, keep the pre-built Blueprint stylesheet reference.");
        nextSteps.Add("If you installed the `tailwind` feature, keep `css/app.css` as your active stylesheet and regenerate it on build.");
        nextSteps.Add("You can now import and use Blazor Blueprint components in pages and layouts.");

        return InstallResult.Completed(
            "Installed Blazor Blueprint integration.",
            operations,
            nextSteps);
    }

    private Task<InstallResult> EnsureBlueprintPackagesAsync(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        int step,
        int totalSteps)
    {
        return EnsurePackagesAsync(
            context,
            dryRun,
            operations,
            step,
            totalSteps,
            "BlazorBlueprint.Components",
            "BlazorBlueprint.Icons.Lucide");
    }

    private static void EnsureProgramSetup(ProjectContext context, bool dryRun, List<FileOperation> operations)
    {
        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            "BlazorBlueprint.Components",
            "builder.Services.AddBlazorBlueprintComponents();");
    }

    private static void EnsureImports(ProjectContext context, bool dryRun, List<FileOperation> operations)
    {
        EnsureImportsWithLines(
            context,
            dryRun,
            operations,
            "@using BlazorBlueprint.Components",
            "@using BlazorBlueprint.Icons.Lucide.Components",
            "@using BlazorBlueprint.Icons.Lucide.Data");
    }

    private static void EnsurePortalHost(ProjectContext context, bool dryRun, List<FileOperation> operations)
    {
        EnsureLayoutHost(context, dryRun, operations, "<BbPortalHost />");
    }
}

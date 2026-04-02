namespace Blazr.Services;

internal sealed partial class FeatureCatalog
{
    private Task<InstallResult> InstallAuthAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "DemoUser.cs"),
            templateProvider.Load("auth", "DemoUser.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "DemoAuthenticationStateProvider.cs"),
            templateProvider.Load("auth", "DemoAuthenticationStateProvider.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "ServiceCollectionExtensions.cs"),
            templateProvider.Load("auth", "ServiceCollectionExtensions.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Authentication", "README.md"),
            templateProvider.Load("auth", "README.md.txt"),
            dryRun,
            operations);

        nextSteps.Add("Add `using " + context.ProjectName + ".Authentication;` to your `Program.cs`.");
        nextSteps.Add("Call `builder.Services.AddBlazrDemoAuthentication();` during app startup.");
        nextSteps.Add("Wrap your router in `CascadingAuthenticationState` and replace the demo provider when you connect real auth.");

        return Task.FromResult(InstallResult.Completed(
            "Installed authentication starter files.",
            operations,
            nextSteps));
    }
}

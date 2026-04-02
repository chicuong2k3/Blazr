namespace Blazr.Services;

internal sealed partial class FeatureCatalog
{
    private async Task<InstallResult> InstallHttpClientAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ApiClient.cs"),
            templateProvider.Load("httpclient", "ApiClient.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ServiceCollectionExtensions.cs"),
            templateProvider.Load("httpclient", "ServiceCollectionExtensions.cs.txt", context.ProjectName),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Http",
            "builder.Services.Add" + context.ProjectName + "Http(new Uri(\"https://api.example.com/\"));");

        nextSteps.Add("Replace `https://api.example.com/` with your real API base URL.");

        return await Task.FromResult(InstallResult.Completed(
            "Installed built-in HttpClient starter.",
            operations,
            nextSteps));
    }

    private async Task<InstallResult> InstallRefitAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Refit.HttpClientFactory");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "IExampleApi.cs"),
            templateProvider.Load("refit", "IExampleApi.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ServiceCollectionExtensions.cs"),
            templateProvider.Load("refit", "ServiceCollectionExtensions.cs.txt", context.ProjectName),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Http",
            "builder.Services.Add" + context.ProjectName + "Refit(new Uri(\"https://api.example.com/\"));");

        nextSteps.Add("Replace `https://api.example.com/` with your real API base URL.");

        return InstallResult.Completed(
            "Installed Refit starter.",
            operations,
            nextSteps);
    }

    private async Task<InstallResult> InstallFlurlAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();

        var packageResult = await EnsurePackagesAsync(context, dryRun, operations, step, totalSteps, "Flurl.Http");
        if (!packageResult.Success)
        {
            return packageResult;
        }

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ApiClient.cs"),
            templateProvider.Load("flurl", "ApiClient.cs.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            context.ProjectDirectory,
            Path.Combine("Http", "ServiceCollectionExtensions.cs"),
            templateProvider.Load("flurl", "ServiceCollectionExtensions.cs.txt", context.ProjectName),
            dryRun,
            operations);

        EnsureProgramRegistration(
            context,
            dryRun,
            operations,
            context.ProjectName + ".Http",
            "builder.Services.Add" + context.ProjectName + "Flurl(\"https://api.example.com/\");");

        nextSteps.Add("Replace `https://api.example.com/` with your real API base URL.");

        return InstallResult.Completed(
            "Installed Flurl starter.",
            operations,
            nextSteps);
    }
}

namespace Blazr.Services;

internal sealed class FeatureInstaller(
    FeatureCatalog catalog,
    ProjectLocator locator,
    ManifestService manifestService)
{
    public Task<InstallResult> InstallAsync(InstallRequest request) =>
        InstallAsync(request, 1, 1);

    public async Task<InstallResult> InstallAsync(InstallRequest request, int step, int totalSteps)
    {
        if (!catalog.TryGet(request.FeatureKey, out var feature))
        {
            return InstallResult.Failed($"Unknown feature '{request.FeatureKey}'.");
        }

        var resolution = locator.Resolve(request.ProjectPath ?? Directory.GetCurrentDirectory());
        if (!resolution.Success)
        {
            return InstallResult.Failed(resolution.Message);
        }

        var projectName = Path.GetFileNameWithoutExtension(resolution.ProjectFilePath);
        var context = new ProjectContext(
            resolution.ProjectDirectory,
            resolution.ProjectFilePath,
            projectName,
            resolution.ProjectType);

        var result = await feature.InstallAsync(context, request.DryRun, step, totalSteps);
        if (!result.Success || request.DryRun)
        {
            return result;
        }

        manifestService.MarkInstalled(context.ProjectDirectory, feature.Key);
        return result;
    }
}

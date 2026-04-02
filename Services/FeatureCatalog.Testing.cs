namespace Blazr.Services;

internal sealed partial class FeatureCatalog
{
    private Task<InstallResult> InstallTestProjectAsync(ProjectContext context, bool dryRun, int step, int totalSteps)
    {
        var operations = new List<FileOperation>();
        var nextSteps = new List<string>();
        var testProjectName = $"{context.ProjectName}.Tests";
        var testProjectDirectory = Path.Combine(Directory.GetParent(context.ProjectDirectory)?.FullName ?? context.ProjectDirectory, testProjectName);

        WriteFile(
            testProjectDirectory,
            $"{testProjectName}.csproj",
            templateProvider.Load("test", "TestProject.csproj.txt", context.ProjectName),
            dryRun,
            operations);

        WriteFile(
            testProjectDirectory,
            "SmokeTests.cs",
            templateProvider.Load("test", "SmokeTests.cs.txt", context.ProjectName, testProjectName),
            dryRun,
            operations);

        nextSteps.Add($"Run `dotnet restore {testProjectName}` after the project is created.");
        nextSteps.Add($"Add `{testProjectName}/{testProjectName}.csproj` to your solution if you keep a `.sln` file.");
        nextSteps.Add("Add bUnit or Playwright later if you want component or end-to-end test coverage.");

        return Task.FromResult(InstallResult.Completed(
            "Created xUnit starter project.",
            operations,
            nextSteps));
    }
}

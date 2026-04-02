namespace Blazr.Services;

internal sealed record FeatureDefinition(
    string Key,
    string Description,
    Func<ProjectContext, bool, int, int, Task<InstallResult>> InstallAsync);

internal sealed record InstallRequest(string FeatureKey, string? ProjectPath, bool DryRun);

internal enum BlazorProjectType
{
    Unknown,
    WebApp,
    Server,
    Wasm
}

internal sealed record ProjectResolution(
    bool Success,
    string Message,
    string ProjectDirectory,
    string ProjectFilePath,
    BlazorProjectType ProjectType)
{
    public static ProjectResolution Failure(string message) =>
        new(false, message, string.Empty, string.Empty, BlazorProjectType.Unknown);

    public static ProjectResolution SuccessResult(
        string projectDirectory,
        string projectFilePath,
        BlazorProjectType projectType) =>
        new(true, string.Empty, projectDirectory, projectFilePath, projectType);
}

internal sealed record ProjectContext(
    string ProjectDirectory,
    string ProjectFilePath,
    string ProjectName,
    BlazorProjectType ProjectType);

internal sealed record FileOperation(string Kind, string Path);

internal sealed record InstallResult(
    bool Success,
    string Message,
    IReadOnlyList<FileOperation> Operations,
    IReadOnlyList<string> NextSteps)
{
    public static InstallResult Failed(string message) =>
        new(false, message, Array.Empty<FileOperation>(), Array.Empty<string>());

    public static InstallResult Completed(
        string message,
        IReadOnlyList<FileOperation> operations,
        IReadOnlyList<string> nextSteps) =>
        new(true, message, operations, nextSteps);
}

internal sealed class BlazrManifest
{
    public List<string> InstalledFeatures { get; set; } = [];

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

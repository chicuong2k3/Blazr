namespace Blazr.Services;

internal sealed record NewProjectRequest(
    string Name,
    string Framework,
    string RenderMode,
    bool IncludeShowcase,
    string? OutputDirectory,
    bool SkipTemplateInstall,
    bool DryRun,
    string HttpClientLibrary,
    IReadOnlyList<string> SelectedFeatures);

internal sealed record NewProjectResult(
    bool Success,
    string Message,
    IReadOnlyList<string> Commands,
    IReadOnlyList<string> NextSteps)
{
    public static NewProjectResult Failed(string message) =>
        new(false, message, Array.Empty<string>(), Array.Empty<string>());

    public static NewProjectResult Completed(
        string message,
        IReadOnlyList<string> commands,
        IReadOnlyList<string> nextSteps) =>
        new(true, message, commands, nextSteps);
}

internal sealed class BlueprintTemplateService(
    FeatureInstaller featureInstaller,
    CommandExecutionService commandExecutionService)
{
    public async Task<NewProjectResult> CreateAsync(NewProjectRequest request)
    {
        if (!IsValidFramework(request.Framework))
        {
            return NewProjectResult.Failed("Invalid framework. Use net8.0, net9.0, or net10.0.");
        }

        if (!IsValidRenderMode(request.RenderMode))
        {
            return NewProjectResult.Failed("Invalid render mode. Use Server, WebAssembly, or Auto.");
        }

        if (!IsValidHttpClient(request.HttpClientLibrary))
        {
            return NewProjectResult.Failed("Invalid http client. Use none, httpclient, refit, or flurl.");
        }

        var outputDirectory = string.IsNullOrWhiteSpace(request.OutputDirectory)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(request.OutputDirectory);

        var projectPath = Path.Combine(outputDirectory, request.Name);
        var commands = BuildCommands(request, projectPath);
        var totalSteps = commands.Count;
        var currentStep = 0;

        if (request.DryRun)
        {
            return NewProjectResult.Completed(
                "Prepared interactive Blazor project creation plan.",
                commands,
                BuildNextSteps(request.Name, outputDirectory, request.HttpClientLibrary, request.SelectedFeatures));
        }

        Directory.CreateDirectory(outputDirectory);

        if (!request.SkipTemplateInstall)
        {
            currentStep++;
            var install = await commandExecutionService.RunAsync(
                "dotnet",
                "new install BlazorBlueprint.Templates",
                outputDirectory,
                currentStep,
                totalSteps,
                "Installing Blazor Blueprint template");
            if (!install.Success)
            {
                return NewProjectResult.Failed(install.Message);
            }
        }

        currentStep++;
        var create = await commandExecutionService.RunAsync(
            "dotnet",
            $"new blazorblueprint -n {request.Name} -F {request.Framework} -R {request.RenderMode} -I {request.IncludeShowcase.ToString().ToLowerInvariant()}",
            outputDirectory,
            currentStep,
            totalSteps,
            $"Creating project {request.Name}");

        if (!create.Success)
        {
            return NewProjectResult.Failed(create.Message);
        }

        foreach (var feature in request.SelectedFeatures)
        {
            currentStep++;
            var featureResult = await featureInstaller.InstallAsync(
                new InstallRequest(feature, projectPath, false),
                currentStep,
                totalSteps);
            if (!featureResult.Success)
            {
                return NewProjectResult.Failed(
                    $"Project was created, but feature '{feature}' failed: {featureResult.Message}");
            }
        }

        return NewProjectResult.Completed(
            $"Created Blazor Blueprint project '{request.Name}'.",
            commands,
            BuildNextSteps(request.Name, outputDirectory, request.HttpClientLibrary, request.SelectedFeatures));
    }

    private static IReadOnlyList<string> BuildCommands(NewProjectRequest request, string projectPath)
    {
        var commands = new List<string>();

        if (!request.SkipTemplateInstall)
        {
            commands.Add("dotnet new install BlazorBlueprint.Templates");
        }

        commands.Add(
            $"dotnet new blazorblueprint -n {request.Name} -F {request.Framework} -R {request.RenderMode} -I {request.IncludeShowcase.ToString().ToLowerInvariant()}");

        foreach (var feature in request.SelectedFeatures)
        {
            commands.Add($"blazr add {feature} --project {projectPath}");
        }

        return commands;
    }

    private static IReadOnlyList<string> BuildNextSteps(
        string projectName,
        string outputDirectory,
        string httpClientLibrary,
        IReadOnlyList<string> selectedFeatures)
    {
        var projectPath = Path.Combine(outputDirectory, projectName);
        var steps = new List<string>
        {
            $"cd {projectPath}"
        };

        if (selectedFeatures.Contains("tailwind", StringComparer.OrdinalIgnoreCase))
        {
            steps.Add("Download `tailwindcss.exe` into the project root before the first build.");
        }

        if (httpClientLibrary is "httpclient" or "refit" or "flurl")
        {
            steps.Add("Review the generated `Http` folder and wire the service registration in `Program.cs`.");
        }

        steps.Add("dotnet run");
        return steps;
    }

    private static bool IsValidFramework(string framework) =>
        framework is "net8.0" or "net9.0" or "net10.0";

    private static bool IsValidRenderMode(string renderMode) =>
        renderMode is "Server" or "WebAssembly" or "Auto";

    private static bool IsValidHttpClient(string value) =>
        value is "none" or "httpclient" or "refit" or "flurl";
}

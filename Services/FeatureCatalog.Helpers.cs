using System.Text;

namespace Blazr.Services;

internal sealed partial class FeatureCatalog
{
    private async Task<InstallResult> EnsurePackagesAsync(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        int step,
        int totalSteps,
        params string[] packages)
    {
        foreach (var package in packages)
        {
            operations.Add(new FileOperation("package", package));

            if (dryRun)
            {
                continue;
            }

            var result = await commandExecutionService.RunAsync(
                "dotnet",
                $"add \"{context.ProjectFilePath}\" package {package}",
                context.ProjectDirectory,
                step,
                totalSteps,
                $"Installing package {package}");

            if (!result.Success)
            {
                return InstallResult.Failed($"Package install failed for '{package}': {result.Message}");
            }
        }

        return InstallResult.Completed("Packages installed.", operations, Array.Empty<string>());
    }

    private static void EnsureStylesheetReference(
        ProjectContext context,
        string linkMarkup,
        bool dryRun,
        List<FileOperation> operations)
    {
        var appPath = FindFirstExistingFile(
            context.ProjectDirectory,
            "App.razor",
            Path.Combine("Components", "App.razor"));

        if (appPath is not null)
        {
            var content = File.ReadAllText(appPath);
            if (content.Contains(linkMarkup, StringComparison.Ordinal))
            {
                return;
            }

            var updated = linkMarkup + Environment.NewLine + content;
            operations.Add(new FileOperation("update", appPath));

            if (!dryRun)
            {
                File.WriteAllText(appPath, updated, Encoding.UTF8);
            }

            return;
        }

        var indexPath = FindFirstExistingFile(context.ProjectDirectory, Path.Combine("wwwroot", "index.html"));
        if (indexPath is null)
        {
            return;
        }

        var indexContent = File.ReadAllText(indexPath);
        if (indexContent.Contains(linkMarkup, StringComparison.Ordinal))
        {
            return;
        }

        var updatedIndex = indexContent.Replace(
            "</head>",
            $"    {linkMarkup}{Environment.NewLine}</head>",
            StringComparison.OrdinalIgnoreCase);

        operations.Add(new FileOperation("update", indexPath));

        if (!dryRun)
        {
            File.WriteAllText(indexPath, updatedIndex, Encoding.UTF8);
        }
    }

    private static void EnsureScriptReference(
        ProjectContext context,
        string scriptMarkup,
        bool dryRun,
        List<FileOperation> operations)
    {
        foreach (var path in GetHostDocumentPaths(context))
        {
            var fullPath = Path.Combine(context.ProjectDirectory, path);
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var content = File.ReadAllText(fullPath);
            if (content.Contains(scriptMarkup, StringComparison.Ordinal))
            {
                return;
            }

            var updated = content.TrimEnd() + Environment.NewLine + scriptMarkup + Environment.NewLine;
            operations.Add(new FileOperation("update", fullPath));
            if (!dryRun)
            {
                File.WriteAllText(fullPath, updated, Encoding.UTF8);
            }

            return;
        }
    }

    private static void EnsureBlazorAutostartDisabled(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        string blazorScriptPath)
    {
        foreach (var path in GetHostDocumentPaths(context))
        {
            var fullPath = Path.Combine(context.ProjectDirectory, path);
            if (!File.Exists(fullPath))
            {
                continue;
            }

            var content = File.ReadAllText(fullPath);
            var source = $"""<script src="{blazorScriptPath}"></script>""";
            var target = $"""<script src="{blazorScriptPath}" autostart="false"></script>""";

            if (!content.Contains(source, StringComparison.Ordinal) || content.Contains(target, StringComparison.Ordinal))
            {
                continue;
            }

            operations.Add(new FileOperation("update", fullPath));
            if (!dryRun)
            {
                File.WriteAllText(fullPath, content.Replace(source, target, StringComparison.Ordinal), Encoding.UTF8);
            }

            return;
        }
    }

    private static IReadOnlyList<string> GetHostDocumentPaths(ProjectContext context)
    {
        return context.ProjectType switch
        {
            BlazorProjectType.Server => new[]
            {
                Path.Combine("Pages", "_Host.cshtml"),
                "_Host.cshtml"
            },
            _ => new[]
            {
                Path.Combine("wwwroot", "index.html"),
                "App.razor",
                Path.Combine("Components", "App.razor")
            }
        };
    }

    private static void EnsureBuildTarget(
        string projectFilePath,
        string targetXml,
        bool dryRun,
        List<FileOperation> operations)
    {
        var content = File.ReadAllText(projectFilePath);
        if (content.Contains("Name=\"BuildTailwindCSS\"", StringComparison.Ordinal))
        {
            return;
        }

        var projectEndTag = "</Project>";
        var updated = content.Replace(projectEndTag, Environment.NewLine + targetXml + Environment.NewLine + projectEndTag, StringComparison.Ordinal);

        operations.Add(new FileOperation("update", projectFilePath));
        if (!dryRun)
        {
            File.WriteAllText(projectFilePath, updated, Encoding.UTF8);
        }
    }

    private static void EnsureProgramRegistration(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        string usingDirective,
        string registrationLine)
    {
        var programPath = Path.Combine(context.ProjectDirectory, "Program.cs");
        if (!File.Exists(programPath))
        {
            return;
        }

        var content = File.ReadAllText(programPath);
        var updated = content;

        updated = EnsureUsingDirective(updated, usingDirective);
        updated = EnsureLineNearBuild(updated, registrationLine);

        if (updated == content)
        {
            return;
        }

        operations.Add(new FileOperation("update", programPath));
        if (!dryRun)
        {
            File.WriteAllText(programPath, updated, Encoding.UTF8);
        }
    }

    private static void EnsureImportsWithLines(
        ProjectContext context,
        bool dryRun,
        List<FileOperation> operations,
        params string[] lines)
    {
        var importsPath = FindFirstExistingFile(
            context.ProjectDirectory,
            "_Imports.razor",
            Path.Combine("Components", "_Imports.razor"));

        if (importsPath is null)
        {
            importsPath = Path.Combine(context.ProjectDirectory, "_Imports.razor");
        }

        var content = File.Exists(importsPath) ? File.ReadAllText(importsPath) : string.Empty;
        var updated = content;

        foreach (var line in lines)
        {
            updated = EnsureAppendedLine(updated, line);
        }

        if (updated == content)
        {
            return;
        }

        operations.Add(new FileOperation(File.Exists(importsPath) ? "update" : "create", importsPath));
        if (!dryRun)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(importsPath)!);
            File.WriteAllText(importsPath, updated, Encoding.UTF8);
        }
    }

    private static void EnsureLayoutHost(ProjectContext context, bool dryRun, List<FileOperation> operations, string markup)
    {
        var layoutPath = FindFirstExistingFile(
            context.ProjectDirectory,
            Path.Combine("Components", "Layout", "MainLayout.razor"),
            Path.Combine("Shared", "MainLayout.razor"),
            "MainLayout.razor");

        if (layoutPath is null)
        {
            return;
        }

        var content = File.ReadAllText(layoutPath);
        if (content.Contains(markup, StringComparison.Ordinal))
        {
            return;
        }

        var updated = content.TrimEnd() + Environment.NewLine + Environment.NewLine + markup + Environment.NewLine;
        operations.Add(new FileOperation("update", layoutPath));

        if (!dryRun)
        {
            File.WriteAllText(layoutPath, updated, Encoding.UTF8);
        }
    }

    private static string ResolveComponentPath(string projectDirectory, string fileName)
    {
        var componentsShared = Path.Combine(projectDirectory, "Components", "Shared");
        if (Directory.Exists(componentsShared))
        {
            return Path.Combine("Components", "Shared", fileName);
        }

        var shared = Path.Combine(projectDirectory, "Shared");
        if (Directory.Exists(shared))
        {
            return Path.Combine("Shared", fileName);
        }

        return fileName;
    }

    private static void WriteFile(
        string rootDirectory,
        string relativePath,
        string content,
        bool dryRun,
        List<FileOperation> operations)
    {
        var fullPath = Path.Combine(rootDirectory, relativePath);
        operations.Add(new FileOperation(File.Exists(fullPath) ? "update" : "create", fullPath));

        if (dryRun)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, NormalizeLineEndings(content), Encoding.UTF8);
    }

    private static string? FindFirstExistingFile(string rootDirectory, params string[] relativePaths)
    {
        foreach (var relativePath in relativePaths)
        {
            var fullPath = Path.Combine(rootDirectory, relativePath);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static string EnsureUsingDirective(string content, string usingDirective)
    {
        var line = $"using {usingDirective};";
        if (content.Contains(line, StringComparison.Ordinal))
        {
            return content;
        }

        return line + Environment.NewLine + content;
    }

    private static string EnsureLineAfterMatch(string content, string match, string lineToInsert)
    {
        if (content.Contains(lineToInsert, StringComparison.Ordinal))
        {
            return content;
        }

        var index = content.IndexOf(match, StringComparison.Ordinal);
        if (index < 0)
        {
            return content;
        }

        var insertionIndex = content.IndexOf(';', index);
        if (insertionIndex < 0)
        {
            return content;
        }

        insertionIndex += 1;
        return content.Insert(insertionIndex, Environment.NewLine + lineToInsert);
    }

    private static string EnsureLineNearBuild(string content, string lineToInsert)
    {
        if (content.Contains(lineToInsert, StringComparison.Ordinal))
        {
            return content;
        }

        const string buildLine = "var app = builder.Build();";
        var buildIndex = content.IndexOf(buildLine, StringComparison.Ordinal);
        if (buildIndex >= 0)
        {
            return content.Insert(buildIndex, lineToInsert + Environment.NewLine);
        }

        return EnsureLineAfterMatch(content, "builder.Services.AddRazorComponents()", lineToInsert);
    }

    private static string EnsureAppendedLine(string content, string line)
    {
        if (content.Contains(line, StringComparison.Ordinal))
        {
            return content;
        }

        if (!string.IsNullOrWhiteSpace(content) && !content.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            content += Environment.NewLine;
        }

        return content + line + Environment.NewLine;
    }

    private static string NormalizeLineEndings(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", Environment.NewLine, StringComparison.Ordinal);
}

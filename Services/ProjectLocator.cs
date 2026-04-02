using System.Xml.Linq;

namespace Blazr.Services;

internal sealed class ProjectLocator
{
    public ProjectResolution Resolve(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);

        if (File.Exists(fullPath))
        {
            if (!fullPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                return ProjectResolution.Failure("The provided file is not a .csproj.");
            }

            return ResolveProject(fullPath);
        }

        if (!Directory.Exists(fullPath))
        {
            return ProjectResolution.Failure($"Project path '{inputPath}' does not exist.");
        }

        var projectFiles = Directory.GetFiles(fullPath, "*.csproj", SearchOption.TopDirectoryOnly);
        if (projectFiles.Length == 0)
        {
            return ProjectResolution.Failure("No .csproj file was found in the target directory.");
        }

        if (projectFiles.Length > 1)
        {
            return ProjectResolution.Failure("Multiple .csproj files were found. Pass `--project` with the exact project file.");
        }

        return ResolveProject(projectFiles[0]);
    }

    private static ProjectResolution ResolveProject(string projectFilePath)
    {
        var projectDirectory = Path.GetDirectoryName(projectFilePath)!;
        var projectType = DetectProjectType(projectFilePath, projectDirectory);

        return ProjectResolution.SuccessResult(projectDirectory, projectFilePath, projectType);
    }

    private static BlazorProjectType DetectProjectType(string projectFilePath, string projectDirectory)
    {
        try
        {
            var projectDocument = XDocument.Load(projectFilePath);
            var root = projectDocument.Root;
            if (root is null)
            {
                return BlazorProjectType.Unknown;
            }

            var sdk = root.Attribute("Sdk")?.Value ?? string.Empty;
            var targetFrameworks = root
                .Descendants()
                .Where(static element => element.Name.LocalName is "TargetFramework" or "TargetFrameworks")
                .Select(static element => element.Value)
                .ToArray();

            var packageReferences = root
                .Descendants()
                .Where(static element => element.Name.LocalName == "PackageReference")
                .Select(static element => element.Attribute("Include")?.Value ?? string.Empty)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var usesWebSdk = sdk.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase);
            var usesBlazorWasmSdk = sdk.Contains("Microsoft.NET.Sdk.BlazorWebAssembly", StringComparison.OrdinalIgnoreCase);
            var targetsBrowser = targetFrameworks.Any(static framework =>
                framework.Contains("-browser", StringComparison.OrdinalIgnoreCase));
            var hasWasmPackage = packageReferences.Contains("Microsoft.AspNetCore.Components.WebAssembly")
                || packageReferences.Contains("Microsoft.AspNetCore.Components.WebAssembly.DevServer");

            if (usesBlazorWasmSdk || targetsBrowser || hasWasmPackage)
            {
                return BlazorProjectType.Wasm;
            }

            if (usesWebSdk)
            {
                if (HasWebAppMarkers(projectDirectory))
                {
                    return BlazorProjectType.WebApp;
                }

                if (HasServerMarkers(projectDirectory))
                {
                    return BlazorProjectType.Server;
                }
            }
        }
        catch
        {
            return BlazorProjectType.Unknown;
        }

        return BlazorProjectType.Unknown;
    }

    private static bool HasWebAppMarkers(string projectDirectory)
    {
        return File.Exists(Path.Combine(projectDirectory, "Components", "Routes.razor"))
            || File.Exists(Path.Combine(projectDirectory, "Components", "App.razor"))
            || File.Exists(Path.Combine(projectDirectory, "Components", "_Imports.razor"));
    }

    private static bool HasServerMarkers(string projectDirectory)
    {
        return File.Exists(Path.Combine(projectDirectory, "_Host.cshtml"))
            || File.Exists(Path.Combine(projectDirectory, "Pages", "_Host.cshtml"))
            || File.Exists(Path.Combine(projectDirectory, "Shared", "MainLayout.razor"));
    }
}
